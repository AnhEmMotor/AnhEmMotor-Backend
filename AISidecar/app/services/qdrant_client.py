import hashlib
import warnings

from langchain_google_genai import GoogleGenerativeAIEmbeddings
from qdrant_client import AsyncQdrantClient
from qdrant_client.http import models as qm

from app.config import get_settings

warnings.filterwarnings(
    "ignore", message="Api key is used with an insecure connection.", category=UserWarning)

PRODUCT_COLLECTION = "product_catalog"
KNOWLEDGE_COLLECTION = "knowledge_base"
PLAN_TEMPLATE_COLLECTION = "plan_templates"
INDEXED_COLLECTIONS = {PRODUCT_COLLECTION, KNOWLEDGE_COLLECTION, PLAN_TEMPLATE_COLLECTION}

VECTOR_SIZE = 768
SCORE_THRESHOLD = 0.55
INGEST_BATCH_SIZE = 100

PRODUCT_PAYLOAD_INDEXES = [
    ("brand_id", qm.PayloadSchemaType.INTEGER),
    ("category_id", qm.PayloadSchemaType.INTEGER),
    ("price", qm.PayloadSchemaType.FLOAT),
    ("in_stock", qm.PayloadSchemaType.BOOL),
    ("is_active", qm.PayloadSchemaType.BOOL),
]

_client: AsyncQdrantClient | None = None
_embedding_cache: dict[str, list[float]] = {}


def get_client() -> AsyncQdrantClient:
    global _client
    if _client is None:
        settings = get_settings()
        _client = AsyncQdrantClient(
            url=settings.qdrant_url, api_key=settings.qdrant_api_key or None, check_compatibility=False)
    return _client


def reset_client() -> None:
    global _client
    _client = None


async def ensure_collections() -> None:
    client = get_client()
    for name in (PRODUCT_COLLECTION, KNOWLEDGE_COLLECTION, PLAN_TEMPLATE_COLLECTION):
        if not await client.collection_exists(name):
            await client.create_collection(
                name, vectors_config=qm.VectorParams(size=VECTOR_SIZE, distance=qm.Distance.COSINE))
    for field, schema in PRODUCT_PAYLOAD_INDEXES:
        await client.create_payload_index(PRODUCT_COLLECTION, field, schema)


async def embed(text: str) -> list[float]:
    key = hashlib.sha256(text.encode()).hexdigest()
    cached = _embedding_cache.get(key)
    if cached is not None:
        return cached
    settings = get_settings()
    embedder = GoogleGenerativeAIEmbeddings(
        model=f"models/{settings.embedding_model}", google_api_key=settings.ai_api_key)
    vector = await embedder.aembed_query(text)
    _embedding_cache[key] = vector
    return vector


def build_product_text(p: dict) -> str:
    parts = [
        p.get("name") or "",
        f"Thương hiệu {p['brand']}" if p.get("brand") else "",
        f"Danh mục {p['category']}" if p.get("category") else "",
        f"Loại xe {p['vehicleType']}" if p.get("vehicleType") else "",
        f"Màu {', '.join(p['colors'])}" if p.get("colors") else "",
        (p.get("description") or "")[:800],
    ]
    return ". ".join(x for x in parts if x)


async def upsert_products(items: list[dict], collection: str = PRODUCT_COLLECTION) -> None:
    client = get_client()
    for start in range(0, len(items), INGEST_BATCH_SIZE):
        batch = items[start:start + INGEST_BATCH_SIZE]
        vectors = [await embed(build_product_text(p)) for p in batch]
        points = [
            qm.PointStruct(id=p["productId"], vector=vector, payload=p)
            for p, vector in zip(batch, vectors)
        ]
        await client.upsert(collection_name=collection, points=points)


async def delete_products(product_ids: list[str]) -> None:
    client = get_client()
    await client.delete(
        collection_name=PRODUCT_COLLECTION,
        points_selector=qm.PointIdsList(points=product_ids),
    )


async def index_knowledge(documents: list[dict]) -> None:
    client = get_client()
    vectors = [await embed(doc["content"]) for doc in documents]
    points = [
        qm.PointStruct(id=doc["chunkId"], vector=vector, payload=doc)
        for doc, vector in zip(documents, vectors)
    ]
    await client.upsert(collection_name=KNOWLEDGE_COLLECTION, points=points)


async def search_products(query: str, max_price: int | None = None,
                           in_stock_only: bool = True, limit: int = 8) -> list[dict]:
    must = [qm.FieldCondition(key="is_active", match=qm.MatchValue(value=True))]
    if in_stock_only:
        must.append(qm.FieldCondition(key="in_stock", match=qm.MatchValue(value=True)))
    if max_price:
        must.append(qm.FieldCondition(key="price", range=qm.Range(lte=max_price)))

    vector = await embed(query)
    client = get_client()
    result = await client.query_points(
        collection_name=PRODUCT_COLLECTION,
        query=vector,
        query_filter=qm.Filter(must=must),
        limit=min(limit, 15),
        score_threshold=SCORE_THRESHOLD,
    )
    return [{"productId": hit.payload.get("productId"), "score": hit.score} for hit in result.points]


async def search_knowledge(query: str, limit: int = 5) -> list[dict]:
    vector = await embed(query)
    client = get_client()
    result = await client.query_points(
        collection_name=KNOWLEDGE_COLLECTION,
        query=vector,
        limit=min(limit, 15),
        score_threshold=SCORE_THRESHOLD,
    )
    return [
        {
            "citationId": f"c{i + 1}",
            "sourceFile": hit.payload.get("sourceFile"),
            "heading": hit.payload.get("heading"),
            "content": hit.payload.get("content"),
        }
        for i, hit in enumerate(result.points)
    ]


PLAN_TEMPLATE_SCORE_THRESHOLD = 0.90


async def upsert_plan_template(template_id: str, canonical_question: str, module: str,
                                required_tools: list[str], required_permissions: list[str],
                                status: str = "active") -> None:
    vector = await embed(canonical_question)
    client = get_client()
    await client.upsert(collection_name=PLAN_TEMPLATE_COLLECTION, points=[
        qm.PointStruct(id=template_id, vector=vector, payload={
            "templateId": template_id,
            "canonicalQuestion": canonical_question,
            "module": module,
            "requiredTools": required_tools,
            "requiredPermissions": required_permissions,
            "status": status,
        }),
    ])


async def find_similar_plan_template(question: str, module: str) -> dict | None:
    vector = await embed(question)
    client = get_client()
    result = await client.query_points(
        collection_name=PLAN_TEMPLATE_COLLECTION,
        query=vector,
        query_filter=qm.Filter(must=[
            qm.FieldCondition(key="module", match=qm.MatchValue(value=module)),
            qm.FieldCondition(key="status", match=qm.MatchValue(value="active")),
        ]),
        limit=1,
        score_threshold=PLAN_TEMPLATE_SCORE_THRESHOLD,
    )
    if not result.points:
        return None
    return {"templateId": result.points[0].payload.get("templateId"), "score": result.points[0].score}


async def reindex_products(items: list[dict], expected_count: int) -> dict:
    client = get_client()
    staging = f"{PRODUCT_COLLECTION}_v2"
    if await client.collection_exists(staging):
        await client.delete_collection(staging)
    await client.create_collection(
        staging, vectors_config=qm.VectorParams(size=VECTOR_SIZE, distance=qm.Distance.COSINE))
    for field, schema in PRODUCT_PAYLOAD_INDEXES:
        await client.create_payload_index(staging, field, schema)
    await upsert_products(items, collection=staging)

    actual_count = (await client.count(staging)).count
    if actual_count != expected_count:
        return {"aliasSwitched": False, "stagingCount": actual_count, "expectedCount": expected_count}

    await client.update_collection_aliases(
        change_aliases_operations=[
            qm.DeleteAliasOperation(delete_alias=qm.DeleteAlias(alias_name=PRODUCT_COLLECTION)),
            qm.CreateAliasOperation(
                create_alias=qm.CreateAlias(collection_name=staging, alias_name=PRODUCT_COLLECTION)),
        ]
    )
    return {"aliasSwitched": True, "stagingCount": actual_count, "expectedCount": expected_count}

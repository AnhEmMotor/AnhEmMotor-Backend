import hashlib
import warnings

from langchain_google_genai import GoogleGenerativeAIEmbeddings
from qdrant_client import AsyncQdrantClient
from qdrant_client.http import models as qm

from app.config import get_settings

warnings.filterwarnings(
    "ignore", message="Api key is used with an insecure connection.", category=UserWarning)

PLAN_TEMPLATE_COLLECTION = "plan_templates"
INDEXED_COLLECTIONS = {PLAN_TEMPLATE_COLLECTION}

VECTOR_SIZE = 768

_client: AsyncQdrantClient | None = None
_embedding_cache: dict[str, list[float]] = {}


def rag_enabled() -> bool:
    settings = get_settings()
    return bool(settings.qdrant_url) and settings.rag_enabled


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
    if not await client.collection_exists(PLAN_TEMPLATE_COLLECTION):
        await client.create_collection(
            PLAN_TEMPLATE_COLLECTION, vectors_config=qm.VectorParams(size=VECTOR_SIZE, distance=qm.Distance.COSINE))


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

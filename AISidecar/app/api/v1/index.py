from fastapi import APIRouter, Depends

from app.api.deps import verify_internal_secret
from app.services import qdrant_client as qc
from app.tools.knowledge import rag_enabled

router = APIRouter(prefix="/internal/index")

_SKIPPED = {"skipped": "rag_disabled"}


@router.post("/products")
async def index_products(payload: dict, _: str = Depends(verify_internal_secret)):
    if not rag_enabled():
        return _SKIPPED
    items = payload.get("items") or []
    await qc.upsert_products(items)
    return {"indexed": len(items)}


@router.post("/products/delete")
async def delete_products(payload: dict, _: str = Depends(verify_internal_secret)):
    if not rag_enabled():
        return _SKIPPED
    product_ids = payload.get("productIds") or []
    await qc.delete_products(product_ids)
    return {"deleted": len(product_ids)}


@router.post("/knowledge")
async def index_knowledge(payload: dict, _: str = Depends(verify_internal_secret)):
    if not rag_enabled():
        return _SKIPPED
    documents = payload.get("documents") or []
    await qc.index_knowledge(documents)
    return {"indexed": len(documents)}


@router.post("/rebuild")
async def rebuild_index(payload: dict, _: str = Depends(verify_internal_secret)):
    if not rag_enabled():
        return _SKIPPED
    collection = payload.get("collection")
    if collection != qc.PRODUCT_COLLECTION:
        return {"error": f"Không hỗ trợ reindex collection '{collection}'"}
    items = payload.get("items") or []
    return await qc.reindex_products(items, expected_count=len(items))

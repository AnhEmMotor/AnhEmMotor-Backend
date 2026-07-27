from fastapi import APIRouter

router = APIRouter()


@router.get("/")
def read_root():
    return {"status": "ok", "message": "AI Sidecar is running"}


@router.get("/health")
def health():
    return {"status": "ok"}

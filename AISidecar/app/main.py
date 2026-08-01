import logging
from contextlib import asynccontextmanager

from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse

from app.core.errors import SidecarError
from app.core.logging import setup_logging
from app.api.v1 import health, chat, search_products, admin, store_chat
from app.services.backend_client import BackendClient
from app.tools.registry import verify_tool_contract

logger = logging.getLogger(__name__)


@asynccontextmanager
async def lifespan(application: FastAPI):
    setup_logging()
    logger.info("AI Sidecar started")
    await verify_tool_contract(BackendClient(""))
    yield
    logger.info("AI Sidecar shutting down")


app = FastAPI(lifespan=lifespan)

app.include_router(health.router)
app.include_router(chat.router)
app.include_router(search_products.router)
app.include_router(admin.router)
app.include_router(store_chat.router)


@app.exception_handler(SidecarError)
async def sidecar_error_handler(request: Request, exc: SidecarError):
    logger.exception("SidecarError: %s", exc)
    return JSONResponse(
        status_code=500,
        content={"detail": exc.user_message},
    )

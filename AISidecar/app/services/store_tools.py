import json
import logging
from pathlib import Path

from langchain_core.tools import StructuredTool
from pydantic import BaseModel, Field, ValidationError, create_model

from app.schemas.tool_envelope import ChatToolEnvelope
from app.services.backend_client import BackendClient

logger = logging.getLogger(__name__)

SOLUTION_FILE_NAME = "AnhEmMotor-Backend.sln"
CATALOG_RELATIVE_PATH = Path("SharedConfig") / "chat-tools-catalog.store.json"

_ARG_TYPES = {"string": str, "integer": int}


def _find_repo_root() -> Path:
    directory = Path(__file__).resolve().parent
    while directory != directory.parent:
        if (directory / SOLUTION_FILE_NAME).exists():
            return directory
        directory = directory.parent
    raise FileNotFoundError(f"Không tìm thấy {SOLUTION_FILE_NAME} ở thư mục cha nào của {__file__}.")


def load_store_catalog() -> list[dict]:
    path = _find_repo_root() / CATALOG_RELATIVE_PATH
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def _build_schema(tool_name: str, args: list[dict]) -> type[BaseModel]:
    fields = {}
    for arg in args:
        python_type = _ARG_TYPES[arg["type"]]
        default = ... if arg.get("required") else arg.get("default")
        fields[arg["name"]] = (python_type, Field(default, description=arg["description"]))
    return create_model(f"{tool_name}_Input", **fields)


def build_store_tools(backend_client: BackendClient, session_id: str = "") -> list[StructuredTool]:
    tools = []
    for entry in load_store_catalog():
        path = entry["path"]
        is_write = entry.get("is_write", False)
        schema = _build_schema(entry["name"], entry["args"])

        async def _call(_path=path, _name=entry["name"], _is_write=is_write, **kwargs):
            payload = {k: v for k, v in kwargs.items() if v != ""}
            if _is_write:
                payload["session_id"] = session_id
            raw = await backend_client.call_public_tool(_path, payload)
            try:
                envelope = ChatToolEnvelope.model_validate(raw)
            except ValidationError as exc:
                logger.error("Tool %s trả envelope không hợp lệ: %s", _name, exc)
                return {"error": f"Dữ liệu trả về từ tool '{_name}' không đúng định dạng, không thể dùng."}
            return envelope.model_dump(mode="json")

        tools.append(StructuredTool.from_function(
            coroutine=_call,
            name=entry["name"],
            description=entry["description"],
            args_schema=schema,
        ))
    return tools


STORE_TOOL_NAMES = frozenset(entry["name"] for entry in load_store_catalog())
IS_WRITE_BY_NAME = {entry["name"]: entry.get("is_write", False) for entry in load_store_catalog()}

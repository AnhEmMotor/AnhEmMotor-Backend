import json
import logging
from pathlib import Path

from langchain_core.tools import StructuredTool
from pydantic import BaseModel, Field, ValidationError, create_model

from app.config import get_settings
from app.services.backend_client import BackendClient
from app.tools.envelope import ChatToolEnvelope

logger = logging.getLogger(__name__)

SOLUTION_FILE_NAME = "AnhEmMotor-Backend.sln"
CATALOG_RELATIVE_PATH = Path("SharedConfig") / "chat-tools-catalog.json"

_ARG_TYPES = {"string": str, "integer": int}
_SUMMARY_EXCLUDED_ARGS = {"from_date", "to_date"}


def _find_repo_root() -> Path:
    directory = Path(__file__).resolve().parent
    while directory != directory.parent:
        if (directory / SOLUTION_FILE_NAME).exists():
            return directory
        directory = directory.parent
    raise FileNotFoundError(f"Không tìm thấy {SOLUTION_FILE_NAME} ở thư mục cha nào của {__file__}.")


def load_catalog() -> list[dict]:
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


def describe_args(tool_name: str, args: dict) -> str:
    catalog_by_name = {entry["name"]: entry for entry in load_catalog()}
    entry = catalog_by_name.get(tool_name)
    arg_labels = {a["name"]: a["label"] for a in entry["args"]} if entry else {}
    parts = [
        f"{arg_labels.get(key, key)}: {value}"
        for key, value in args.items()
        if value not in (None, "") and key not in _SUMMARY_EXCLUDED_ARGS
    ]
    return ", ".join(parts)


def build_tools(backend_client: BackendClient, allowed_names: set[str] | None = None) -> list[StructuredTool]:
    tools = []
    for entry in load_catalog():
        if allowed_names is not None and entry["name"] not in allowed_names:
            continue
        path = entry["path"]
        schema = _build_schema(entry["name"], entry["args"])

        async def _call(_path=path, _name=entry["name"], **kwargs):
            payload = {k: v for k, v in kwargs.items() if v != ""}
            raw = await backend_client.call_tool(_path, payload)
            try:
                envelope = ChatToolEnvelope.model_validate(raw)
            except ValidationError as exc:
                logger.error("Tool %s trả envelope không hợp lệ: %s", _name, exc)
                return {"error": f"Dữ liệu trả về từ tool '{_name}' không đúng định dạng, không thể dùng."}
            if get_settings().tool_flags.get(_name) == "shadow":
                logging.getLogger("chat_tools.shadow").info(
                    "tool=%s payload=%s response=%s", _name, payload, raw)
            return envelope.model_dump(mode="json")

        tools.append(StructuredTool.from_function(
            coroutine=_call,
            name=entry["name"],
            description=entry["description"],
            args_schema=schema,
        ))
    return tools

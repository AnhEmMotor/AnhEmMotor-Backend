import json
import logging
from pathlib import Path
from typing import Callable

from langchain_core.tools import StructuredTool
from pydantic import BaseModel, Field, ValidationError, create_model

from app.config import get_settings
from app.services.backend_client import BackendClient
from app.schemas.tool_envelope import ChatToolEnvelope
from app.tools.knowledge import KNOWLEDGE_TOOL_NAMES, build_knowledge_tools

logger = logging.getLogger(__name__)

CATALOG_RELATIVE_PATH = Path("SharedConfig") / "chat-tools-catalog.json"

_ARG_TYPES = {"string": str, "integer": int}
_SUMMARY_EXCLUDED_ARGS = {"from_date", "to_date"}


def _find_repo_root() -> Path:
    directory = Path(__file__).resolve().parent
    while directory != directory.parent:
        if (directory / CATALOG_RELATIVE_PATH).exists():
            return directory
        directory = directory.parent
    raise FileNotFoundError(f"Không tìm thấy {CATALOG_RELATIVE_PATH} ở thư mục cha nào của {__file__}.")


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


def _catalog_label(tool_name: str) -> str:
    entry = next((e for e in load_catalog() if e["name"] == tool_name), None)
    return entry["label"] if entry else tool_name


def _generic_summary(name: str, result: dict) -> str:
    label = _catalog_label(name)
    items = result.get("items")
    if not isinstance(items, list) or not items:
        return f"{label}: đã hoàn tất"
    total = result.get("totalCount", len(items))
    if total == 1:
        scalars = [f"{k}: {v}" for k, v in items[0].items() if not isinstance(v, (dict, list))][:3]
        if scalars:
            return f"{label} — {', '.join(scalars)}"
    return f"{label}: {total} kết quả"


SUMMARIZERS: dict[str, Callable[[dict], str]] = {
    "get_sales_summary": lambda r: (
        f"Doanh thu {sum(i.get('totalRevenue', 0) for i in r.get('items', [])):,.0f} ₫ "
        f"· {r.get('totalCount', 0)} ngày"
    ),
    "get_product_stock": lambda r: (
        f"Tồn kho {sum(i.get('stockQuantity', 0) for i in r.get('items', []))} "
        f"({len(r.get('items', []))} biến thể)"
    ),
    "get_pnl_report": lambda r: (
        f"Lợi nhuận ròng {sum(i.get('netProfit', 0) for i in r.get('items', [])):,.0f} ₫ "
        f"({r.get('totalCount', 0)} kỳ)"
    ),
    "get_order_status": lambda r: (
        f"{r.get('totalCount', 0)} đơn hàng"
        + (f", mới nhất: {r['items'][0].get('statusId', 'không rõ')}" if r.get("items") else "")
    ),
}


def summarize_result(name: str, result: dict) -> str:
    if not isinstance(result, dict) or result.get("error"):
        return "Không lấy được dữ liệu"
    fn = SUMMARIZERS.get(name)
    return fn(result) if fn else _generic_summary(name, result)


def build_tools(backend_client: BackendClient, allowed_names: set[str] | None = None) -> list[StructuredTool]:
    tools = []
    for entry in load_catalog():
        if entry["name"] in KNOWLEDGE_TOOL_NAMES:
            continue
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


def build_all_tools(backend_client: BackendClient, allowed_names: set[str] | None = None) -> list[StructuredTool]:
    return build_tools(backend_client, allowed_names) + build_knowledge_tools(backend_client, allowed_names)

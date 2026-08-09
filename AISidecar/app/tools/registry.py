import difflib
import hashlib
import json
import logging
from dataclasses import dataclass, field
from typing import Literal

from app.config import get_settings
from app.core.llm import get_llm
from app.services.chat_tools import load_catalog

logger = logging.getLogger(__name__)

MAX_TOOLS_PER_MODULE = 10
MAX_TOOLS_PER_REQUEST = 20
PINNED_TOOLS = frozenset()

DEFAULT_MODULES_ON_ROUTER_FAILURE = ["product", "sales"]

_locally_disabled: set[str] = set()


@dataclass(frozen=True)
class ToolSpec:
    name: str
    module: str
    required_permissions: tuple[str, ...] = field(default_factory=tuple)
    is_write: bool = False
    version: int = 1
    status: Literal["active", "deprecated", "removed"] = "active"
    replaced_by: str | None = None
    since: str = ""


def load_tool_specs() -> dict[str, ToolSpec]:
    specs = {}
    for entry in load_catalog():
        status = "removed" if entry["name"] in _locally_disabled else (entry.get("status") or "active")
        specs[entry["name"]] = ToolSpec(
            name=entry["name"],
            module=entry.get("module") or "",
            required_permissions=tuple(entry.get("required_permissions") or ()),
            is_write=bool(entry.get("is_write")),
            version=int(entry.get("version") or 1),
            status=status,
            replaced_by=entry.get("replaced_by"),
            since=entry.get("since") or "",
        )
    return specs


async def verify_tool_contract(backend_client) -> dict:
    settings = get_settings()
    specs = load_tool_specs()
    local_active = {name for name, s in specs.items() if s.status == "active"}
    try:
        manifest = await backend_client.get_tool_manifest()
    except Exception as exc:
        logger.warning("Không lấy được tool manifest từ backend: %s", exc)
        return {"missing_backend": [], "missing_spec": [], "stale_build": False}

    backend_tools = set(manifest.get("tools") or [])
    missing_backend = sorted(local_active - backend_tools)
    missing_spec = sorted(backend_tools - local_active)

    if missing_backend:
        logger.error("Tool chưa có endpoint ở backend: %s", missing_backend)
        _locally_disabled.update(missing_backend)
    if missing_spec:
        logger.warning("Endpoint chưa được khai báo ToolSpec: %s", missing_spec)

    stale_build = bool(settings.expected_build_id) and manifest.get("buildId") != settings.expected_build_id
    if stale_build:
        logger.error("Sidecar đang chạy build cũ: expected=%s, backend=%s",
                      settings.expected_build_id, manifest.get("buildId"))

    return {"missing_backend": missing_backend, "missing_spec": missing_spec, "stale_build": stale_build}


def registry_fingerprint(specs: dict[str, ToolSpec] | None = None) -> str:
    specs = specs if specs is not None else load_tool_specs()
    payload = sorted(
        (s.name, s.version, sorted(s.required_permissions))
        for s in specs.values() if s.status == "active"
    )
    return hashlib.sha256(json.dumps(payload, default=str).encode()).hexdigest()[:16]


def filter_by_permission(specs: dict[str, ToolSpec], permissions: list[str]) -> list[ToolSpec]:
    granted = set(permissions or [])
    allowed = [
        s for s in specs.values()
        if s.status == "active" and set(s.required_permissions).issubset(granted)
    ]
    logger.info("Đã cấp %d/%d tool cho user", len(allowed), len(specs))
    return allowed


def select_tools_for_request(allowed: list[ToolSpec], modules: list[str],
                              expanded_modules: set[str] | None = None,
                              run_id: str | None = None) -> list[ToolSpec]:
    expanded_modules = expanded_modules or set()
    router_set = set(modules[:2])

    scoped = [s for s in allowed if s.name in PINNED_TOOLS
              or s.module in router_set or s.module in expanded_modules]

    def priority(spec: ToolSpec) -> int:
        if spec.name in PINNED_TOOLS:
            return 2
        if spec.module in router_set:
            return 0
        return 1

    scoped.sort(key=lambda s: (priority(s), s.module, s.name))

    if len(scoped) > MAX_TOOLS_PER_REQUEST:
        dropped = [s.name for s in scoped[MAX_TOOLS_PER_REQUEST:]]
        logger.warning("Vượt trần tool: nạp %d/%d, đã bỏ %s",
                        MAX_TOOLS_PER_REQUEST, len(scoped), dropped, extra={"run_id": run_id})
        scoped = scoped[:MAX_TOOLS_PER_REQUEST]

    return scoped


def resolve_tool_call_error(name: str, state: dict, specs: dict[str, ToolSpec] | None = None) -> dict | None:
    specs = specs if specs is not None else load_tool_specs()
    spec = specs.get(name)
    allowed_names = state.get("allowed_tool_names") or set()

    if spec is None:
        suggestion = difflib.get_close_matches(name, list(allowed_names), n=1)
        hint = f" Có phải bạn muốn dùng '{suggestion[0]}'?" if suggestion else ""
        return {"kind": "tool_not_found", "message": (
            f"Không có tool tên '{name}'. Chỉ dùng các tool trong danh sách được cung cấp.{hint} "
            "Nếu không có tool phù hợp, hãy nói rõ với người dùng là bạn chưa hỗ trợ việc này.")}

    if spec.status == "removed":
        if spec.replaced_by:
            return {"kind": "tool_removed",
                    "message": f"Tool '{name}' không còn dùng. Hãy dùng '{spec.replaced_by}'."}
        return {"kind": "tool_removed", "message": f"Tool '{name}' đã bị loại bỏ. Không có tool thay thế."}

    flags_snapshot = state.get("tool_flags_snapshot") or {}
    if flags_snapshot.get(name) == "off":
        return {"kind": "tool_not_available", "message": (
            f"Tool '{name}' hiện đang tắt. Hãy nói với người dùng bạn chưa hỗ trợ việc này.")}

    if name in allowed_names:
        return None

    scoped_modules = set(state.get("scoped_modules") or [])
    expanded_modules = set(state.get("expanded_modules") or [])
    if spec.module and spec.module not in scoped_modules and spec.module not in expanded_modules:
        return {"kind": "module_expand", "module": spec.module}

    return {"kind": "tool_not_available", "message": (
        f"Bạn không có quyền dùng '{name}'. Hãy nói với người dùng rằng họ không có quyền truy cập "
        "thông tin này. KHÔNG đoán dữ liệu.")}


def build_tool_scope(state: dict) -> list[ToolSpec]:
    specs = load_tool_specs()
    allowed = filter_by_permission(specs, state.get("permissions") or [])

    modules = list(state.get("scoped_modules") or [])
    expanded = set(state.get("expanded_modules") or set())

    names_from_plan = set()
    if step := state.get("current_plan_step"):
        names_from_plan = set(step.get("expectedTools") or [])

    scoped = select_tools_for_request(allowed, modules, expanded, run_id=state.get("run_id"))
    if names_from_plan:
        plan_tools = [s for s in allowed if s.name in names_from_plan]
        pinned_tools = [s for s in allowed if s.name in PINNED_TOOLS]
        scoped = plan_tools + [t for t in pinned_tools if t not in plan_tools]

    return scoped


async def infer_step_tools(step_text: str, allowed: list[ToolSpec]) -> list[str]:
    catalog_text = "\n".join(f"- {s.name}" for s in allowed)
    prompt = (
        f"Chọn tối đa 3 tool phù hợp nhất cho bước công việc sau, CHỈ chọn trong danh sách.\n"
        f"Bước: {step_text}\nDanh sách tool:\n{catalog_text}\n"
        "Trả về JSON list tên tool, ví dụ [\"tool_a\", \"tool_b\"]."
    )
    llm = get_llm(temperature=0)
    response = await llm.ainvoke(prompt)
    text = response if isinstance(response, str) else getattr(response, "text", "")
    try:
        names = json.loads(text)
    except (ValueError, TypeError):
        names = []
    allowed_names = {s.name for s in allowed}
    return [n for n in names if isinstance(n, str) and n in allowed_names][:3]

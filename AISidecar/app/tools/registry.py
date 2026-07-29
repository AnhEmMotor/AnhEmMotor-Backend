import json
import logging
from dataclasses import dataclass, field

from app.core.llm import get_llm
from app.services.chat_tools import load_catalog

logger = logging.getLogger(__name__)

MAX_TOOLS_PER_MODULE = 10
MAX_TOOLS_PER_REQUEST = 20
PINNED_TOOLS = frozenset({"search_knowledge"})

DEFAULT_MODULES_ON_ROUTER_FAILURE = ["product", "sales"]


@dataclass(frozen=True)
class ToolSpec:
    name: str
    module: str
    required_permissions: tuple[str, ...] = field(default_factory=tuple)
    is_write: bool = False


def load_tool_specs() -> dict[str, ToolSpec]:
    specs = {}
    for entry in load_catalog():
        specs[entry["name"]] = ToolSpec(
            name=entry["name"],
            module=entry.get("module") or "",
            required_permissions=tuple(entry.get("required_permissions") or ()),
            is_write=bool(entry.get("is_write")),
        )
    return specs


def filter_by_permission(specs: dict[str, ToolSpec], permissions: list[str]) -> list[ToolSpec]:
    granted = set(permissions or [])
    allowed = [s for s in specs.values() if set(s.required_permissions).issubset(granted)]
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
    text = response if isinstance(response, str) else getattr(response, "content", "")
    try:
        names = json.loads(text)
    except (ValueError, TypeError):
        names = []
    allowed_names = {s.name for s in allowed}
    return [n for n in names if isinstance(n, str) and n in allowed_names][:3]

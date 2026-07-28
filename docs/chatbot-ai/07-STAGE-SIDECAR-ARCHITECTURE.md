# Stage 7 — Tái cấu trúc AI Sidecar

> Yêu cầu #7 · Ưu tiên: 🔴 Cao · Ước lượng: 1–2 ngày · Phụ thuộc: **Stage 1**
> Mục tiêu: cấu trúc rõ ràng, dễ bảo trì — **làm trước khi thêm bất kỳ tính năng nào**.

Sidecar hiện chỉ 195 dòng. Sau khi thêm agent, tool, Qdrant, plan mode, guardrail... nó sẽ
lên 3.000–5.000 dòng. Refactor bây giờ tốn 1 ngày; refactor sau tốn 1 tuần.

---

## 7.1. Vấn đề của cấu trúc hiện tại

```
AISidecar/
  main.py                              # 19 dòng
  dependencies.py                      # auth
  requirements.txt                     # không pin version
  controllers/
    manager_chat_controller.py         # controller kiêm luôn HTTP client, prompt, streaming
    search_controller.py               # định nghĩa Pydantic model + prompt + chain ngay trong file
    test_controller.py
  schemas/chat_schemas.py
  services/llm_factory.py
```

| Vấn đề | Hệ quả |
|---|---|
| Controller ôm cả business logic, HTTP client, prompt | Không test được từng phần |
| `search_controller.py` khởi tạo `llm` và `chain` ở **module level** | Env chưa sẵn sàng lúc import → dính `FakeListLLM` vĩnh viễn; không đổi được config runtime |
| Prompt là string rải rác trong code | Sửa prompt phải sửa code, không review được |
| Config đọc `os.environ.get(...)` rải rác 6 chỗ | Không validate, gõ sai tên biến không ai biết |
| `except Exception: pass` | Lỗi bị nuốt hoàn toàn |
| Không có logging | Không debug được production |
| `requirements.txt` không pin version | Deploy hôm nay khác hôm qua |
| Không có test | Sửa gì cũng sợ |

---

## 7.2. Cấu trúc đích

```
AISidecar/
  app/
    __init__.py
    main.py                      # chỉ tạo FastAPI app + include router + lifespan
    config.py                    # Settings (pydantic-settings) — NGUỒN SỰ THẬT DUY NHẤT cho env

    api/
      __init__.py
      deps.py                    # dependency: verify_internal_secret, get_settings, get_auth_header
      v1/
        __init__.py
        chat.py                  # POST /manager-chat, /manager-chat/generate-title
        search.py                # POST /search
        health.py                # GET /, GET /health
        admin.py                 # endpoint debug — chỉ bật khi ENABLE_TEST_ENDPOINTS

    core/
      __init__.py
      llm.py                     # LLM factory (thay llm_factory.py cũ)
      logging.py                 # cấu hình structlog / logging chuẩn
      errors.py                  # exception nội bộ + exception handler của FastAPI
      redaction.py               # che thông tin nhạy cảm (Stage 11)

    prompts/
      __init__.py
      loader.py                  # đọc & render file .md, có cache
      system_manager_chat.md     # system prompt chính — SỬA PROMPT KHÔNG CẦN SỬA CODE
      system_plan_mode.md        # Stage 10
      title_generation.md
      search_intent.md

    agents/
      __init__.py
      state.py                   # định nghĩa AgentState (LangGraph)
      manager_agent.py           # graph chính của Manager Chat
      nodes.py                   # các node: route, call_model, call_tools, summarize
      checkpointer.py            # LangGraph checkpointer (Stage 8)

    tools/
      __init__.py
      registry.py                # build_tools(auth, permissions) — nơi duy nhất khai báo tool
      base.py                    # helper chung: gọi backend, xử lý lỗi, đo thời gian
      products.py                # search_products, get_product_stock, get_low_stock
      orders.py                  # get_order_status
      analytics.py               # get_sales_summary, get_top_selling
      knowledge.py               # tìm kiếm Qdrant (Stage 12)

    guardrails/
      __init__.py
      input_guard.py             # kiểm tra input user (Stage 13)
      tool_guard.py              # kiểm tra tool call trước khi thực thi (Stage 13)
      output_guard.py            # kiểm tra output trước khi trả

    services/
      __init__.py
      backend_client.py          # HTTP client duy nhất gọi .NET (context + tools)
      qdrant_client.py           # Stage 12
      embedding.py               # Stage 12

    schemas/
      __init__.py
      chat.py                    # ChatRequest, GenerateTitleRequest, StreamEvent
      search.py                  # SearchIntent
      plan.py                    # Stage 10
      events.py                  # định nghĩa các loại event stream

  tests/                         # ĐÃ TỒN TẠI từ Stage 1.6.2 — Stage này cập nhật, không tạo mới
    conftest.py                  # cập nhật: import app.main thay vì main
    test_dependencies.py         # từ Stage 1 — cập nhật import
    test_llm_factory.py          # từ Stage 1 — chuyển sang app.core.llm
    test_config_contract.py      # từ Stage 1 — trỏ sang app/config.py
    test_module_level_init.py    # từ Stage 1 — trỏ sang app/api/v1/search.py
    test_config.py               # MỚI: Settings đọc đúng env, validate đúng
    test_prompts.py              # MỚI: loader render placeholder, thiếu file thì lỗi rõ
    test_errors.py               # MỚI: exception trả user_message, không lộ chi tiết
    test_backend_client.py       # MỚI: dùng respx mock, 403 → ForbiddenError

  pyproject.toml                 # thay requirements.txt
  requirements.txt               # sinh ra từ pyproject, có pin version
  requirements-dev.txt           # ĐÃ TỒN TẠI từ Stage 1.6.2 — chuyển vào pyproject [dev]
  pytest.ini                     # ĐÃ TỒN TẠI từ Stage 1.6.2
  README.md                      # cách chạy standalone để debug
```

> ⚠️ **Refactor này làm hỏng toàn bộ test Python viết ở Stage 1.** Đây là phụ thuộc bắt buộc,
> không phải việc dọn dẹp tuỳ chọn — xem mục 7.10.

---

## 7.3. `app/config.py` — nguồn sự thật cho config

Thay toàn bộ `os.environ.get(...)` rải rác:

```python
from functools import lru_cache
from typing import Literal
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    # --- Kết nối backend .NET ---
    backend_url: str = "http://localhost:5000/api"
    backend_internal_secret: str = ""

    # --- LLM ---
    ai_provider: Literal["gemini", "apiendpoint"] = "gemini"
    api_key: str = ""
    ai_api_endpoint: str = ""
    model: str = "gemini-3.5-flash"           # GIỮ NGUYÊN — xem Stage 1.2

    fast_model: str = "gemini-3.5-flash"      # Stage 14.4 — router, phân loại, sinh title

    # --- Vận hành ---
    port: int = 8000
    environment: Literal["Development", "Staging", "Production"] = "Development"
    enable_test_endpoints: bool = False
    request_timeout_seconds: float = 15.0

    # --- Hiển thị suy nghĩ (Stage 11) ---
    # BA mức, không phải bool — xem Stage 11.2
    tool_detail_level: Literal["Full", "Summary", "Minimal"] = "Full"

    # --- Qdrant (Stage 12) ---
    qdrant_url: str = ""
    qdrant_api_key: str = ""
    embedding_model: str = "text-embedding-004"
    rag_enabled: bool = False                 # Stage 12.2 — tắt là chatbot vẫn chạy bằng SQL

    # --- Checkpointer LangGraph (Stage 10.6) ---
    postgres_url: str = ""                    # rỗng → dùng MemorySaver

    # --- Cache plan (Stage 19) ---
    plan_cache_enabled: bool = False

    # --- Cờ tool (Stage 16.8, 17.6) ---
    # {"get_pnl_report": "canary", "get_payroll_summary": "off"}
    tool_flags: dict[str, Literal["off", "shadow", "canary", "full"]] = {}
    tool_kill_switch: tuple[str, ...] = ()    # áp NGAY, kể cả run đang chạy (17.6)

    @property
    def backend_base(self) -> str:
        """URL gốc của backend (bỏ hậu tố /api)."""
        return self.backend_url.rstrip("/").removesuffix("/api")

    @property
    def is_production(self) -> bool:
        return self.environment == "Production"


@lru_cache
def get_settings() -> Settings:
    return Settings()
```

**Backend .NET phải truyền thêm env** trong `Infrastructure/Services/Ai/AiSidecarManager.cs`:

```csharp
startInfo.EnvironmentVariables["ENVIRONMENT"] = env.EnvironmentName;
startInfo.EnvironmentVariables["ENABLE_TEST_ENDPOINTS"] = env.IsDevelopment() ? "true" : "false";
startInfo.EnvironmentVariables["FAST_MODEL"] = config["AISetup:FastModel"] ?? "";
// BA mức, không phải bool — mặc định Summary ở Production (Stage 11.2)
startInfo.EnvironmentVariables["TOOL_DETAIL_LEVEL"] =
    config["AISetup:ToolDetailLevel"] ?? (env.IsProduction() ? "Summary" : "Full");
startInfo.EnvironmentVariables["QDRANT_URL"] = config["AISetup:QdrantUrl"] ?? "";
startInfo.EnvironmentVariables["QDRANT_API_KEY"] = config["AISetup:QdrantApiKey"] ?? "";
startInfo.EnvironmentVariables["RAG_ENABLED"] = config["AISetup:RagEnabled"] ?? "false";
startInfo.EnvironmentVariables["PLAN_CACHE_ENABLED"] = config["AISetup:PlanCacheEnabled"] ?? "false";
startInfo.EnvironmentVariables["POSTGRES_URL"] = config.GetConnectionString("PostgreSql") ?? "";
startInfo.EnvironmentVariables["TOOL_FLAGS"] =
    JsonSerializer.Serialize(config.GetSection("AISetup:ToolFlags").Get<Dictionary<string, string>>() ?? new());
```
(inject `IWebHostEnvironment env` vào constructor của `AiSidecarManager`)

### `appsettings.json` sau Stage này

```jsonc
"AISetup": {
    "AiProvider": "Gemini",
    "AiApiEndpoint": "",
    "ApiKey": "",
    "Model": "gemini-3.5-flash",       // GIỮ NGUYÊN — Stage 1.2
    "FastModel": "gemini-3.5-flash",   // Stage 14.4
    "EmbeddingModel": "text-embedding-004",
    "ToolDetailLevel": "Summary",      // Full | Summary | Minimal — Stage 11.2
    "QdrantUrl": "",
    "QdrantApiKey": "",
    "RagEnabled": false,               // Stage 12.2
    "PlanCacheEnabled": false,         // Stage 19.10
    "ToolFlags": {},                   // Stage 16.8
    "LangSmithTracing": true,
    "LangSmithApiKey": ""
}
```

> ⚠️ **Quy tắc bất di bất dịch từ Stage này trở đi:** thêm bất kỳ khoá config nào ở Stage sau
> thì **phải** cập nhật cùng lúc ba nơi: `Settings` (ở trên), `AiSidecarManager` env, và
> `appsettings.json`. Nếu không, `Settings` mất tư cách "nguồn sự thật duy nhất".
> Có test chặn — xem mục 7.10.

---

## 7.4. `app/prompts/` — tách prompt khỏi code

**Vấn đề:** prompt là thứ sửa nhiều nhất nhưng đang nằm lẫn trong code Python.

`app/prompts/loader.py`:
```python
from functools import lru_cache
from pathlib import Path

PROMPT_DIR = Path(__file__).parent


@lru_cache
def _read(name: str) -> str:
    path = PROMPT_DIR / f"{name}.md"
    if not path.exists():
        raise FileNotFoundError(f"Không tìm thấy prompt: {path}")
    return path.read_text(encoding="utf-8")


def render(name: str, **kwargs) -> str:
    """Đọc prompt và thay các placeholder dạng {ten_bien}."""
    return _read(name).format(**kwargs)
```

Dùng: `render("system_manager_chat", full_name=..., roles=..., permissions=...)`

**Lợi ích:** sửa prompt = sửa file markdown, reviewer đọc diff prompt như đọc văn bản,
không lẫn với logic.

---

## 7.5. `app/services/backend_client.py` — gom mọi lời gọi về .NET

Hiện `manager_chat_controller.py` tự tạo `httpx.AsyncClient`, tự ghép URL, tự set header.
Sau này thêm 6 tool nữa là lặp lại 6 lần.

```python
import httpx
from app.config import get_settings
from app.core.errors import BackendError, ForbiddenError


class BackendClient:
    """Client duy nhất giao tiếp với backend .NET."""

    def __init__(self, auth_header: str):
        self._settings = get_settings()
        self._auth_header = auth_header

    def _headers(self) -> dict:
        return {
            "Authorization": self._auth_header,
            "X-Internal-Secret": self._settings.backend_internal_secret,
        }

    async def _post(self, path: str, payload: dict) -> dict:
        url = f"{self._settings.backend_base}{path}"
        timeout = self._settings.request_timeout_seconds
        async with httpx.AsyncClient(timeout=timeout) as client:
            resp = await client.post(url, json=payload, headers=self._headers())
        if resp.status_code == 403:
            raise ForbiddenError(path)
        if resp.status_code >= 400:
            raise BackendError(path, resp.status_code)
        return resp.json()

    async def get_context(self, session_id: str, message: str,
                          history_limit: int = 20) -> dict:
        return await self._post("/internal/chat/context", {
            "sessionId": session_id,
            "message": message,
            "historyLimit": history_limit,
        })

    async def call_tool(self, tool_path: str, payload: dict) -> dict:
        return await self._post(f"/internal/chat/tools/{tool_path.lstrip('/')}", payload)
```

**Một chỗ duy nhất** để: thêm retry, thêm timeout, thêm log, thêm trace. Không rải rác.

**Cải thiện thêm:** dùng `httpx.AsyncClient` dùng chung theo vòng đời app (FastAPI `lifespan`)
thay vì tạo mới mỗi request — tiết kiệm TCP handshake, xem [14-STAGE-PERFORMANCE.md](14-STAGE-PERFORMANCE.md).

---

## 7.6. `app/core/errors.py` — hết `except: pass`

```python
class SidecarError(Exception):
    """Lỗi nội bộ của sidecar."""
    user_message = "Đã có lỗi xảy ra. Vui lòng thử lại."


class BackendError(SidecarError):
    user_message = "Không lấy được dữ liệu từ hệ thống. Vui lòng thử lại."

    def __init__(self, path: str, status: int):
        self.path, self.status = path, status
        super().__init__(f"Backend {path} trả về {status}")


class ForbiddenError(SidecarError):
    user_message = "Bạn không có quyền truy cập dữ liệu này."

    def __init__(self, path: str):
        self.path = path
        super().__init__(f"Không có quyền gọi {path}")


class LlmError(SidecarError):
    user_message = "Không kết nối được tới dịch vụ AI. Vui lòng thử lại sau."
```

**Nguyên tắc:** log đầy đủ chi tiết ở server (`logger.exception`), chỉ trả `user_message`
ra ngoài. Không bao giờ đẩy `str(e)` cho user (xem Stage 12.3 và Stage 11).

---

## 7.7. `app/core/logging.py` — structured logging

```python
import logging
import sys
import json


class JsonFormatter(logging.Formatter):
    def format(self, record: logging.LogRecord) -> str:
        payload = {
            "level": record.levelname,
            "logger": record.name,
            "message": record.getMessage(),
        }
        for key in ("session_id", "run_id", "tool", "duration_ms"):
            if hasattr(record, key):
                payload[key] = getattr(record, key)
        if record.exc_info:
            payload["exception"] = self.formatException(record.exc_info)
        return json.dumps(payload, ensure_ascii=False)


def setup_logging(level: str = "INFO") -> None:
    handler = logging.StreamHandler(sys.stdout)
    handler.setFormatter(JsonFormatter())
    root = logging.getLogger()
    root.handlers = [handler]
    root.setLevel(level)
```

Log của sidecar đi qua stdout → `AiSidecarManager` đã bật `RedirectStandardOutput`.
**Cần bổ sung** ở .NET: subscribe `OutputDataReceived` / `ErrorDataReceived` và ghi vào
`ILogger` để log sidecar xuất hiện chung với log ứng dụng.

> **Tuyệt đối không log nội dung tin nhắn của user** — chỉ log `session_id`, độ dài, thời gian.

---

## 7.8. `pyproject.toml` + pin version

`requirements.txt` hiện không pin version nào:
```
fastapi
uvicorn
langchain
...
```
→ Mỗi lần deploy có thể cài version khác nhau. LangChain là thư viện **breaking change rất thường xuyên**.

```toml
[project]
name = "anhemmotor-ai-sidecar"
version = "0.1.0"
requires-python = ">=3.11"
dependencies = [
    "fastapi>=0.115,<0.120",
    "uvicorn[standard]>=0.32,<0.36",
    "pydantic>=2.9,<3",
    "pydantic-settings>=2.6,<3",
    "httpx>=0.27,<0.29",
    "langchain-core>=0.3,<0.4",
    "langchain-google-genai>=2.0,<3",
    "langchain-openai>=0.2,<0.4",
    "langgraph>=0.2,<0.4",
]

[project.optional-dependencies]
dev = ["pytest>=8", "pytest-asyncio>=0.24", "respx>=0.21", "ruff>=0.7"]
qdrant = ["qdrant-client>=1.12,<2"]
```

Sinh `requirements.txt` khoá chặt để deploy: `pip-compile` hoặc `uv pip compile`.

> `langchain` và `langchain-community` (đang có trong requirements) **không còn cần** nếu dùng
> `langchain-core` + `langgraph` — bỏ đi để giảm ~40 dependency.

---

## 7.8b. Dùng gì của LangChain/LangGraph — không tự chế

**Nguyên tắc: mọi thứ LangChain/LangGraph đã có thì dùng sẵn.** Chỉ tự viết phần thuộc nghiệp vụ
AnhEmMotor mà không thư viện nào biết.

### Dùng sẵn của thư viện

| Việc | Dùng | Ở Stage |
|---|---|---|
| Vòng lặp agent | `langgraph.StateGraph` + `create_react_agent` (bước đầu) | 3, 9 |
| Định nghĩa tool | `langchain_core.tools.StructuredTool` + `args_schema` Pydantic | 3 |
| Thực thi tool + bắt lỗi | `langgraph.prebuilt.ToolNode(handle_tool_errors=...)` | 13 |
| Streaming từng token & event | `graph.astream_events(version="v2")` | 3, 8 |
| Lưu / khôi phục state | `AsyncPostgresSaver` (checkpointer) | 8, 10 |
| Dừng chờ người dùng duyệt | `interrupt()` của LangGraph | 10 |
| Sửa state từ bên ngoài | `graph.aupdate_state(config, values)` | 9 |
| Chạy tool song song | `ToolNode` tự xử lý nhiều `tool_calls` trong một lượt | 14 |
| Output có cấu trúc | `llm.with_structured_output(Schema)` | 13, 19, 20 |
| Cắt lịch sử theo token | `langchain_core.messages.trim_messages` | 2, 14 |
| Kiểu message | `SystemMessage` / `HumanMessage` / `AIMessage` / `ToolMessage` | 2, 17 |
| Trace | LangSmith qua env `LANGCHAIN_TRACING_V2` | 6 |

### Tự viết — vì là nghiệp vụ riêng, không thư viện nào có

| Việc | Vì sao phải tự viết |
|---|---|
| `ToolSpec` + registry theo permission | Mô hình phân quyền 185 hằng số của dự án |
| Pre-flight router theo module | Gắn với 13 module nghiệp vụ riêng |
| `RunSnapshot` | Yêu cầu nhất quán nội tại — không phải bài toán của LangGraph |
| Redaction | Quy tắc PII của dự án |
| Envelope + parity test | Định nghĩa nghiệp vụ trong `GLOSSARY.md` |
| `ChatRun` / `ChatRunEvent` ở .NET | Run engine là của .NET, LangGraph không biết |

### Ba chỗ dễ tự chế nhầm — đừng làm

1. **Đừng tự viết vòng lặp tool.** `ToolNode` với `handle_tool_errors` đã xử lý: gọi tool, bắt
   exception, trả `ToolMessage` cho model tự sửa. Logic "2 lần tự sửa" ở Stage 13.4 là **cấu hình
   cho `ToolNode`**, không phải vòng lặp `while` tự viết.
2. **Đừng tự quản lý message history trong dict.** Dùng `AgentState` với reducer `add_messages`
   của LangGraph — nó xử lý merge, dedupe theo id, và tương thích với checkpointer.
3. **Đừng tự viết retry/backoff cho LLM.** `llm.with_retry(...)` của `langchain-core` có sẵn.

### Một chỗ nên chuyển sang API mới

`AISidecar/controllers/search_controller.py` hiện dùng `PromptTemplate | llm | PydanticOutputParser`.
Cách này parse text nên hay hỏng khi model trả kèm markdown fence. Từ Stage này chuyển sang
`llm.with_structured_output(SearchIntent)` — dùng function calling của provider, không parse text.

---

## 7.9. Lộ trình refactor (không big-bang)

Làm từng bước, sau mỗi bước chat vẫn phải chạy được:

| Bước | Việc | Kiểm chứng |
|---|---|---|
| 7.a | Tạo `app/`, chuyển `main.py`, cập nhật `PYTHONPATH` / lệnh uvicorn ở `AiSidecarManager.cs` | `GET /` trả 200 |
| 7.b | Thêm `config.py`, thay hết `os.environ.get` | Đổi env → hành vi đổi đúng |
| 7.c | Thêm `core/logging.py` + `core/errors.py`, xoá mọi `except: pass` | Log JSON xuất hiện trong log .NET |
| 7.d | Tách `services/backend_client.py` | Chat vẫn chạy |
| 7.e | Tách `prompts/` ra file `.md` | Sửa file md → prompt đổi, không cần build lại |
| 7.f | Chuyển controller sang `api/v1/`, bỏ khởi tạo module-level trong `search.py` | AI Search vẫn chạy |
| 7.g | `pyproject.toml` + pin version, gộp `requirements-dev.txt` vào `[dev]` | `pytest` xanh |

**Mỗi bước phải cập nhật test tương ứng** — xem mục 7.10 để biết file nào sửa ở bước nào.

**Lệnh uvicorn phải đổi** ở `Infrastructure/Services/Ai/AiSidecarManager.cs`:
```csharp
Arguments = $"-m uvicorn app.main:app --host 127.0.0.1 --port {port} --log-level warning",
```

---

## 7.10. Di trú test đã có từ Stage 1

Stage 1.6 đã tạo 4 file test Python **trỏ vào cấu trúc phẳng cũ**. Refactor sang `app/` làm chúng
hỏng hết. Đây là phần việc **bắt buộc** của Stage này — làm cùng lúc với từng bước refactor,
không để dồn cuối.

### Bảng ánh xạ

| File test | Sửa gì | Sửa ở bước nào |
|---|---|---|
| `tests/conftest.py` | `import main` → `from app.main import app`; thêm fixture `settings` | 7.a |
| `tests/test_config_contract.py` | Đọc fallback từ `app/core/llm.py`; thêm đối chiếu với `app/config.py` | 7.b |
| `tests/test_llm_factory.py` | `from services.llm_factory import get_llm` → `from app.core.llm import get_llm` | 7.b |
| `tests/test_dependencies.py` | `from app.api.deps import verify_internal_secret`; đường dẫn route không đổi | 7.f |
| `tests/test_module_level_init.py` | Trỏ `controllers/search_controller.py` → `app/api/v1/search.py` | 7.f |

### Hai test cần viết lại về bản chất, không chỉ đổi import

**1. `test_config_contract.py`** — sau 7.b, tên model có **ba** nguồn thay vì hai:
`appsettings.json`, `app/config.py` (default của `Settings.model`), và không còn fallback rải rác.
Test phải khẳng định `appsettings.json` khớp `app/config.py`:

```python
def test_ten_model_khop_giua_appsettings_va_settings(backend_root):
    """Sau Stage 7, nguồn sự thật phía Python là app/config.py, không phải llm_factory.py."""
    from app.config import Settings

    appsettings_model = _read_appsettings_model(backend_root)
    default_model = Settings.model_fields["model"].default

    assert appsettings_model == default_model, (
        f"appsettings.json có Model='{appsettings_model}' nhưng "
        f"app/config.py mặc định '{default_model}'"
    )
```

**2. `test_module_level_init.py`** — sau 7.f, kiểm tra bằng cách import thay vì đọc source
(mạnh hơn, không phụ thuộc cách viết):

```python
def test_import_app_khong_can_env(monkeypatch):
    """Import toàn bộ app không được cần env nào — chỉ khi gọi hàm mới cần."""
    for key in ("API_KEY", "BACKEND_URL", "BACKEND_INTERNAL_SECRET", "QDRANT_URL"):
        monkeypatch.delenv(key, raising=False)

    import importlib
    import sys
    for name in [m for m in sys.modules if m.startswith("app.")]:
        sys.modules.pop(name, None)

    importlib.import_module("app.main")     # không được ném exception
```

### Bổ sung test mới cho hạ tầng vừa dựng

| File | Kiểm gì |
|---|---|
| `test_config.py` | `Settings` đọc đúng env; `backend_base` bỏ hậu tố `/api`; `is_production` đúng; giá trị `ai_provider` ngoài `Literal` → lỗi validate |
| `test_prompts.py` | `render()` thay đúng placeholder; thiếu file → `FileNotFoundError` có tên file; thiếu biến → lỗi rõ ràng chứ không im lặng |
| `test_errors.py` | Mỗi exception có `user_message`; `user_message` **không** chứa URL nội bộ, tên host, hay `str(e)` gốc |
| `test_backend_client.py` | Dùng `respx`: 403 → `ForbiddenError`; 500 → `BackendError`; timeout không treo; header `X-Internal-Secret` được gửi |

### Guard test .NET cần cập nhật

`UnitTests/SidecarConfigGuard.cs` (Stage 1.6.4) assert `--host 127.0.0.1`. Sau 7.a, chuỗi lệnh đổi
thành `app.main:app` → **bổ sung** assert, đừng để guard test trở nên vô nghĩa:

```csharp
[Fact(DisplayName = "GUARD_03 - AiSidecarManager trỏ đúng entrypoint app.main:app")]
public void AiSidecarManager_TroDung_Entrypoint()
{
    var path = Path.Combine(RepoRoot(), "Infrastructure", "Services", "Ai", "AiSidecarManager.cs");
    var content = File.ReadAllText(path);

    content.Should().Contain("app.main:app",
        "sau Stage 7, entrypoint là app/main.py chứ không phải main.py ở gốc");
    content.Should().NotContain("uvicorn main:app");
}
```

### Nguyên tắc khi refactor

> **Sau mỗi bước 7.a–7.g, `pytest` phải xanh trước khi sang bước tiếp.**
> Nếu một bước làm đỏ quá 3 file test, bước đó quá lớn — chia nhỏ thêm.
> Đây là lý do lộ trình 7.9 chia 7 bước thay vì làm một lần.

---

## Definition of Done — Stage 7

- [ ] Cấu trúc thư mục đúng như mục 7.2 (các thư mục của Stage sau có thể còn rỗng).
- [ ] Không còn `os.environ.get` nào ngoài `app/config.py`.
- [ ] Không còn `except Exception: pass` nào trong toàn bộ sidecar.
- [ ] Không còn khởi tạo LLM / chain ở module level.
- [ ] Prompt nằm trong file `.md`, sửa prompt không cần đụng file `.py`.
- [ ] `Settings` có đủ field cho **mọi** Stage sau: `fast_model`, `tool_detail_level`,
      `rag_enabled`, `plan_cache_enabled`, `postgres_url`, `tool_flags`, `tool_kill_switch`.
- [ ] Không tự viết vòng lặp tool / retry / message history — dùng `ToolNode`,
      `with_retry`, `add_messages` của LangGraph (mục 7.8b).
- [ ] Log của sidecar (JSON) xuất hiện trong log của backend .NET.
- [ ] `requirements.txt` pin version cụ thể; `langchain` + `langchain-community` đã gỡ.
- [ ] `AiSidecarManager.cs` trỏ đúng `app.main:app` và truyền đủ env mới.
- [ ] Chat + AI Search vẫn hoạt động y hệt trước refactor.

### Test (mục 7.10)

- [ ] **4 file test từ Stage 1 đã di trú xong và pass** — không xoá, không skip test nào.
- [ ] `test_config_contract.py` đổi sang đối chiếu `appsettings.json` với `app/config.py`.
- [ ] `test_module_level_init.py` chuyển sang kiểm bằng import: `import app.main` không cần env nào.
- [ ] 4 file test mới pass: `test_config.py`, `test_prompts.py`, `test_errors.py`, `test_backend_client.py`.
- [ ] `test_errors.py` chứng minh `user_message` không lộ URL nội bộ / tên host / `str(e)` gốc.
- [ ] `UnitTests/SidecarConfigGuard.cs` bổ sung `GUARD_03` (entrypoint `app.main:app`).
- [ ] **Test hợp đồng config:** mọi khoá trong `AISetup` của `appsettings.json` đều có field
      tương ứng trong `Settings`, và ngược lại — chặn việc Stage sau thêm config mà quên
      cập nhật một trong ba nơi (mục 7.3).
- [ ] `search_controller` đã chuyển sang `with_structured_output`, bỏ `PydanticOutputParser`.
- [ ] **Sau mỗi bước 7.a–7.g, `pytest` xanh** — không dồn việc sửa test tới cuối Stage.
- [ ] `run-chatbot-tests.ps1` (Stage 1.6.5) vẫn chạy được không cần sửa.
- [ ] Bước CI cài dependency đổi từ `requirements-dev.txt` sang `pip install -e ".[dev]"`.

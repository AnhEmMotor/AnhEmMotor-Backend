# Stage 1 — Sửa nền móng & dọn bug

> Ưu tiên: 🔴 Cao · Ước lượng: 0.5–1 ngày · Phụ thuộc: không
> Mục tiêu: làm cho luồng hiện tại **đúng và nhất quán** trước khi xây thêm tính năng.

---

## 1.1. Sửa `SendManagerChatMessageCommandHandler` (REST không chạy)

**File:** `Application/Features/ManagerChat/Commands/SendManagerChatMessage/SendManagerChatMessageCommandHandler.cs`

**Vấn đề:** Handler đọc response và tìm property JSON `"reply"`:

```csharp
var sidecarResponse = JsonSerializer.Deserialize<JsonElement>(responseString);
if (sidecarResponse.TryGetProperty("reply", out var replyProp))
```

Nhưng `AISidecar/controllers/manager_chat_controller.py` trả `StreamingResponse(..., media_type="text/plain")`.
→ `JsonSerializer.Deserialize` ném exception → rơi vào `catch` → luôn lưu `"Không thể kết nối đến AI Sidecar."`

**Chọn 1 trong 2 hướng:**

### Hướng A (khuyến nghị) — Bỏ hẳn đường REST send-message
SignalR đã là kênh chính. Đường REST vừa trùng lặp vừa không stream được.

1. Xoá endpoint `POST /sessions/{id}/message` khỏi `WebAPI/Controllers/V1/ManagerChatController.cs`.
2. Xoá thư mục `Application/Features/ManagerChat/Commands/SendManagerChatMessage/`.
3. Xoá `Application/ApiContracts/ManagerChat/Requests/SendManagerChatMessageRequest.cs`.
4. Xoá `sendMessage()` khỏi `AnhEmMotor-Management/src/api/chat/chat.api.ts`.
5. Cập nhật `ControllerTests/ManagerChatControllerTests.cs` bỏ test tương ứng.

### Hướng B — Giữ REST làm fallback khi SignalR chết
1. Thêm endpoint non-stream ở sidecar:
   ```python
   @router.post("/manager-chat/sync")
   async def handle_chat_sync(request: Request, chat_req: ChatRequest):
       # ... build messages giống handle_chat
       result = await llm.ainvoke(messages)
       return {"reply": result.content}
   ```
2. Đổi URL trong handler: `$"{sidecarUrl}/manager-chat/sync"`.
3. Bọc `JsonSerializer.Deserialize` trong try riêng, log lỗi cụ thể thay vì nuốt exception.

> **Quyết định cần chốt trước khi code:** A hay B. Mặc định trong plan này là **A**.
>
> ✅ **Đã chốt: Hướng A.** Đã xoá endpoint, `Commands/SendManagerChatMessage/`,
> `SendManagerChatMessageRequest.cs` và `sendMessage()` ở `chat.api.ts`.
> `ManagerChatControllerTests.cs` vốn không có test cho endpoint này nên không phải bỏ test nào.

---

## 1.2. Model mặc định — KHÔNG sửa

**Chốt:** `gemini-3.5-flash` là tên model **đúng**, giữ nguyên ở cả
`WebAPI/appsettings.json` (`AISetup:Model`) và fallback trong
`AISidecar/services/llm_factory.py`.

Việc duy nhất cần làm ở mục này là **đảm bảo tên model chỉ nằm ở một nguồn sự thật**:

```bash
# Không được có chỗ nào hard-code tên model ngoài 2 vị trí trên
grep -rn "gemini-" --include="*.py" --include="*.json" --include="*.cs" . \
  | grep -v node_modules | grep -v ".venv"
```

Nếu sau này cần tách model theo mục đích (model rẻ cho routing, model mạnh cho tổng hợp),
xem [14-STAGE-PERFORMANCE.md](14-STAGE-PERFORMANCE.md) mục 14.4 — khi đó `AISetup` sẽ có
`Model`, `FastModel`, `EmbeddingModel` thay vì một khoá duy nhất.

---

## 1.3. Thống nhất `Role` của ChatMessage

**Vấn đề:** 3 nơi dùng 3 kiểu khác nhau.

| Nơi | Giá trị |
|---|---|
| `StreamManagerChatMessageCommandHandler.cs` | `"User"` / `"AI"` |
| `chat.api.ts` (TS interface) | `"User" \| "Assistant" \| "System"` |
| `ChatDrawer.vue` (runtime) | `"User"` / `"AI"` |

TS interface sai so với thực tế — hiện chạy được nhưng type không bảo vệ gì.

**Hành động:**

1. Tạo constant ở backend thay vì magic string:
   ```csharp
   // Domain/Constants/ChatRoles.cs  (tạo mới)
   namespace Domain.Constants;

   public static class ChatRoles
   {
       public const string User = "User";
       public const string Ai = "AI";
       public const string System = "System";
   }
   ```
2. Thay toàn bộ `Role = "User"` / `Role = "AI"` bằng `ChatRoles.User` / `ChatRoles.Ai`.
3. Sửa TS interface trong `chat.api.ts`:
   ```ts
   export type ChatRole = "User" | "AI" | "System";

   export interface ChatMessage {
     role: ChatRole;
     message: string;
     createdAt: string;
   }
   ```
4. Kiểm tra `ManagerChatMessageDto.cs` map đúng field.

---

## 1.4. Bảo vệ endpoint `/manager-chat` của sidecar

**File:** `AISidecar/controllers/manager_chat_controller.py`

**Vấn đề:** `/search` và `/test-role` đều có `Depends(verify_internal_token)`, riêng `/manager-chat`
chỉ kiểm tra "có header Authorization hay không" — không validate gì cả. Sidecar bind `0.0.0.0`.

**Hành động:**

1. Backend gửi kèm internal secret ở header riêng (không đè token của user):
   ```csharp
   // StreamManagerChatMessageCommandHandler.cs
   httpRequest.Headers.Add("X-Internal-Secret", internalSecret);
   if (!string.IsNullOrEmpty(request.Token))
       httpRequest.Headers.Add("Authorization", $"Bearer {request.Token}");
   ```
   `internalSecret` lấy từ `IConfiguration["Jwt:Key"]` — trùng với biến env
   `BACKEND_INTERNAL_SECRET` mà `AiSidecarManager.cs` đã set sẵn.

2. Thêm dependency mới ở `AISidecar/dependencies.py`:
   ```python
   from fastapi import Header

   def verify_internal_header(x_internal_secret: str = Header(None)):
       if x_internal_secret != BACKEND_INTERNAL_SECRET:
           raise HTTPException(status_code=403, detail="Invalid internal secret")
       return x_internal_secret
   ```

3. Áp vào route:
   ```python
   @router.post("/manager-chat")
   async def handle_chat(request: Request, chat_req: ChatRequest,
                         _: str = Depends(verify_internal_header)):
   ```

4. **Bonus:** đổi uvicorn bind từ `0.0.0.0` sang `127.0.0.1` trong
   `Infrastructure/Services/Ai/AiSidecarManager.cs` (dòng `Arguments = $"-m uvicorn main:app --host 0.0.0.0 ..."`)
   vì sidecar chỉ được gọi nội bộ.

---

## 1.5. Dọn dẹp phụ

- `AISidecar/controllers/search_controller.py` import `ChatGoogleGenerativeAI` trực tiếp ở đầu file
  nhưng không dùng (đã chuyển sang `get_llm`) → xoá import thừa.
- `search_controller.py` khởi tạo `llm = get_llm(...)` ở **module level** → nếu env chưa sẵn sàng
  lúc import sẽ tạo `FakeListLLM` vĩnh viễn. Chuyển sang khởi tạo lazy trong hàm `search()`.
- `result.dict()` / `SearchIntent().dict()` là API Pydantic v1 đã deprecated → đổi sang `.model_dump()`.
- `WebAPI/appsettings.json` đang commit sẵn `LangSmithApiKey` dạng placeholder có prefix thật
  (`lsv2_pt_...`) → xác nhận đây không phải key thật; nếu là thật thì **revoke ngay** và chuyển sang
  user-secrets / biến môi trường.
- Xoá các file rác ở gốc `AnhEmMotor-Backend/`: `build_errors.txt`, `build_output.txt`,
  `build_results.txt`, `build_results2.txt`, `diff.txt`, `fix_preview.txt`, `broken_words.txt`
  (hoặc thêm vào `.gitignore`).

---

## 1.6. Hạ tầng test & các bài test cho Stage 1

Đây là phần **thiết lập một lần** cho toàn bộ lộ trình chatbot. Mọi Stage sau bổ sung test vào
đúng hạ tầng này.

### 1.6.1. Hạ tầng đang có (đã kiểm chứng)

**Backend .NET** — 3 project test, đã cấu hình sẵn:

| Project | Mục đích | Thư viện |
|---|---|---|
| `UnitTests` | Handler, service, logic thuần | xunit.v3 3.2.2, **Moq** 4.20.72, FluentAssertions 8.10.0, AutoFixture.AutoMoq, MockQueryable.Moq |
| `ControllerTests` | Controller với `ISender` mock | xunit.v3, Moq, FluentAssertions |
| `IntegrationTests` | E2E qua HTTP thật | `Testcontainers.PostgreSql`, `Respawn`, `Microsoft.AspNetCore.Mvc.Testing` |

- Target framework: **net10.0**
- Coverage: `coveragerc.runsettings` đã có ở gốc
- Helper sẵn dùng trong `IntegrationTests/SetupClass/`:
  `IntegrationTestWebAppFactory`, `IntegrationTestAuthHelper.CreateUserWithPermissionsAsync`,
  `IntegrationTestFileHelper`

**Quy ước đặt file — theo đúng repo hiện tại, không tạo cấu trúc mới:**

| Project | Quy ước | File chat hiện có |
|---|---|---|
| `UnitTests` | **File phẳng** theo feature (`Auth.cs`, `Product.cs`, `Statistics.cs`...) | `UnitTests/ManagerChat.cs` — 3 test cho query handler |
| `ControllerTests` | `<Feature>ControllerTests.cs` | `ControllerTests/ManagerChatControllerTests.cs` — 1 test |
| `IntegrationTests` | File phẳng, `DisplayName` dạng `<PREFIX>_<số> - <mô tả>` | `IntegrationTests/ManagerChat.cs` — `MCHAT_01..03` |

→ **Test mới của Stage 1 bổ sung vào các file đang có**, và đánh số tiếp từ `MCHAT_04`.
Không tạo thư mục `UnitTests/Features/...` — repo không dùng cấu trúc đó.

> ⚠️ **Dự án dùng Moq, không dùng NSubstitute.** Mọi ví dụ test trong bộ tài liệu này phải theo
> cú pháp Moq (`new Mock<T>()`, `.Setup(...)`, `.Object`).

> ⚠️ **`UnitTests/ManagerChat.cs` hiện chưa có `DisplayName`.** Test mới nên có `DisplayName`
> để đọc kết quả CI dễ hơn; test cũ để nguyên, không sửa kèm trong Stage này.

**AISidecar — hiện chưa có test nào.** Có `.venv` với `python.exe` nhưng **chưa cài pytest**,
chưa có `tests/`, chưa có `pytest.ini`. (`AISidecar/controllers/test_controller.py` không phải
test — đó là endpoint debug `/test-role`, sẽ bị tắt ở Stage 5.2.)
Mục 1.6.2 dựng từ đầu.

### 1.6.2. Dựng hạ tầng test cho AISidecar

#### File mới: `AISidecar/requirements-dev.txt`
```
-r requirements.txt
pytest>=8.3
pytest-asyncio>=0.24
httpx>=0.27
respx>=0.21
```
> `httpx` cần cho `fastapi.testclient.TestClient`; `respx` để mock lời gọi HTTP ra backend .NET.

#### File mới: `AISidecar/pytest.ini`
```ini
[pytest]
testpaths = tests
python_files = test_*.py
python_functions = test_*
asyncio_mode = auto
addopts = -q --strict-markers
filterwarnings =
    ignore::DeprecationWarning:google.*
```

#### File mới: `AISidecar/tests/conftest.py`
```python
"""Fixture dùng chung cho test của AI Sidecar."""
import os
import sys
from pathlib import Path

import pytest

# Cho phép import module của sidecar khi chạy pytest từ thư mục AISidecar
SIDECAR_ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(SIDECAR_ROOT))

INTERNAL_SECRET = "test-internal-secret-abc123"


@pytest.fixture(autouse=True)
def _clean_env(monkeypatch):
    """Đặt env tối thiểu và xoá env thật để test không phụ thuộc máy chạy."""
    for key in ("API_KEY", "AI_PROVIDER", "MODEL", "AI_API_ENDPOINT", "BACKEND_URL"):
        monkeypatch.delenv(key, raising=False)
    monkeypatch.setenv("BACKEND_INTERNAL_SECRET", INTERNAL_SECRET)


@pytest.fixture
def client():
    """TestClient của FastAPI app. Import muộn để env fixture kịp áp dụng."""
    from fastapi.testclient import TestClient
    import main

    return TestClient(main.app)


@pytest.fixture
def backend_root() -> Path:
    """Đường dẫn tới thư mục AnhEmMotor-Backend, dùng cho test đối chiếu cấu hình."""
    return SIDECAR_ROOT.parent
```

> **Lưu ý:** `dependencies.py` hiện đọc `BACKEND_INTERNAL_SECRET` ở **module level**
> (`BACKEND_INTERNAL_SECRET = os.environ.get(...)`). Nghĩa là nó bị đóng băng lúc import →
> `monkeypatch.setenv` trong fixture sẽ **không** có tác dụng nếu module đã được import trước đó.
> Đây chính là một biểu hiện của vấn đề "khởi tạo ở module level" nêu ở mục 1.5.
> **Cách xử lý ở Stage 1:** đổi `verify_internal_header` đọc env **trong hàm**:
> ```python
> def verify_internal_header(x_internal_secret: str = Header(None)):
>     expected = os.environ.get("BACKEND_INTERNAL_SECRET", "")
>     if not expected or x_internal_secret != expected:
>         raise HTTPException(status_code=403, detail="Invalid internal secret")
>     return x_internal_secret
> ```
> Vừa test được, vừa cho phép đổi secret không cần restart. Stage 7 sẽ chuyển sang `Settings`.

### 1.6.3. Các bài test Python cho Stage 1

#### `AISidecar/tests/test_dependencies.py` — mục 1.4

```python
"""Kiểm tra endpoint nội bộ được bảo vệ bằng X-Internal-Secret."""
import pytest
from tests.conftest import INTERNAL_SECRET

PROTECTED_ENDPOINTS = [
    ("/manager-chat", {"session_id": "s1", "message": "xin chào"}),
    ("/manager-chat/generate-title", {"message": "xin chào"}),
]


@pytest.mark.parametrize("path,payload", PROTECTED_ENDPOINTS)
def test_thieu_internal_secret_tra_403(client, path, payload):
    resp = client.post(path, json=payload, headers={"Authorization": "Bearer fake"})
    assert resp.status_code == 403, f"{path} phải yêu cầu X-Internal-Secret"


@pytest.mark.parametrize("path,payload", PROTECTED_ENDPOINTS)
def test_sai_internal_secret_tra_403(client, path, payload):
    resp = client.post(path, json=payload, headers={
        "Authorization": "Bearer fake",
        "X-Internal-Secret": "sai-secret",
    })
    assert resp.status_code == 403


def test_thieu_authorization_tra_401(client):
    """Có internal secret nhưng thiếu token user → 401, không phải 403."""
    resp = client.post("/manager-chat",
                       json={"session_id": "s1", "message": "xin chào"},
                       headers={"X-Internal-Secret": INTERNAL_SECRET})
    assert resp.status_code == 401


def test_health_khong_yeu_cau_secret(client):
    """Health check phải mở, nhưng không được lộ thông tin nội bộ."""
    resp = client.get("/")
    assert resp.status_code == 200
    body = resp.json()
    assert body["status"] == "ok"
    # Không rò rỉ cấu hình
    text = str(body).lower()
    for leak in ("secret", "api_key", "apikey", "token", "backend_url"):
        assert leak not in text
```

#### `AISidecar/tests/test_llm_factory.py` — mục 1.2 và 1.5

```python
"""Kiểm tra llm_factory chọn đúng provider và tôn trọng env."""
from services.llm_factory import get_llm

DEFAULT_MODEL = "gemini-3.5-flash"      # KHÔNG đổi — xem mục 1.2


def test_khong_co_api_key_tra_fake_llm(monkeypatch):
    """Thiếu API key thì phải trả LLM giả, không được ném exception lúc khởi động."""
    monkeypatch.delenv("API_KEY", raising=False)
    llm = get_llm()
    assert type(llm).__name__ == "FakeListLLM"


def test_provider_apiendpoint_dung_chat_openai(monkeypatch):
    monkeypatch.setenv("AI_PROVIDER", "apiendpoint")
    monkeypatch.setenv("AI_API_ENDPOINT", "http://localhost:11434/v1")
    monkeypatch.setenv("MODEL", "qwen2.5:7b")
    llm = get_llm()
    assert type(llm).__name__ == "ChatOpenAI"
    assert llm.model_name == "qwen2.5:7b"


def test_provider_apiendpoint_khong_can_api_key(monkeypatch):
    """Ollama/local không cần key — phải tự điền placeholder, không rơi về FakeListLLM."""
    monkeypatch.setenv("AI_PROVIDER", "apiendpoint")
    monkeypatch.setenv("AI_API_ENDPOINT", "http://localhost:11434/v1")
    monkeypatch.delenv("API_KEY", raising=False)
    llm = get_llm()
    assert type(llm).__name__ == "ChatOpenAI"


def test_gemini_ton_trong_env_model(monkeypatch):
    monkeypatch.setenv("API_KEY", "fake-key-for-test")
    monkeypatch.setenv("MODEL", "gemini-3.5-flash")
    llm = get_llm()
    assert type(llm).__name__ == "ChatGoogleGenerativeAI"
    assert DEFAULT_MODEL in str(llm.model)


def test_temperature_duoc_truyen_dung(monkeypatch):
    monkeypatch.setenv("API_KEY", "fake-key-for-test")
    llm = get_llm(temperature=0.42)
    assert llm.temperature == 0.42
```

#### `AISidecar/tests/test_config_contract.py` — mục 1.2, chống lệch hai nguồn cấu hình

```python
"""Đối chiếu tên model giữa appsettings.json và fallback trong llm_factory.py.

Đây là test hợp đồng: hai nguồn sự thật phải khớp nhau, nếu không thì môi trường
thiếu env MODEL sẽ chạy model khác với môi trường có env.
"""
import json
import re
from pathlib import Path

import pytest


def _read_appsettings_model(backend_root: Path) -> str:
    raw = (backend_root / "WebAPI" / "appsettings.json").read_text(encoding="utf-8-sig")
    # appsettings.json có comment // → loại bỏ trước khi parse
    cleaned = re.sub(r"^\s*//.*$", "", raw, flags=re.MULTILINE)
    cleaned = re.sub(r"(?<![:\"])//[^\"\n]*$", "", cleaned, flags=re.MULTILINE)
    return json.loads(cleaned)["AISetup"]["Model"]


def _read_factory_fallback(sidecar_root: Path) -> str:
    src = (sidecar_root / "services" / "llm_factory.py").read_text(encoding="utf-8")
    matches = re.findall(r'os\.environ\.get\(\s*"MODEL"\s*,\s*"([^"]+)"\s*\)', src)
    assert matches, "Không tìm thấy fallback của MODEL trong llm_factory.py"
    return matches[-1]      # nhánh Gemini là nhánh mặc định


def test_ten_model_khop_giua_appsettings_va_sidecar(backend_root):
    sidecar_root = backend_root / "AISidecar"
    assert _read_appsettings_model(backend_root) == _read_factory_fallback(sidecar_root)


def test_khong_hard_code_ten_model_ngoai_hai_vi_tri_cho_phep(backend_root):
    """Tên model chỉ được xuất hiện ở appsettings.json và llm_factory.py."""
    allowed = {
        backend_root / "WebAPI" / "appsettings.json",
        backend_root / "AISidecar" / "services" / "llm_factory.py",
    }
    skip_dirs = {".venv", "node_modules", "obj", "bin", ".git", "TestResults", "docs", "tests"}

    offenders = []
    for path in backend_root.rglob("*"):
        if not path.is_file() or path.suffix not in {".py", ".cs", ".json", ".ts", ".vue"}:
            continue
        if any(part in skip_dirs for part in path.parts) or path in allowed:
            continue
        try:
            if "gemini-" in path.read_text(encoding="utf-8", errors="ignore"):
                offenders.append(str(path.relative_to(backend_root)))
        except OSError:
            continue

    assert not offenders, f"Tên model bị hard-code ngoài vị trí cho phép: {offenders}"
```

#### `AISidecar/tests/test_module_level_init.py` — mục 1.5

```python
"""Import module không được tạo LLM hay đọc env đóng băng."""
import importlib
import sys


def test_import_search_controller_khong_tao_llm(monkeypatch):
    """Sau mục 1.5, LLM phải được khởi tạo lazy trong hàm search()."""
    monkeypatch.delenv("API_KEY", raising=False)
    sys.modules.pop("controllers.search_controller", None)

    module = importlib.import_module("controllers.search_controller")

    assert not hasattr(module, "llm"), \
        "search_controller không được khởi tạo llm ở module level (mục 1.5)"
    assert not hasattr(module, "chain"), \
        "search_controller không được khởi tạo chain ở module level (mục 1.5)"


def test_khong_con_import_thua(monkeypatch):
    """ChatGoogleGenerativeAI không được import trực tiếp — dùng get_llm()."""
    from pathlib import Path
    src = Path(__file__).resolve().parents[1] / "controllers" / "search_controller.py"
    content = src.read_text(encoding="utf-8")
    assert "from langchain_google_genai import" not in content, \
        "Xoá import thừa ChatGoogleGenerativeAI (mục 1.5)"


def test_dung_model_dump_thay_vi_dict(monkeypatch):
    """Pydantic v2: .dict() đã deprecated."""
    from pathlib import Path
    src = Path(__file__).resolve().parents[1] / "controllers" / "search_controller.py"
    content = src.read_text(encoding="utf-8")
    assert ".dict()" not in content, "Đổi .dict() sang .model_dump() (mục 1.5)"
```

### 1.6.4. Các bài test .NET cho Stage 1

#### File mới: `UnitTests/ManagerChatStream.cs`

Test mục **1.3** (`ChatRoles`) và **1.4** (header `X-Internal-Secret`).
Tách file riêng thay vì nhồi vào `UnitTests/ManagerChat.cs` vì cần nhiều mock dùng chung —
vẫn giữ quy ước file phẳng của repo.

> **Giả định:** sau mục 1.4, `StreamManagerChatMessageCommandHandler` nhận thêm
> `IConfiguration configuration` trong constructor để lấy `Jwt:Key`. Test dưới đây viết theo
> chữ ký mới.

```csharp
using System.Net;
using System.Text;
using Application.Features.ManagerChat.Commands.StreamManagerChatMessage;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Chat;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace UnitTests;

public class ManagerChatStream
{
    private const string JwtKey = "test-jwt-key-0123456789";

    private readonly Mock<IChatReadRepository> _chatRead = new();
    private readonly Mock<IChatInsertRepository> _chatInsert = new();
    private readonly Mock<IPermissionReadRepository> _permissions = new();
    private readonly Mock<IAiSidecarUrlProvider> _sidecarUrl = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly List<ChatMessage> _savedMessages = [];
    private HttpRequestMessage? _capturedRequest;

    private StreamManagerChatMessageCommandHandler CreateHandler(string sidecarBody = "Xin chào")
    {
        _chatInsert.Setup(x => x.AddMessage(It.IsAny<ChatMessage>()))
                   .Callback<ChatMessage>(_savedMessages.Add);

        _sidecarUrl.Setup(x => x.GetSidecarUrl()).Returns("http://localhost:8000");

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => _capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(sidecarBody, Encoding.UTF8, "text/plain"),
            });

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>()))
               .Returns(new HttpClient(handlerMock.Object));

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Key"] = JwtKey })
            .Build();

        return new StreamManagerChatMessageCommandHandler(
            _chatRead.Object, _chatInsert.Object, _permissions.Object,
            _sidecarUrl.Object, _unitOfWork.Object, factory.Object, config);
    }

    private void GivenSessionOwnedBy(Guid userId, Guid sessionId)
    {
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatSession { Id = sessionId, UserId = userId });
    }

    private static async Task<List<string>> Drain(IAsyncEnumerable<string> stream)
    {
        var chunks = new List<string>();
        await foreach (var c in stream) chunks.Add(c);
        return chunks;
    }

    [Fact(DisplayName = "STREAM_01 - Không có quyền thì ném UnauthorizedAccessException")]
    public async Task Handle_ThrowsUnauthorized_WhenNoPermission()
    {
        var userId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        var handler = CreateHandler();
        var command = new StreamManagerChatMessageCommand(
            Guid.NewGuid(), "xin chào", userId, "token");

        var act = async () => await Drain(handler.Handle(command, CancellationToken.None));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _savedMessages.Should().BeEmpty("chưa có quyền thì không được lưu gì");
    }

    [Fact(DisplayName = "STREAM_02 - Session của người khác thì bị từ chối")]
    public async Task Handle_Throws_WhenSessionBelongsToAnotherUser()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _permissions.Setup(x => x.HasAnyPermissionAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
        _chatRead.Setup(x => x.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ChatSession { Id = sessionId, UserId = Guid.NewGuid() });

        var handler = CreateHandler();
        var command = new StreamManagerChatMessageCommand(sessionId, "xin chào", userId, "token");

        var act = async () => await Drain(handler.Handle(command, CancellationToken.None));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "STREAM_03 - Lưu tin nhắn với ChatRoles, không dùng magic string")]
    public async Task Handle_UsesChatRolesConstants()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        GivenSessionOwnedBy(userId, sessionId);

        var handler = CreateHandler("Doanh thu tháng 7 là 1,2 tỷ.");
        var command = new StreamManagerChatMessageCommand(
            sessionId, "doanh thu tháng này?", userId, "token");

        await Drain(handler.Handle(command, CancellationToken.None));

        _savedMessages.Should().HaveCount(2);
        _savedMessages[0].Role.Should().Be(ChatRoles.User);
        _savedMessages[0].Message.Should().Be("doanh thu tháng này?");
        _savedMessages[1].Role.Should().Be(ChatRoles.Ai);
        _savedMessages[1].Message.Should().Be("Doanh thu tháng 7 là 1,2 tỷ.");
    }

    [Fact(DisplayName = "STREAM_04 - Gửi kèm X-Internal-Secret và Authorization riêng biệt")]
    public async Task Handle_SendsInternalSecretHeader()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        GivenSessionOwnedBy(userId, sessionId);

        var handler = CreateHandler();
        var command = new StreamManagerChatMessageCommand(
            sessionId, "xin chào", userId, "user-jwt-token");

        await Drain(handler.Handle(command, CancellationToken.None));

        _capturedRequest.Should().NotBeNull();
        _capturedRequest!.Headers.GetValues("X-Internal-Secret")
            .Should().ContainSingle().Which.Should().Be(JwtKey);
        _capturedRequest.Headers.GetValues("Authorization")
            .Should().ContainSingle().Which.Should().Be("Bearer user-jwt-token");
    }

    [Fact(DisplayName = "STREAM_05 - Stream trả về đủ nội dung theo từng chunk")]
    public async Task Handle_StreamsAllContent()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        GivenSessionOwnedBy(userId, sessionId);

        var longText = new string('a', 100);   // dài hơn buffer 32 ký tự
        var handler = CreateHandler(longText);
        var command = new StreamManagerChatMessageCommand(sessionId, "hỏi", userId, "token");

        var chunks = await Drain(handler.Handle(command, CancellationToken.None));

        chunks.Should().HaveCountGreaterThan(1, "phải chia thành nhiều chunk");
        string.Concat(chunks).Should().Be(longText);
    }
}
```

#### Bổ sung `ControllerTests/ManagerChatControllerTests.cs` — mục 1.1

```csharp
[Fact(DisplayName = "MCHATC_01 - Endpoint gửi tin nhắn REST đã bị loại bỏ (Hướng A)")]
public void Controller_KhongConAction_SendMessage()
{
    var actions = typeof(ManagerChatController)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Select(m => m.Name)
        .ToList();

    actions.Should().NotContain("SendMessage",
        "Stage 1.1 Hướng A đã bỏ đường REST, chỉ dùng SignalR");
}

[Fact(DisplayName = "MCHATC_02 - Controller yêu cầu xác thực")]
public void Controller_CoAuthorizeAttribute()
{
    typeof(ManagerChatController)
        .GetCustomAttributes<AuthorizeAttribute>(inherit: true)
        .Should().NotBeEmpty();
}
```

#### Bổ sung `IntegrationTests/ManagerChat.cs` — E2E, đánh số tiếp `MCHAT_04`

```csharp
[Fact(DisplayName = "MCHAT_04 - Endpoint gửi tin nhắn REST không còn tồn tại")]
public async Task SendMessageEndpoint_KhongConTonTai()
{
    var uniqueId = Guid.NewGuid().ToString("N")[..8];
    var token = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
        _factory.Services, $"user_{uniqueId}", "Password123!",
        [Permissions.Marketing.BannerManagement.Create], CancellationToken.None)
        .ConfigureAwait(true);

    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await _client.PostAsJsonAsync(
        $"/api/v1/manager-chat/sessions/{Guid.NewGuid()}/message",
        new { content = "xin chào" }).ConfigureAwait(true);

    response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
}

[Fact(DisplayName = "MCHAT_05 - Không thể xem lịch sử phiên chat của người khác")]
public async Task GetHistory_TraVeNotFound_KhiSessionCuaNguoiKhac()
{
    var idA = Guid.NewGuid().ToString("N")[..8];
    var idB = Guid.NewGuid().ToString("N")[..8];

    var tokenA = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
        _factory.Services, $"userA_{idA}", "Password123!",
        [Permissions.Marketing.BannerManagement.Create], CancellationToken.None)
        .ConfigureAwait(true);
    var tokenB = await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
        _factory.Services, $"userB_{idB}", "Password123!",
        [Permissions.Marketing.BannerManagement.Create], CancellationToken.None)
        .ConfigureAwait(true);

    // A tạo session
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
    var created = await _client.PostAsJsonAsync("/api/v1/manager-chat/sessions",
        new CreateManagerChatSessionRequest { Title = "Của A" }).ConfigureAwait(true);
    created.EnsureSuccessStatusCode();
    var sessionId = (await created.Content.ReadFromJsonAsync<JsonElement>()
        .ConfigureAwait(true)).GetProperty("id").GetGuid();

    // B đọc lịch sử của A
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
    var response = await _client
        .GetAsync($"/api/v1/manager-chat/sessions/{sessionId}/history").ConfigureAwait(true);

    response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
}
```

> `MCHAT_05` là test IDOR đầu tiên. Stage 5.4 và Stage 13 sẽ mở rộng cho `update`, `delete`,
> `context`, và về sau cho `SubscribeRun` (Stage 8).

#### File mới: `UnitTests/SidecarConfigGuard.cs` — mục 1.4 bonus

Kiểm tra bất biến cấu hình bằng cách đọc source. Rẻ, và chặn được hồi quy mà unit test thường bỏ sót.

```csharp
using FluentAssertions;

namespace UnitTests;

public class SidecarConfigGuard
{
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AnhEmMotor-Backend.sln")))
            dir = dir.Parent;
        dir.Should().NotBeNull("phải tìm được thư mục gốc chứa file .sln");
        return dir!.FullName;
    }

    [Fact(DisplayName = "GUARD_01 - Sidecar chỉ bind 127.0.0.1, không bind 0.0.0.0")]
    public void AiSidecarManager_KhongBind_TatCaInterface()
    {
        var path = Path.Combine(RepoRoot(), "Infrastructure", "Services", "Ai", "AiSidecarManager.cs");
        var content = File.ReadAllText(path);

        content.Should().NotContain("--host 0.0.0.0",
            "sidecar chỉ được gọi nội bộ, xem mục 1.4");
        content.Should().Contain("--host 127.0.0.1");
    }

    [Fact(DisplayName = "GUARD_02 - Không commit secret thật trong appsettings.json")]
    public void Appsettings_KhongChuaSecretThat()
    {
        var path = Path.Combine(RepoRoot(), "WebAPI", "appsettings.json");
        var content = File.ReadAllText(path);

        // Key LangSmith thật có dạng lsv2_pt_<32+ ký tự hex>
        content.Should().NotMatchRegex(@"lsv2_pt_[0-9a-f]{32,}",
            "chuyển LangSmithApiKey sang env/user-secrets, xem mục 1.5");
    }
}
```

### 1.6.5. Lệnh chạy test

#### Backend .NET

Chạy toàn bộ:
```bash
dotnet test AnhEmMotor-Backend.sln
```

Chỉ test liên quan chatbot (nhanh, dùng khi đang code):
```bash
dotnet test AnhEmMotor-Backend.sln --filter "FullyQualifiedName~ManagerChat|FullyQualifiedName~ChatTools|FullyQualifiedName~SidecarConfigGuard"
```

Từng project:
```bash
dotnet test UnitTests/UnitTests.csproj --filter "FullyQualifiedName~ManagerChat"
```

```bash
dotnet test ControllerTests/ControllerTests.csproj --filter "FullyQualifiedName~ManagerChat"
```

Integration test — **cần Docker đang chạy** (Testcontainers dựng PostgreSQL):
```bash
dotnet test IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~ManagerChat"
```

Kèm coverage:
```bash
dotnet test AnhEmMotor-Backend.sln --settings coveragerc.runsettings --collect:"XPlat Code Coverage"
```

#### AISidecar (Python)

Cài dependency test — chỉ cần làm một lần:
```bash
AISidecar/.venv/Scripts/python.exe -m pip install -r AISidecar/requirements-dev.txt
```

Chạy toàn bộ test của sidecar:
```bash
cd AISidecar && .venv/Scripts/python.exe -m pytest
```

Chi tiết từng test, kèm output:
```bash
cd AISidecar && .venv/Scripts/python.exe -m pytest -v -s
```

Một file cụ thể:
```bash
cd AISidecar && .venv/Scripts/python.exe -m pytest tests/test_dependencies.py -v
```

Kèm coverage (cài thêm `pytest-cov`):
```bash
cd AISidecar && .venv/Scripts/python.exe -m pytest --cov=. --cov-report=term-missing
```

> **Vì sao gọi `.venv/Scripts/python.exe` thay vì `pytest` trực tiếp:** đảm bảo dùng đúng
> interpreter của venv mà `AiSidecarManager` sẽ dùng lúc chạy thật, không phụ thuộc việc đã
> activate venv hay chưa. Trên Linux/VPS thay bằng `.venv/bin/python`.

#### Script chạy cả hai

File mới: `AnhEmMotor-Backend/run-chatbot-tests.ps1`

```powershell
# Chạy toàn bộ test liên quan AI Chatbot (cả .NET và Python).
# Dùng: ./run-chatbot-tests.ps1 [-SkipIntegration] [-SkipPython]
param(
    [switch]$SkipIntegration,
    [switch]$SkipPython
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$failed = @()

Write-Host "`n=== 1/4 Unit tests (.NET) ===" -ForegroundColor Cyan
dotnet test "$root/UnitTests/UnitTests.csproj" `
    --filter "FullyQualifiedName~ManagerChat|FullyQualifiedName~ChatTools|FullyQualifiedName~SidecarConfigGuard" `
    --nologo
if ($LASTEXITCODE -ne 0) { $failed += "UnitTests" }

Write-Host "`n=== 2/4 Controller tests (.NET) ===" -ForegroundColor Cyan
dotnet test "$root/ControllerTests/ControllerTests.csproj" `
    --filter "FullyQualifiedName~ManagerChat" --nologo
if ($LASTEXITCODE -ne 0) { $failed += "ControllerTests" }

if (-not $SkipIntegration) {
    Write-Host "`n=== 3/4 Integration tests (.NET, cần Docker) ===" -ForegroundColor Cyan
    docker info *> $null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Docker chưa chạy — bỏ qua integration test." -ForegroundColor Yellow
    } else {
        dotnet test "$root/IntegrationTests/IntegrationTests.csproj" `
            --filter "FullyQualifiedName~ManagerChat" --nologo
        if ($LASTEXITCODE -ne 0) { $failed += "IntegrationTests" }
    }
} else {
    Write-Host "`n=== 3/4 Integration tests — đã bỏ qua ===" -ForegroundColor Yellow
}

if (-not $SkipPython) {
    Write-Host "`n=== 4/4 AISidecar tests (Python) ===" -ForegroundColor Cyan
    $python = Join-Path $root "AISidecar/.venv/Scripts/python.exe"
    if (-not (Test-Path $python)) { $python = Join-Path $root "AISidecar/.venv/bin/python" }

    if (-not (Test-Path $python)) {
        Write-Host "Không tìm thấy venv của AISidecar — bỏ qua." -ForegroundColor Yellow
    } else {
        Push-Location (Join-Path $root "AISidecar")
        & $python -m pytest
        if ($LASTEXITCODE -ne 0) { $failed += "AISidecar" }
        Pop-Location
    }
} else {
    Write-Host "`n=== 4/4 AISidecar tests — đã bỏ qua ===" -ForegroundColor Yellow
}

Write-Host ""
if ($failed.Count -gt 0) {
    Write-Host "THẤT BẠI: $($failed -join ', ')" -ForegroundColor Red
    exit 1
}
Write-Host "Tất cả test đã pass." -ForegroundColor Green
```

Dùng:
```bash
pwsh ./run-chatbot-tests.ps1
```

Bỏ qua integration test khi không có Docker:
```bash
pwsh ./run-chatbot-tests.ps1 -SkipIntegration
```

#### CI

Bổ sung vào `.github/workflows/deploy.yml` (hoặc workflow CI riêng) — bước test Python
hiện **chưa có**:

```yaml
      - name: Setup Python
        uses: actions/setup-python@v5
        with:
          python-version: '3.13'

      - name: Cài dependency AISidecar
        working-directory: AnhEmMotor-Backend/AISidecar
        run: pip install -r requirements-dev.txt

      - name: Test AISidecar
        working-directory: AnhEmMotor-Backend/AISidecar
        run: pytest --junitxml=pytest-results.xml

      - name: Test .NET
        working-directory: AnhEmMotor-Backend
        run: dotnet test AnhEmMotor-Backend.sln --nologo
```

### 1.6.6. Bản đồ test cho các Stage sau

Để không phải nghĩ lại mỗi Stage:

| Loại test | File (theo quy ước phẳng của repo) | Dùng cho |
|---|---|---|
| Handler, service | `UnitTests/ManagerChat.cs`, `ManagerChatStream.cs`, `ManagerChatRun.cs`, `ChatTools.cs` | Stage 3, 8, 9, 10 |
| Bất biến cấu hình/source | `UnitTests/SidecarConfigGuard.cs`, `ChatToolsGuard.cs` | Stage 1, 16 (chặn `IgnoreQueryFilters`), 17 |
| Controller | `ControllerTests/ManagerChatControllerTests.cs` | Stage 3, 10 |
| E2E + IDOR | `IntegrationTests/ManagerChat.cs`, `ChatTools.cs` | Stage 5, 8, 13 |
| **Parity** (tool vs báo cáo UI) | `IntegrationTests/ChatToolParity.cs` | **Stage 16** |
| Contract snapshot DTO | `UnitTests/ChatToolContracts.cs` | Stage 16, 17 |
| Prompt, tool, guardrail (Python) | `AISidecar/tests/` | Stage 2, 7, 11, 13 |
| Eval chất lượng AI (không phải test) | `AISidecar/evals/` | Stage 6, 12, 13 |

> **Phân biệt rõ test và eval:** test cho kết quả nhị phân, chạy mọi lần commit.
> Eval cho điểm số, chạy khi đổi prompt/model/tool. Đừng để eval vào `pytest` — nó cần gọi
> LLM thật, chậm và tốn tiền.

### 1.6.7. Cảnh báo: Stage 7 sẽ làm hỏng các test Python này

4 file test Python ở mục 1.6.3 import theo cấu trúc phẳng hiện tại
(`main`, `services.llm_factory`, `controllers.search_controller`).
[07-STAGE-SIDECAR-ARCHITECTURE.md](07-STAGE-SIDECAR-ARCHITECTURE.md) chuyển toàn bộ sang `app/`
→ **cả 4 file sẽ đỏ**.

Đây là điều **đã lường trước và có kế hoạch**, không phải nợ kỹ thuật: mục **7.10** có bảng ánh xạ
từng file sửa gì ở bước refactor nào, cộng 4 file test mới cho hạ tầng `app/`.

**Đừng vì lý do này mà hoãn viết test ở Stage 1.** Có test trước khi refactor là cách duy nhất
biết refactor không làm hỏng hành vi — đó chính là giá trị lớn nhất của chúng.
Chi phí di trú là đổi dòng `import`, rất nhỏ so với việc refactor mà không có lưới an toàn.

---

## Definition of Done — Stage 1

- [x] Quyết định A/B cho mục 1.1 đã được chốt và thực hiện xong. → **Hướng A**.
- [x] Tên model `gemini-3.5-flash` chỉ xuất hiện ở `appsettings*.json` và fallback của `llm_factory.py`.
      (Đã gỡ hard-code ở `AiSidecarManager.cs` và `deploy.yml`.)
- [x] Constant role được dùng ở toàn bộ backend; TS type khớp runtime.
      Dùng lại `Domain/Constants/ChatRole.cs` đã có sẵn (đổi `Assistant` → `Ai = "AI"`) thay vì
      tạo thêm class `ChatRoles` trùng lặp.
- [x] Gọi thẳng `POST http://127.0.0.1:<port>/manager-chat` không kèm `X-Internal-Secret` → trả 403.
      Đã verify bằng FastAPI `TestClient`: thiếu header / sai secret / secret rỗng đều 403,
      secret đúng → 200. `/manager-chat/generate-title` cũng được bảo vệ tương tự.
- [x] Sidecar chỉ lắng nghe trên `127.0.0.1` (cả `AiSidecarManager.cs` lẫn nhánh `__main__` của `main.py`);
      `_sidecarUrl` cũng đổi sang `127.0.0.1` cho khớp.
- [x] `dotnet build` không warning mới (0 error / 7 warning — đúng bằng baseline);
      `UnitTests` 522 pass, `ControllerTests` 229 pass.
      *Ghi chú:* trước Stage 1, `dotnet build` đã FAIL sẵn 2 lỗi compile trong test
      (`ManagerChatControllerTests.cs` mock sai kiểu trả về, `UnitTests/ManagerChat.cs` gọi ctor
      7 tham số) — đã sửa luôn vì chặn DoD.
- [ ] Chat thử end-to-end trên Management UI: gửi tin nhắn → nhận stream → reload thấy lịch sử đúng.
      **Chưa chạy** — cần DB + API key thật, phải thao tác tay.

### Hạ tầng test tự động (mục 1.6)

**AISidecar (dựng từ đầu)**
- [x] `requirements-dev.txt`, `pytest.ini`, `tests/conftest.py` đã tạo; `pytest` cài trong `.venv`.
- [x] `verify_internal_header` đọc env **trong hàm**, không phải module level.
      Đồng thời fail-closed khi env rỗng, không rơi về hằng `default_secret_if_not_set`.
- [x] `tests/test_dependencies.py` pass — thiếu / sai / rỗng **header** đều 403; thiếu / rỗng
      **env** `BACKEND_INTERNAL_SECRET` cũng 403 (fail-closed); thiếu token user → 401;
      `/` không rò rỉ cấu hình.
- [x] `tests/test_llm_factory.py` pass — provider, endpoint, temperature và `MODEL` rỗng.
- [x] `tests/test_config_contract.py` pass — `appsettings` khớp fallback `llm_factory.py`;
      không hard-code `gemini-` ở nơi khác.
- [x] `tests/test_module_level_init.py` pass — import `search_controller` **không gọi `get_llm`**
      (kiểm theo hành vi, không theo tên biến); chain được cache đúng một lần; không còn import
      thừa; không còn `.dict()`; `main.py` chỉ bind loopback.

**Backend .NET (bổ sung vào file đang có, theo quy ước phẳng)**
- [x] `UnitTests/ManagerChatStream.cs` — `STREAM_01..05` pass.
- [x] `UnitTests/SidecarConfigGuard.cs` — `GUARD_01`, `GUARD_02` pass.
- [x] `ControllerTests/ManagerChatControllerTests.cs` — bổ sung `MCHATC_01`, `MCHATC_02`.
- [x] `IntegrationTests/ManagerChat.cs` — bổ sung `MCHAT_04`, `MCHAT_05` (test IDOR đầu tiên).

**Chạy được**
- [x] `run-chatbot-tests.ps1` chạy xanh cả 4 nhóm.
- [x] `pwsh ./run-chatbot-tests.ps1 -SkipIntegration` chạy được trên máy không có Docker,
      và **không báo "tất cả test đã pass"** khi có nhóm bị bỏ qua — chỉ báo
      "Các nhóm ĐÃ CHẠY đều pass. CHƯA CHẠY: ...".
- [x] CI đã có bước cài `requirements-dev.txt` và chạy `pytest`, đặt `if: always()` để kết quả
      Python không bị che khi test .NET đỏ.
- [x] Số test sau Stage 1: `UnitTests` 522 → **529**, `ControllerTests` 229 → **231**,
      `IntegrationTests` 351 → **353**, `AISidecar` 0 → **29**.

#### Kiểm chứng test không rỗng (mutation check)

Đã cấy đột biến rồi hoàn tác, xác nhận test THẬT SỰ đỏ:

| Đột biến | Test bắt được |
|---|---|
| Bỏ guard `not expected` trong `verify_internal_header` | `test_env_secret_vang_mat_van_chan`, `test_env_secret_rong_van_chan` |
| Dựng chain ở module level (không đặt tên biến `llm`) | `test_import_search_controller_khong_goi_get_llm` |
| Thêm lại route `sessions/{id}/message` dưới tên method `PostMessage` | `MCHATC_01` và `MCHAT_04` |
| Cấy file chứa `gemini-9.9-flash` ngoài vị trí cho phép | `test_khong_hard_code_ten_model_ngoai_vi_tri_cho_phep` |

Bốn đột biến này **đều lọt lưới ở bản test đầu tiên** — phát hiện qua vòng review đối kháng, đã
sửa lại test theo hướng kiểm hành vi/route thay vì kiểm tên biến, tên method.

#### Những chỗ phải làm khác với snippet trong mục 1.6

Snippet trong tài liệu được viết trước khi code Stage 1 hoàn tất nên có 6 chỗ không chạy được
nguyên xi:

| # | Vấn đề | Cách xử lý |
|---|---|---|
| 1 | `.gitignore` có `*.txt` + `!requirements.txt` → `requirements-dev.txt` bị ignore, CI sẽ không có file để `pip install` | Thêm `!requirements-dev.txt` vào `.gitignore` |
| 2 | Test .NET dùng `ChatRoles.User/.Ai` | Class thật tên `ChatRole` (xem DoD ở trên) — test viết theo `ChatRole` |
| 3 | `test_config_contract.py` bóc comment JSONC bằng regex → vỡ ở `appsettings.json:55` vì comment ở đó chứa dấu `"` | Thay bằng bộ bóc comment có nhận biết string literal |
| 4 | Regex `os.environ.get("MODEL", "...")` không khớp code sau mục 1.2 (đã đổi sang `... or "..."`) | Regex khớp cả hai dạng |
| 5 | Test đọc `WebAPI/appsettings.json` — file này bị gitignore nên **không tồn tại trên CI** | Rơi về `appsettings.Template.json`; `GUARD_02` quét mọi `appsettings*.json` đang có |
| 6 | `MCHAT_04/05` gán `CreateUserWithPermissionsAsync(...)` vào `token`, nhưng hàm này trả `ApplicationUser` | Thêm helper `CreateUserAndLoginAsync` gọi tiếp `AuthenticateAsync` |

Ngoài ra `_client.PostAsJsonAsync(..., ct)` bị **nhập nhằng** với một extension trùng chữ ký trong
repo → phải gọi tường minh `HttpClientJsonExtensions.PostAsJsonAsync(...)`.

#### Lỗi có sẵn phải sửa để 1.6 chạy được

`Program.cs:39` bỏ qua `AddInfrastructureServices` khi môi trường là `Test`, nhưng
`IntegrationTestWebAppFactory` **quên đăng ký lại** `IAiSidecarUrlProvider` và `IHttpClientFactory`
→ mọi handler ManagerChat 500 ngay ở bước DI. Vì vậy `MCHAT_03` đã đỏ từ trước Stage 1
(kiểm chứng bằng cách stash thay đổi rồi chạy lại). Đã thêm `FakeAiSidecarUrlProvider` +
`services.AddHttpClient()` vào factory.

> `PRODUCT_063` trong `IntegrationTests` vẫn đỏ — đã kiểm chứng là đỏ sẵn từ trước, không liên quan
> Stage 1. Full suite: 352/353 pass.

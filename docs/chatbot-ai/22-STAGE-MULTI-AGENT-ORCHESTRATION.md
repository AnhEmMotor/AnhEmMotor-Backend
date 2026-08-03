# Stage 22 — Multi-Agent: Sub-agent theo prompt động

> Bổ sung · Ưu tiên: 🟡 Trung bình · Ước lượng: 5–7 ngày
> Phụ thuộc cứng: **Stage 7** (kiến trúc sidecar), **Stage 8** (Run Engine / `ChatRunEvent`),
> **Stage 9** (`StateGraph` + `absorb_steering`), **Stage 11** (Reasoning Transparency —
> `ReasoningPanel.vue` để hiện khối lồng cho sub-agent), **Stage 13** (`ToolSpec` + Guardrails),
> **Stage 20** (Dynamic Tool Scoping).
>
> ⚠️ Sáu Stage phụ thuộc ở trên đều nằm trong `done/` — Stage này **chỉ đọc** chúng để tích hợp,
> **không sửa** file của chúng. Mọi điểm cần bổ sung vào Run Engine (event mới), Reasoning
> Transparency (panel lồng) hay Tool Scoping (tool luôn bật) được ghi ở đây như **việc cần làm khi
> triển khai**, không phải thay đổi đã áp vào các file `done/` đó.
>
> Vị trí trong lộ trình: đầu Đợt 5 (xem `00-OVERVIEW.md` mục 3, 4, 5) — sau khi Stage 20 (Đợt 3) và
> Stage 11 (Đợt 4) đã xong.

Mục tiêu: cho agent cha (Manager Agent, đồ thị đã xây từ Stage 9) khả năng **tự quyết định phân
việc** cho một hoặc vài sub-agent tạm thời khi câu hỏi có nhiều nhánh độc lập nhau. Điểm khác biệt
so với mọi cơ chế chọn-tool đã có (Stage 13 router, Stage 20 scoping): sub-agent **không** phải một
dàn agent chuyên biệt cố định (kiểu supervisor + worker đặt tên sẵn) — mỗi sub-agent được **sinh ra
bằng chính prompt do agent cha soạn tại thời điểm chạy**, làm xong một nhiệm vụ hẹp rồi biến mất.

---

## 22.1. Vấn đề — vì sao một agent là không đủ cho một số câu hỏi

Stage 20 đã giải quyết tốt bài toán "quá nhiều tool" bằng router + scoping (trần
`MAX_TOOLS_PER_REQUEST = 20`, `MAX_TOOLS_PER_MODULE = 10` — `done/13-STAGE-GUARDRAILS.md` mục
13.3b). Nhưng trần đó vẫn giả định **một luồng suy luận duy nhất** xử lý tuần tự mọi nhánh của câu
hỏi. Với câu hỏi thật sự có nhiều nhánh **độc lập nhau về chủ đề**, ví dụ:

> *"So sánh doanh thu, tồn kho sắp hết và số đơn trễ hạn của 3 chi nhánh trong quý này."*

một agent duy nhất phải nạp tool của cả `sales`, `inventory`, `logistics` vào cùng một ngữ cảnh,
gọi tuần tự qua nhiều vòng — vi phạm mục tiêu "trung vị số vòng agent ≤ 3" (Stage 14.1), và tăng
rủi ro trộn nhầm số liệu giữa các nhánh (đúng loại lỗi mà Stage 18.12 đã phải xử lý cho trường hợp
đơn giản hơn — nhiều kỳ trong 1 tool).

Catalog 91 tool (Stage 15) càng làm vấn đề rõ hơn: một câu hỏi đa nhánh có thể cần chạm tới nhiều
hơn 1 module mà Stage 13.3c cho phép nạp an toàn trong cùng một ngữ cảnh.

**Không phải giải pháp thay thế Stage 20** — là một lớp bổ sung, dùng khi agent cha *chủ động*
nhận ra một nhánh việc đủ độc lập để tách ra, không phải cơ chế chạy mặc định cho mọi câu hỏi.

---

## 22.2. Phạm vi — và cố tình không làm gì

Để giữ đúng tinh thần "không tự chế cái nặng hơn cần thiết" (Stage 7.8b), Stage này **cố tình**:

- **Không** dựng một dàn agent chuyên biệt cố định (vd "Inventory Agent", "Sales Agent" đặt tên
  sẵn, mỗi cái một system prompt viết tay). Theo đúng yêu cầu: sub-agent được **agent cha sinh
  prompt** cho từng lần gọi — số lượng, chủ đề của sub-agent hoàn toàn động.
- **Không** đệ quy nhiều tầng. Tối đa **1 cấp con**: cha sinh con, con không được sinh cháu. Đây là
  giới hạn cứng bằng thiết kế (registry của con không có tool `delegate_to_subagent`), không phải
  một bộ đếm runtime có thể bị vượt qua.
- **Không** thay thế Stage 13 Guardrails / Stage 20 Tool Scoping. Sub-agent (con) vẫn phải đi qua
  đúng router + scoping đó để chọn tool — chỉ khác là chạy trong một vòng lặp riêng, tách biệt
  ngữ cảnh với cha.
- **Không** cho sub-agent chạy tool ghi dữ liệu ở phiên bản đầu (xem lý do ở mục 22.5). Mọi hành
  động ghi vẫn phải qua agent cha + Plan Mode (Stage 10) như hiện tại.
- **Không** tạo `ChatRun` riêng cho mỗi sub-agent — toàn bộ hoạt động của sub-agent nằm trong
  **cùng một `ChatRun`** của cha, chỉ thêm event mới được gắn nhãn (mục 22.4). Tạo `ChatRun` riêng
  sẽ kéo theo giới hạn "1 run/user" (Stage 8.8), việc huỷ 2 chiều (Stage 18.3) và bộ đếm run — không
  cần thiết cho một tác vụ con sống vài giây.

---

## 22.3. Kiến trúc

```
Agent cha (StateGraph, Stage 9 — không đổi cấu trúc đồ thị)
   │
   call_model_node  →  LLM quyết định gọi delegate_to_subagent(...) × N (N ≤ 3)
   │
   call_tools_node  →  asyncio.gather chạy N lệnh gọi song song (CƠ CHẾ CÓ SẴN — Stage 14.2c,
   │                    không viết fan-out mới)
   │
   └─ mỗi lệnh gọi delegate_to_subagent thực thi:
         │
         create_react_agent(child_llm, child_tools)   ← Stage 3's prebuilt, KHÔNG dựng StateGraph
         │                                                riêng cho con (con không cần steering/
         │                                                plan-mode/nhiều lượt — một nhiệm vụ, một
         │                                                lượt, dùng xong bỏ)
         ├─ child_tools: đi qua ĐÚNG resolve_modules/build_tool_scope của Stage 20, luôn is_write=False
         ├─ RunSnapshot: dùng chung instance với cha theo run_id (Stage 18.2) — không tạo ảnh chụp
         │   dữ liệu riêng, tránh lệch asOf giữa cha và con
         └─ trả về DUY NHẤT một chuỗi text tổng hợp → nội dung ToolMessage của cha
              (con inputs/reasoning riêng của nó không lọt vào context của cha — chỉ kết quả)
```

### Tool mới: `delegate_to_subagent`

Đăng ký như mọi tool khác qua `ToolSpec` (`AISidecar/app/tools/registry.py`, quy ước ở
`done/13-STAGE-GUARDRAILS.md` mục 13.2), `module="orchestration"`, `is_write=False`:

```python
class DelegateToSubAgentArgs(BaseModel):
    objective: str = Field(
        ..., description="Nhiệm vụ cụ thể, ĐỘC LẬP với các nhánh khác — một câu rõ ràng."
    )
    context_note: str = Field(
        "", description="Bối cảnh cần thiết rút từ hội thoại chính, tối đa ~300 ký tự."
    )
    module_hint: list[str] = Field(
        default_factory=list,
        description="Gợi ý module tool (vd ['inventory']). Rỗng = để router con tự chọn.",
    )
```

Factory (minh hoạ, không phải code sản xuất):

```python
def make_delegate_tool(ctx: AuthContext, run_id: str, budget: SubAgentBudget) -> StructuredTool:
    async def _run(objective: str, context_note: str = "", module_hint: list[str] | None = None) -> str:
        if not budget.try_consume():
            return (f"[Từ chối] Đã đạt trần {MAX_SUBAGENTS_PER_TURN} sub-agent trong lượt này — "
                     "tổng hợp câu trả lời bằng dữ liệu đã có, không tách thêm nhánh nữa.")

        # Vẫn qua input guard — objective/context_note do LLM cha sinh ra, nhưng cha có thể lặp lại
        # nguyên văn nội dung do người dùng gõ (phòng thủ theo chiều sâu, cùng nguyên tắc Stage 13.2)
        objective = input_guard.scrub(objective)
        context_note = input_guard.scrub(context_note)

        modules = module_hint or await resolve_modules(objective, ctx.routing_ctx)   # Stage 20, dùng lại
        child_tools = build_tool_scope(ctx, modules, force_read_only=True)           # Stage 20, ép read-only

        child_llm = get_llm(temperature=0.3)
        child_agent = create_react_agent(child_llm, child_tools)                     # Stage 3's prebuilt

        system_prompt = render_prompt("system_subagent.md", note=context_note, server_date=ctx.server_date)
        try:
            result = await asyncio.wait_for(
                child_agent.ainvoke({"messages": [SystemMessage(system_prompt), HumanMessage(objective)]}),
                timeout=SUBAGENT_TIMEOUT_S,
            )
        except asyncio.TimeoutError:
            return f"[Sub-agent quá thời gian ({SUBAGENT_TIMEOUT_S}s)] Không có kết quả cho: {objective}"

        return result["messages"][-1].content

    return StructuredTool.from_function(coroutine=_run, name="delegate_to_subagent",
                                         args_schema=DelegateToSubAgentArgs)
```

> `budget` là một bộ đếm nhỏ theo `run_id` (reset mỗi lượt gọi `call_model_node`), không phải state
> LangGraph — không cần thêm field vào `AgentState` cho việc này.

### `delegate_to_subagent` luôn bật, không phụ thuộc router chọn module

Đây là một **quyết định cần chốt** của Stage này (theo đúng khuôn mẫu mục 5 của
`00-OVERVIEW.md`, áp dụng riêng cho Stage 22 — không sửa bảng quyết định gốc ở đó):

| # | Quyết định | Mặc định trong Stage này |
|---|---|---|
| 22-a | `delegate_to_subagent` có nằm trong tập tool router chọn theo module không? | **Không — luôn bật**, gia nhập `PINNED_TOOLS` (`done/20-STAGE-DYNAMIC-TOOL-SCOPING.md` mục 20.5) cùng `search_knowledge`, vì đây là công cụ điều phối, không thuộc một module nghiệp vụ cụ thể nào. Khi triển khai cần thêm dòng này vào `PINNED_TOOLS`, không sửa lại toàn bộ mục 20.5. |
| 22-b | Sub-agent (con) có được dùng tool ghi không? | **Không, ở phiên bản đầu** — xem 22.5 |
| 22-c | Đồ thị của con dùng `StateGraph` tự dựng hay `create_react_agent` prebuilt? | **`create_react_agent`** — con không cần steering/plan-mode, dùng bản Stage 3 là đủ, tránh dựng lại đồ thị |
| 22-d | Trần số sub-agent mỗi lượt | **`MAX_SUBAGENTS_PER_TURN = 3`** |
| 22-e | Con có `RunSnapshot` riêng không | **Không — dùng chung snapshot của cha** theo `run_id`, giữ `asOf` nhất quán (Stage 18.2) |

---

## 22.4. Sự kiện mới & luồng hiển thị

Toàn bộ hoạt động của sub-agent phát sinh **event mới trong cùng `ChatRunEvent`** của cha (không
tạo `ChatRun` riêng — xem 22.2), gắn thêm field `subagentId` trong `Payload` để FE nhóm lại:

| Event (mới) | Khi nào phát | Ghi chú |
|---|---|---|
| `subagent_started` | Ngay khi `delegate_to_subagent` bắt đầu chạy | Payload: `{subagentId, objective}` (đã qua input guard) |
| `subagent_thinking` | Con sinh `<suy_nghi>` (tái dùng cơ chế Stage 11.4) | Cùng `subagentId` |
| `subagent_tool_call` | Con gọi tool nội bộ | Tái dùng `make_tool_preview` của Stage 11.3 — **cùng nguyên tắc 2 đường dữ liệu** (18.11): bản đầy đủ chỉ vào `ToolMessage` của con, bản che vào event |
| `subagent_completed` | Con trả kết quả | Payload: `{subagentId, summary}` |
| `subagent_failed` | Timeout hoặc lỗi | Payload: `{subagentId, reason}` — **không** rò rỉ traceback nội bộ ra FE |

> Khi triển khai: bổ sung 5 dòng trên vào bảng canonical event type ở `done/08-STAGE-RUN-ENGINE.md`
> (nguyên tắc tương thích của bảng đó áp dụng nguyên vẹn: FE bỏ qua an toàn mọi `Type` lạ, nên
> triển khai Stage này trước khi FE cập nhật không làm hỏng client cũ).

**FE (`ReasoningPanel.vue`, Stage 11.6):** mỗi `subagentId` render thành một khối lồng thêm một cấp
bên trong panel suy nghĩ hiện có — mặc định thu gọn, tự mở khi đang chạy, **giống hệt hành vi đã có**
cho tool call thường (không thiết kế UI mới). Tái dùng nguyên quyết định 11.2 ("bỏ khái niệm mức
hiển thị — luôn Full, chỉ field nhạy cảm bị redact") cho nội dung của sub-agent.

---

## 22.5. Bảo mật & phân quyền

- **Không escalation:** con nhận **đúng** object permission của cha (`ctx.permissions`), không suy
  luận lại từ LLM. Test bắt buộc: `scope(tool con) ⊆ scope(tool cha)`.
- **Không tool ghi ở con (v1):** một sub-agent không tự "xin duyệt" được — cơ chế Plan Mode
  (Stage 10) gắn `plan_approved` vào state của **cha**, không có khái niệm tương đương cho một lệnh
  gọi tool ngắn hạn bên trong `delegate_to_subagent`. Nếu một nhánh việc cần hành động ghi, sub-agent
  chỉ **đề xuất bằng văn bản**, agent cha đọc đề xuất đó rồi tự gọi tool ghi (qua đúng luồng Plan
  Mode đã có, không đổi gì). Đây là điểm giảm phạm vi có chủ đích — nếu sau này cần con ghi trực
  tiếp, phải thiết kế lại cơ chế approval cho từng con, không mở khoá âm thầm.
- **Prompt injection qua `objective`/`context_note`:** hai trường này do LLM cha sinh ra, nhưng cha
  có thể lặp lại nguyên văn văn bản người dùng đã gõ (kể cả nội dung injection). Vẫn bắt buộc chạy
  qua `input_guard.scrub` y hệt input gốc của người dùng (phòng thủ theo chiều sâu — không tin
  tưởng ngầm định output của LLM cha).
- **Định nghĩa nghiệp vụ dùng chung:** system prompt của con phải mang đúng các quy tắc trong
  `GLOSSARY.md` (doanh thu, tồn kho, "tháng này"...) và quy tắc múi giờ VN (Stage 16.2) — tách phần
  này thành một fragment prompt dùng chung (`prompts/_business_definitions.md`) được cả
  `system_manager_chat.md` (cha) và `system_subagent.md` (con) include, tránh hai bản định nghĩa lệch
  nhau theo thời gian.

---

## 22.6. Chi phí, giới hạn & Observability

Mỗi `delegate_to_subagent` là **ít nhất 1 vòng LLM đầy đủ** thêm (bản thân con có thể lặp 1–3 vòng
ReAct) — cộng dồn trực tiếp vào chi phí/độ trễ của lượt chat. Không dựng cơ chế đo mới — cắm vào
đúng những gì Stage 14.1 và Stage 6.6 đã định nghĩa:

| Việc cần bổ sung khi triển khai | Vào đâu |
|---|---|
| Field `subagent_count`, `subagent_total_rounds` cho mỗi run | Bảng chỉ số Stage 14.1 (`14-STAGE-PERFORMANCE.md` mục 14.1) |
| `subagentId` trong `extra` của log JSON | `JsonFormatter` — `AISidecar/app/core/logging.py` (Stage 7.7) |
| `parent_run_id`/`subagent_id` vào metadata LangSmith | Đoạn `config = {"metadata": {...}}` ở Stage 6.6 |

**Giới hạn cứng** (không tái dùng timeout nào khác vì phạm vi khác nhau):
- `MAX_SUBAGENTS_PER_TURN = 3` — kiểm bằng bộ đếm `budget` trong factory (22.3).
- `SUBAGENT_TIMEOUT_S = 20` — mỗi sub-agent, riêng với trần vòng lặp chung của cha (Stage 13.6:
  6 vòng thường / 12 vòng plan mode) vì đây là một lời gọi tool, không phải một vòng agent của cha.
- Tổng thời gian chạy vẫn nằm trong timeout run hiện có (Stage 8) — **không** cần một tầng timeout
  "tổng cho mọi sub-agent" riêng, vì timeout run đã bao trùm toàn bộ lượt.

---

## 22.7. Khi nào dùng cái gì

| Tình huống | Dùng |
|---|---|
| Một chủ đề, dù cần nhiều tool | Router + scoping bình thường (Stage 13/20) — **không** delegate |
| Câu tiếp nối / follow-up | Fast path Stage 20.4 — không liên quan tới Stage này |
| Nhiều nhánh **độc lập hẳn về chủ đề** (khác module, không cần thông tin của nhau) | `delegate_to_subagent` cho mỗi nhánh, chạy song song |
| Cần ghi dữ liệu | Luôn qua agent cha + Plan Mode — sub-agent chỉ đề xuất bằng văn bản |
| Câu hỏi đơn giản, chào hỏi | Fast path Stage 14.2a — không liên quan |

Bổ sung một đoạn vào `system_manager_chat.md` khi triển khai, hướng dẫn khi nào nên gọi
`delegate_to_subagent` thay vì tự gọi tool tuần tự — ví dụ:

```markdown
Chỉ dùng delegate_to_subagent khi câu hỏi có từ 2 nhánh trở lên THẬT SỰ độc lập nhau (khác chủ đề,
không cần kết quả của nhau). Với câu hỏi một chủ đề, luôn tự gọi tool trực tiếp — không phân việc.
```

> **Không nhầm với song song hoá tool call (Stage 14.2c).** Cơ chế `asyncio.gather` trong
> `call_tools_node` là **có sẵn** và **được tái dùng nguyên vẹn** để chạy N lệnh gọi
> `delegate_to_subagent` song song — Stage này không viết cơ chế fan-out mới, chỉ thêm một tool mới
> chạy bên trong node đã có.

---

## 22.8. Testing

`AISidecar/tests/test_subagent.py` (theo đúng quy ước file test hiện có — xem
`06-STAGE-TESTING-OBSERVABILITY.md` mục 6.4):

- `test_tran_so_luong_subagent_moi_luot` — lệnh gọi `delegate_to_subagent` thứ 4 trong cùng một
  lượt bị từ chối rõ ràng, không âm thầm bỏ qua.
- `test_subagent_khong_the_de_quy` — registry tool của con **không chứa** `delegate_to_subagent`.
- `test_subagent_khong_co_tool_ghi` — không `ToolSpec` nào có `is_write=True` lọt vào scope của con.
- `test_permission_subagent_la_tap_con_cua_cha` — scope tool của con luôn là tập con của cha, không
  bao giờ có tool mà cha không có quyền.
- `test_ket_qua_subagent_khong_chua_dau_sao_khi_vao_llm` — cùng nguyên tắc test của Stage 18.11: nội
  dung trả về từ con đưa vào `ToolMessage` của cha không được chứa chuỗi `***`.
- `test_cau_hoi_don_gian_khong_kich_hoat_subagent` — case eval (mở rộng `evals/questions.yaml`
  Stage 6.5) xác nhận câu hỏi một chủ đề không gọi `delegate_to_subagent`.

`UnitTests/ManagerChatSubAgentEvents.cs` (nếu event mới cần test riêng phía .NET, theo quy ước file
phẳng — xem `06-STAGE-TESTING-OBSERVABILITY.md` mục 6.1):
- Event `subagent_*` được ghi/replay đúng thứ tự `Seq` như mọi `ChatRunEvent` khác — không cần
  bảng hay luồng ghi riêng.

---

## 22.9. Rủi ro

| Rủi ro | Giảm thiểu |
|---|---|
| Chi phí tăng vọt (mỗi lượt tối đa +3 vòng LLM) | Trần cứng `MAX_SUBAGENTS_PER_TURN = 3` + timeout riêng; theo dõi `subagent_count` (22.6) |
| Agent cha lạm dụng delegate cho câu hỏi đơn giản | Hướng dẫn rõ trong prompt (22.7) + eval case xác nhận không kích hoạt thừa |
| Rò rỉ quyền qua sub-agent | Test bắt buộc: scope(con) ⊆ scope(cha), không tool ghi (22.5, 22.8) |
| Trùng lặp với router/tool-scoping đã có (Stage 20) | Mục 22.7 định rõ ranh giới; con vẫn đi qua đúng `resolve_modules`/`build_tool_scope` |
| UI lồng nhiều tầng gây rối mắt | Tái dùng nguyên hành vi collapse/expand đã có ở Stage 11, không thiết kế UI mới (22.4) |
| Prompt injection qua `objective`/`context_note` | Vẫn qua `input_guard.scrub` như input gốc của người dùng (22.5) |
| Lệch `asOf` giữa số liệu cha và con | Con dùng chung `RunSnapshot` theo `run_id`, không tạo ảnh chụp riêng (22.3, 22.6 quyết định 22-e) |

---

## Definition of Done — Stage 22

- [ ] `delegate_to_subagent` đăng ký trong `tools/registry.py` như một `ToolSpec` bình thường
      (`module="orchestration"`, `is_write=False`), gia nhập `PINNED_TOOLS` của Stage 20.
- [ ] Trần `MAX_SUBAGENTS_PER_TURN = 3` có test; lệnh gọi vượt trần trả thông báo rõ ràng, không
      âm thầm bỏ qua.
- [ ] Depth = 1 tuyệt đối — test xác nhận registry của con không có `delegate_to_subagent`.
- [ ] Con chỉ nhận tool **read-only**; không `ToolSpec` nào có `is_write=True` lọt vào scope của con.
- [ ] Permission của con là **tập con** permission của cha — có test chống escalation.
- [ ] 5 event mới (`subagent_started/thinking/tool_call/completed/failed`) được bổ sung vào bảng
      canonical của `done/08-STAGE-RUN-ENGINE.md` khi triển khai; FE bỏ qua an toàn nếu chưa cập nhật.
- [ ] `ReasoningPanel.vue` hiện được khối lồng cho sub-agent, mặc định thu gọn/tự mở giống hành vi
      hiện có (Stage 11.6) — không có toggle "mức hiển thị" mới (giữ nguyên quyết định 11.2).
- [ ] Redaction cho log/event của sub-agent theo đúng nguyên tắc 2 đường dữ liệu (11.3/18.11): kết
      quả đầy đủ chỉ vào `ToolMessage`, bản che chỉ vào event ra FE.
- [ ] `subagent_count`, `subagent_total_rounds` xuất hiện trong log JSON và metadata LangSmith.
- [ ] Con dùng chung `RunSnapshot` của cha theo `run_id` — test xác nhận không tạo ảnh chụp riêng.
- [ ] Test: câu hỏi một chủ đề **không** kích hoạt `delegate_to_subagent` (tránh lạm dụng).
- [ ] Test: 3 sub-agent gọi trong cùng lượt chạy qua đúng `asyncio.gather` sẵn có (Stage 14.2c),
      không có cơ chế song song mới được viết thêm.
- [x] Đưa vào danh sách/thứ tự thực hiện của `00-OVERVIEW.md` (mục 3, 4, 5).

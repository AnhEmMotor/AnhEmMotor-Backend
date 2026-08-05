from typing import List, Optional
from pydantic import BaseModel, Field


class PlanChatOperation(BaseModel):
    type: str = Field(description="Loại thao tác: edit, add, remove, reorder, hoặc comment")
    step_id: Optional[str] = Field(default=None, description="id bước bị tác động — bắt buộc trừ khi type=add")
    title: Optional[str] = Field(default=None, description="Tiêu đề mới của bước (type=edit/add)")
    detail: Optional[str] = Field(default=None, description="Mô tả mới của bước (type=edit/add)")
    comment: Optional[str] = Field(default=None, description="Nội dung bình luận (type=comment)")
    order: Optional[int] = Field(default=None, description="Thứ tự mới (type=reorder)")


class PlanChatIntent(BaseModel):
    intent: str = Field(description="edit_plan nếu diễn giải được thành thao tác cụ thể, unclear nếu không đủ rõ")
    operations: List[PlanChatOperation] = Field(default_factory=list)
    reply: str = Field(description="Câu trả lời ngắn gọn, thân thiện xác nhận với người dùng bạn đã hiểu và sẽ làm gì")

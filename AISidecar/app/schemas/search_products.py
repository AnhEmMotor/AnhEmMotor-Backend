from typing import List, Optional
from pydantic import BaseModel, Field

class SearchIntent(BaseModel):
    keyword: Optional[str] = Field(description="Tên sản phẩm cụ thể nếu có, ví dụ: SH 150i, Air Blade, mũ bảo hiểm", default="")
    brand: Optional[str] = Field(description="Tên thương hiệu nếu có, ví dụ: Honda, Yamaha, Shoei", default="")
    category: Optional[str] = Field(description="Danh mục chính, ví dụ: Xe máy, Phụ tùng, Phụ kiện", default="")
    vehicleType: Optional[str] = Field(description="Loại xe nếu có, ví dụ: Xe ga, Xe số, Xe côn tay", default="")
    priceMin: Optional[int] = Field(description="Giá thấp nhất (VNĐ) nếu có", default=0)
    priceMax: Optional[int] = Field(description="Giá cao nhất (VNĐ) nếu có", default=60000000)
    colors: List[str] = Field(description="Danh sách màu sắc nếu có, ví dụ: Đỏ, Đen, Xanh", default=[])
    intent: str = Field(description="Ý định người dùng (search, unknown)", default="search")

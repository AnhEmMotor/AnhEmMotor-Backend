class SidecarError(Exception):
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

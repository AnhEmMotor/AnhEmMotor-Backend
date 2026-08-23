from fastapi import APIRouter, Depends
from dependencies import verify_internal_token

router = APIRouter()

@router.post("/test-role")
def test_role(request_data: dict, token: str = Depends(verify_internal_token)):
    user_id = request_data.get("userId", None)
    roles = request_data.get("roles", [])
    
    response_msg = f"AI Sidecar received test. User: {user_id}, Roles: {', '.join(roles)}"
    
    return {"result": response_msg, "status": "success"}

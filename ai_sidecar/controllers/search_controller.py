from fastapi import APIRouter, Depends
from dependencies import verify_internal_token

router = APIRouter()

@router.post("/search")
def search(request_data: dict, token: str = Depends(verify_internal_token)):
    keyword = request_data.get("keyword", "")
    user_id = request_data.get("userId", None)
    
    response_msg = f"AI Search processed for keyword: '{keyword}'. Requested by User ID: {user_id}"
    
    return {"result": response_msg, "status": "success"}

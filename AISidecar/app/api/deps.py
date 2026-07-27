from fastapi import Header, HTTPException
from app.config import get_settings

def verify_internal_secret(x_internal_secret: str | None = Header(None)):
    expected = get_settings().backend_internal_secret
    if not expected or x_internal_secret != expected:
        raise HTTPException(status_code=403, detail="Invalid internal secret")
    return x_internal_secret

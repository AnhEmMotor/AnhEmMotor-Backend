import os
from fastapi import Header, HTTPException


def verify_internal_secret(x_internal_secret: str | None = Header(None)):
    expected = os.environ.get("BACKEND_INTERNAL_SECRET", "")
    if not expected or x_internal_secret != expected:
        raise HTTPException(status_code=403, detail="Invalid internal secret")
    return x_internal_secret

import os
from fastapi import HTTPException, Depends
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials

security = HTTPBearer()
BACKEND_INTERNAL_SECRET = os.environ.get("BACKEND_INTERNAL_SECRET", "default_secret_if_not_set")

def verify_internal_token(credentials: HTTPAuthorizationCredentials = Depends(security)):
    token = credentials.credentials
    if token != BACKEND_INTERNAL_SECRET:
        raise HTTPException(status_code=403, detail="Invalid internal secret")
    return token

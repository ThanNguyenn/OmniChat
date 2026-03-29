import os
from fastapi import Depends, HTTPException, status
from fastapi.security import APIKeyHeader

# API_KEY = os.getenv("API_KEY")
# API_KEY_NAME = os.getenv("API_KEY_NAME", "x-api-key")

API_KEY = "YmVsb25nc3RhbmRhcmR3YXNoc2FpZGNsb3RoZXN3b3JzZWFsb3VkZ2F2ZWNvb2tkaXI="
API_KEY_NAME = "omni-chat-api-key"

api_key_header = APIKeyHeader(name=API_KEY_NAME, auto_error=False)

def verify_api_key(api_key: str = Depends(api_key_header)):
    if api_key != API_KEY:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Unauthorized"
        )
from fastapi import FastAPI
import os
import uvicorn
from controllers import test_controller, search_controller, manager_chat_controller

app = FastAPI()

app.include_router(test_controller.router)
app.include_router(search_controller.router)
app.include_router(manager_chat_controller.router)

PORT = int(os.environ.get("PORT", 8000))

@app.get("/")
def read_root():
    return {"status": "ok", "message": "AI Sidecar is running"}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=PORT)

from fastapi import FastAPI
from api.api import router

app = FastAPI(title="Intent Classification Service")

app.include_router(router)
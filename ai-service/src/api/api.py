from fastapi import APIRouter, Depends
from schemas.ner_schemas import OrderResponse
from schemas.predict_schemas import PredictRequest, PredictResponse
from services.model_service import IntentClassifier
# from services.ner_service import NERService
from core.security import verify_api_key
import logging
logger = logging.getLogger("uvicorn")
router = APIRouter()

classifier = IntentClassifier()
# ner_service = NERService()
@router.post("/predict", dependencies=[Depends(verify_api_key)], response_model=PredictResponse)
def predict(request: PredictRequest):
    logger.info(f"Incoming text: {request.text}")

    intents, details = classifier.predict(request.text)
    return PredictResponse(intents=intents, details=details)
# @router.post("/ner", dependencies=[Depends(verify_api_key)], response_model=OrderResponse)
# def ner(request: PredictRequest):
#     result = ner_service.extract_order(request.text)
#     return result
from fastapi import APIRouter, Depends
from schemas.ner_schemas import OrderResponse
from schemas.predict_schemas import PredictRequest, PredictResponse
from services.model_service import IntentClassifier
# from services.ner_service import NERService
from core.security import verify_api_key

router = APIRouter()

classifier = IntentClassifier()
# ner_service = NERService()
@router.post("/predict", dependencies=[Depends(verify_api_key)], response_model=PredictResponse)
def predict(request: PredictRequest):
    intents, details = classifier.predict(request.text)
    return PredictResponse(intents=intents, details=details)

# @router.post("/ner", dependencies=[Depends(verify_api_key)], response_model=OrderResponse)
# def ner(request: PredictRequest):
#     result = ner_service.extract_order(request.text)
#     return result
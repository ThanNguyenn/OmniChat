from fastapi import APIRouter, Depends
from schemas.schemas import PredictRequest, PredictResponse
from services.model_service import IntentClassifier
from core.security import verify_api_key

router = APIRouter()

classifier = IntentClassifier()

@router.post("/predict", dependencies=[Depends(verify_api_key)], response_model=PredictResponse)
def predict(request: PredictRequest):
    intents, details = classifier.predict(request.text)
    return PredictResponse(intents=intents, details=details)
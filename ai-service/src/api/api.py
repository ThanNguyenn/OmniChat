from fastapi import APIRouter
from schemas.schemas import PredictRequest, PredictResponse
from services.model_service import IntentClassifier

router = APIRouter()

classifier = IntentClassifier()

@router.post("/predict", response_model=PredictResponse)
def predict(request: PredictRequest):
    intents, details = classifier.predict(request.text)
    return PredictResponse(intents=intents, details=details)
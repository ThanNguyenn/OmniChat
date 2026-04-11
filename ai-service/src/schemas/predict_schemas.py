from pydantic import BaseModel
from typing import List

class PredictRequest(BaseModel):
    text: str

class LabelResult(BaseModel):
    label: str
    confidence: float
    threshold: float
    predicted: bool

class PredictResponse(BaseModel):
    intents: List[str]
    details: List[LabelResult]
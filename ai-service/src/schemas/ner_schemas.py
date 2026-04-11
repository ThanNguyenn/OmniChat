from pydantic import BaseModel
from pydantic import BaseModel
from typing import List, Optional

class NERRequest(BaseModel):
    message: str

class NERToken(BaseModel):
    token: str
    label: str

class OrderItem(BaseModel):
    brand: str
    volume_ml: Optional[int]
    product_kind: str
    quantity: int


class OrderResponse(BaseModel):
    items: List[OrderItem]
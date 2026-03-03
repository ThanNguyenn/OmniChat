import torch


class Settings:
    # MODEL_PATH = "./nlp/final_model"
    MODEL_PATH = "Quuko/omni-chat-intent-classification"
    BASE_MODEL = "vinai/phobert-base"
    DEVICE = "cuda" if torch.cuda.is_available() else "cpu"

    LABELS = [
        "ORDER_CREATION",
        "POST_SALE_CHANGE",
        "PRE_SALE",
        "PAYMENT",
        "ORDER_STATUS",
    ]

    THRESHOLDS = {
        "ORDER_CREATION": 0.52,
        "POST_SALE_CHANGE": 0.64,
        "PRE_SALE": 0.72,
        "PAYMENT": 0.37,
        "ORDER_STATUS": 0.22,
    }

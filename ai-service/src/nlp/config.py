import torch


class Settings:
    # MODEL_PATH = "./nlp/final_model"
    PREDICT_MODEL_PATH = "Quuko/omni-chat-intent-classification"
    BASE_MODEL = "vinai/phobert-base"
    NER_MODEL_PATH = "./nlp/ner_model"
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

    NER_LABELS = [
        "O",
        "B-QUANTITY",
        "I-QUANTITY",
        "B-VOLUME",
        "I-VOLUME",
        "B-UNIT",
        "I-UNIT",
        "B-BRAND",
        "I-BRAND",
        "B-KIND",
        "I-KIND",
    ]

    label2id = {l: i for i, l in enumerate(NER_LABELS)}
    id2label = {i: l for l, i in label2id.items()}

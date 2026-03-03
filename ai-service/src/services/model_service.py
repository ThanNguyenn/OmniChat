import torch
from transformers import AutoTokenizer, AutoModelForSequenceClassification
from underthesea import word_tokenize
from nlp.config import Settings

class IntentClassifier:

    def __init__(self):
        self.device = Settings.DEVICE
        self.labels = Settings.LABELS
        self.thresholds = Settings.THRESHOLDS

        self.tokenizer = AutoTokenizer.from_pretrained(Settings.BASE_MODEL)
        self.model = AutoModelForSequenceClassification.from_pretrained(Settings.MODEL_PATH)
        self.model.to(self.device)
        self.model.eval()

    def _segment_text(self, text: str) -> str:
        return word_tokenize(text, format="text")

    def _predict_proba(self, text: str):
        segmented = self._segment_text(text)

        inputs = self.tokenizer(
            segmented,
            return_tensors="pt",
            truncation=True,
            padding=True,
            max_length=256
        )

        inputs = {k: v.to(self.device) for k, v in inputs.items()}

        with torch.no_grad():
            outputs = self.model(**inputs)
            logits = outputs.logits
            probs = torch.sigmoid(logits).cpu().numpy()[0]

        return probs

    def predict(self, text: str):
        probs = self._predict_proba(text)

        predicted_intents = []
        details = []

        for i, label in enumerate(self.labels):
            confidence = float(probs[i])
            threshold = self.thresholds[label]
            is_predicted = confidence >= threshold

            if is_predicted:
                predicted_intents.append(label)

            details.append({
                "label": label,
                "confidence": confidence,
                "threshold": threshold,
                "predicted": is_predicted
            })

        return predicted_intents, details
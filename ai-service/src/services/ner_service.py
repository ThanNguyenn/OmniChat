import torch
from transformers import AutoTokenizer, AutoModelForTokenClassification
from underthesea import word_tokenize
from nlp.config import Settings
import re

class NERService:
    def __init__(self):
        self.tokenizer = AutoTokenizer.from_pretrained(Settings.NER_MODEL_PATH)
        self.model = AutoModelForTokenClassification.from_pretrained(
            Settings.NER_MODEL_PATH
        ).to(Settings.DEVICE)

        self.model.eval()

    # =========================
    # PUBLIC ENTRY
    # =========================
    def extract_order(self, text: str):
        tokens = self._preprocess(text)
        ner_output = self._predict(tokens)
        items = self._group_entities(ner_output)
        items = [self._normalize_item(i) for i in items]

        return {"items": items}

    # =========================
    # STEP 1: PREPROCESS
    # =========================
    def _normalize_tokens(self, tokens: list[str]):
        normalized = []
        i = 0

        while i < len(tokens):
            tok = tokens[i]

            if i < len(tokens) - 1:
                next_tok = tokens[i + 1]
                if re.fullmatch(r"\d+", tok) and next_tok == "lít":
                    normalized.append(f"{tok}_lít")
                    i += 2
                    continue

            if i < len(tokens) - 1:
                if tok == "không" and tokens[i + 1] == "đường":
                    normalized.append("không_đường")
                    i += 2
                    continue

                if tok == "sữa" and tokens[i + 1] == "chua":
                    normalized.append("sữa_chua")
                    i += 2
                    continue

            if tok == "st":
                normalized.extend(["sữa", "tươi"])
                i += 1
                continue

            normalized.append(tok)
            i += 1

        return normalized

    def _preprocess(self, text: str):
        text = text.lower().strip()

        segmented = word_tokenize(text, format="text")
        tokens = segmented.split()

        tokens = self._normalize_tokens(tokens)
        return tokens
    # =========================
    # STEP 2: NER PREDICTION
    # =========================
    def _predict(self, tokens: list[str]):
        encoding = self.tokenizer(
            tokens,
            is_split_into_words=True,
            return_tensors="pt",
            truncation=True,
            padding=True
        )

        word_ids = encoding.word_ids(batch_index=0)

        inputs = {k: v.to(Settings.DEVICE) for k, v in encoding.items()}

        with torch.no_grad():
            outputs = self.model(**inputs)

        logits = outputs.logits
        preds = torch.argmax(logits, dim=-1)[0].cpu().tolist()

        results = []
        prev_word_idx = None

        for idx, word_idx in enumerate(word_ids):
            if word_idx is None:
                continue

            if word_idx != prev_word_idx:
                token = tokens[word_idx]
                label = Settings.id2label[preds[idx]]
                results.append((token, label))

            prev_word_idx = word_idx

        return results

    # =========================
    # STEP 3: GROUP ENTITIES
    # =========================
    def _group_entities(self, ner_output):
        items = []
        current = {}

        for token, label in ner_output:
            if label == "B-QUANTITY":
                if current:
                    items.append(current)
                    current = {}

                try:
                    current["quantity"] = int(token)
                except:
                    current["quantity"] = 1

            elif label == "B-VOLUME":
                current["volume"] = token

            elif label == "B-UNIT":
                current["unit"] = token

            elif label == "B-BRAND":
                current["brand"] = token

            elif label == "B-KIND":
                current["kind"] = token

        if current:
            items.append(current)

        return items

    # =========================
    # STEP 4: NORMALIZATION
    # =========================
    def _normalize_item(self, item):
        volume_map = {
            "1_lít": 880,
            "2_lít": 1760
        }

        return {
            "brand": item.get("brand", "long_thanh"),
            "volume_ml": volume_map.get(item.get("volume")),
            "product_kind": item.get("kind", "sugar"),
            "quantity": item.get("quantity", 1)
        }
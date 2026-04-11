import json
import numpy as np
import torch
from datasets import Dataset
from transformers import (
    AutoTokenizer,
    AutoModelForTokenClassification,
    TrainingArguments,
    Trainer
)
from sklearn.metrics import f1_score

# =========================
# 1. LABELS (match config)
# =========================
label_list = [
    "O",
    "B-QUANTITY", "I-QUANTITY",
    "B-VOLUME", "I-VOLUME",
    "B-UNIT", "I-UNIT",
    "B-BRAND", "I-BRAND",
    "B-KIND", "I-KIND"
]

label2id = {l: i for i, l in enumerate(label_list)}
id2label = {i: l for l, i in label2id.items()}

# =========================
# 2. LOAD DATA
# =========================
# Expected format:
# [
#   {
#     "tokens": ["2", "chai", "1_lít"],
#     "ner_tags": ["B-QUANTITY", "B-UNIT", "B-VOLUME"]
#   }
# ]

with open("ner_data.json", "r", encoding="utf-8") as f:
    raw_data = json.load(f)

def encode(example):
    example["labels"] = [label2id[tag] for tag in example["ner_tags"]]
    return example

dataset = Dataset.from_list(raw_data)
dataset = dataset.map(encode)

dataset = dataset.train_test_split(test_size=0.2, seed=42)

# =========================
# 3. TOKENIZER
# =========================
tokenizer = AutoTokenizer.from_pretrained("vinai/phobert-base")

def tokenize_and_align_labels(examples):
    tokenized = tokenizer(
        examples["tokens"],
        is_split_into_words=True,
        truncation=True,
        padding="max_length",
        max_length=128
    )

    all_labels = []

    for i, labels in enumerate(examples["labels"]):
        word_ids = tokenized.word_ids(batch_index=i)

        previous_word_idx = None
        label_ids = []

        for word_idx in word_ids:
            if word_idx is None:
                label_ids.append(-100)

            elif word_idx != previous_word_idx:
                label_ids.append(labels[word_idx])

            else:
                # convert B → I
                current = labels[word_idx]
                if current % 2 == 1:
                    label_ids.append(current + 1)
                else:
                    label_ids.append(current)

            previous_word_idx = word_idx

        all_labels.append(label_ids)

    tokenized["labels"] = all_labels
    return tokenized


dataset = dataset.map(tokenize_and_align_labels, batched=True)

dataset.set_format(
    type="torch",
    columns=["input_ids", "attention_mask", "labels"]
)

# =========================
# 4. MODEL
# =========================
model = AutoModelForTokenClassification.from_pretrained(
    "vinai/phobert-base",
    num_labels=len(label_list),
    id2label=id2label,
    label2id=label2id
)

# =========================
# 5. METRICS
# =========================
def compute_metrics(p):
    predictions, labels = p
    predictions = np.argmax(predictions, axis=2)

    true_labels = []
    true_preds = []

    for pred, lab in zip(predictions, labels):
        for p_i, l_i in zip(pred, lab):
            if l_i != -100:
                true_labels.append(l_i)
                true_preds.append(p_i)

    return {
        "f1": f1_score(true_labels, true_preds, average="macro")
    }

# =========================
# 6. TRAINING
# =========================
training_args = TrainingArguments(
    output_dir="./ner_results",
    eval_strategy="epoch",
    save_strategy="epoch",
    learning_rate=2e-5,
    per_device_train_batch_size=16,
    per_device_eval_batch_size=16,
    num_train_epochs=8,
    weight_decay=0.01,
    logging_steps=10,
    load_best_model_at_end=True,
    metric_for_best_model="f1",
    greater_is_better=True,
    fp16=True
)

trainer = Trainer(
    model=model,
    args=training_args,
    train_dataset=dataset["train"],
    eval_dataset=dataset["test"],
    tokenizer=tokenizer,
    compute_metrics=compute_metrics
)

# =========================
# 7. TRAIN
# =========================
trainer.train()

trainer.save_model("./nlp/ner_model")
tokenizer.save_pretrained("./nlp/ner_model")
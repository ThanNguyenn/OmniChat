import json
import numpy as np
import torch
from datasets import Dataset
import time
from underthesea import word_tokenize
from transformers import (
    AutoTokenizer,
    AutoModelForSequenceClassification,
    TrainingArguments,
    Trainer,
    EarlyStoppingCallback
)
from sklearn.metrics import f1_score, classification_report

# =========================
# 1. DEFINE LABEL SPACE
# ========================= 

label_list = [
    "ORDER_CREATION",
    "POST_SALE_CHANGE",
    "PRE_SALE",
    "PAYMENT",
    "ORDER_STATUS"
]

label2id = {label: i for i, label in enumerate(label_list)}
id2label = {i: label for label, i in label2id.items()}


# =========================
# 2. LOAD DATA
# =========================

file_path = r"C:\Users\THAN\Downloads\ClassifyIntent2.json"

with open(file_path, "r", encoding="utf-8") as f:
    raw_data = json.load(f)


def encode_labels(intent_list):
    vector = [0] * len(label_list)
    for intent in intent_list:
        vector[label2id[intent]] = 1
    return [float(v) for v in vector]

def segment_text(text):
    return word_tokenize(text, format="text")

processed_data = []

for item in raw_data:
    segmented = segment_text(item["text"])
    processed_data.append({
        "text": segmented,
        "labels": encode_labels(item["intent"])
    })

dataset = Dataset.from_list(processed_data)

dataset = dataset.train_test_split(test_size=0.2, seed=42)
train_dataset = dataset["train"]
val_dataset = dataset["test"]


# =========================
# 3. TOKENIZER
# =========================

tokenizer = AutoTokenizer.from_pretrained("vinai/phobert-base")


def tokenize_function(example):
    return tokenizer(
        example["text"],
        padding="max_length",
        truncation=True,
        max_length=128
    )


train_dataset = train_dataset.map(tokenize_function, batched=True)
val_dataset = val_dataset.map(tokenize_function, batched=True)

train_dataset.set_format(
    type="torch",
    columns=["input_ids", "attention_mask", "labels"]
)
val_dataset.set_format(
    type="torch",
    columns=["input_ids", "attention_mask", "labels"]
)


# =========================
# 4. MODEL
# =========================

model = AutoModelForSequenceClassification.from_pretrained(
    "vinai/phobert-base",
    num_labels=len(label_list),
    id2label=id2label,
    label2id=label2id,
    problem_type="multi_label_classification"
)


# =========================
# 5. METRICS
# =========================

THRESHOLD = 0.5

def compute_metrics(eval_pred):
    logits, labels = eval_pred
    probs = torch.sigmoid(torch.from_numpy(logits))
    preds = (probs > THRESHOLD).int().numpy()

    macro_f1 = f1_score(labels, preds, average="macro")
    micro_f1 = f1_score(labels, preds, average="micro")

    return {
        "macro_f1": macro_f1,
        "micro_f1": micro_f1
    }

# =========================
# 6. TRAINING ARGUMENTS
# =========================

training_args = TrainingArguments(
    output_dir=f"./results/run_{int(time.time())}",
    eval_strategy="epoch",
    save_strategy="epoch",
    save_total_limit=1,
    learning_rate=2e-5,
    per_device_train_batch_size=16,
    per_device_eval_batch_size=16,
    num_train_epochs=8,
    weight_decay=0.01,
    logging_dir="./logs",
    logging_strategy="steps",
    logging_steps=5,                 
    report_to="tensorboard",         
    load_best_model_at_end=True,
    metric_for_best_model="micro_f1",   
    greater_is_better=True,
    fp16=True,
    warmup_ratio=0.1
)


# =========================
# 7. TRAINER
# =========================

trainer = Trainer(
    model=model,
    args=training_args,
    train_dataset=train_dataset,
    eval_dataset=val_dataset,
    compute_metrics=compute_metrics,
    callbacks=[EarlyStoppingCallback(early_stopping_patience=2)]
)


# =========================
# 8. TRAIN
# =========================

trainer.train()
trainer.save_model("./final_model")
tokenizer.save_pretrained("./final_model")

# =========================
# 9. THRESHOLD TUNING
# =========================

print("\nTuning threshold...")

pred_output = trainer.predict(val_dataset)
logits = pred_output.predictions
labels = pred_output.label_ids

probs = torch.sigmoid(torch.tensor(logits))

num_labels = len(label_list)
best_thresholds = np.zeros(num_labels)


for i in range(num_labels):
    best_t = 0.5 
    best_score = 0
    
    for t in np.arange(0.05, 0.95, 0.01):
        current_preds = (probs[:, i] > t).int().numpy()
        score = f1_score(labels[:, i], current_preds)
        
        if score > best_score:
            best_score = score
            best_t = t
    
    best_thresholds[i] = best_t

print("Best thresholds per label:")
for label, t in zip(label_list, best_thresholds):
    print(label, round(t, 2))

with open("./final_model/thresholds.json", "w") as f:
    json.dump(
        {label: float(t) for label, t in zip(label_list, best_thresholds)},
        f,
        indent=2
    )


# =========================
# 10. FINAL REPORT
# =========================

final_preds = np.zeros_like(probs.numpy())

for i in range(num_labels):
    final_preds[:, i] = (probs[:, i] > best_thresholds[i]).int()

print("\nClassification Report:")
print(classification_report(labels, final_preds, target_names=label_list))

print("\nFinal Evaluation with tuned threshold:")
print("Macro F1:", f1_score(labels, final_preds, average="macro"))
print("Micro F1:", f1_score(labels, final_preds, average="micro"))
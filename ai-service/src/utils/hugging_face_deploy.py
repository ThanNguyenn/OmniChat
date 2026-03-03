from huggingface_hub import HfApi, login

# This will automatically use the token from your ENV
api = HfApi() 
repo_id = "Quuko/omni-chat-intent-classification"

# 1. Ensure repo exists
api.create_repo(repo_id=repo_id, exist_ok=True)

# 2. Upload the folder
print("Uploading folder to Hugging Face...")
api.upload_folder(
    folder_path="../nlp/final_model",
    repo_id=repo_id,
    repo_type="model"
)
print(f"Done! View your model at https://huggingface.co/{repo_id}")

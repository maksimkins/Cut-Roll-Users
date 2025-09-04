from transformers import AutoTokenizer, AutoModel
import torch

model_name = "sentence-transformers/all-MiniLM-L6-v2"
save_dir = "./onnx_model"

# Load tokenizer and model
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModel.from_pretrained(model_name)

# Dummy input
dummy = tokenizer("Hello world!", return_tensors="pt")

# Export
torch.onnx.export(
    model,
    (dummy["input_ids"], dummy["attention_mask"]),
    f"{save_dir}/model.onnx",
    input_names=["input_ids", "attention_mask"],
    output_names=["last_hidden_state", "pooler_output"],
    dynamic_axes={
        "input_ids": {0: "batch", 1: "sequence"},
        "attention_mask": {0: "batch", 1: "sequence"},
    },
    opset_version=14
)

# Save tokenizer for ONNX runtime use
tokenizer.save_pretrained(save_dir)

print(f"ONNX model saved in {save_dir}")

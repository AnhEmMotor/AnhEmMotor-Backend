import ast
import tokenize
import os
from pathlib import Path

base_dir = Path(r"C:\Users\BINH\Documents\Documents\AnhEmMotorProject\AnhEmMotor-Backend\AISidecar\app")
py_files = []
for root, dirs, files in os.walk(base_dir):
    if 'venv' in root or '.venv' in root or '__pycache__' in root or 'tests' in root:
        continue
    for f in files:
        if f.endswith('.py'):
            py_files.append(os.path.join(root, f))

for filepath in py_files:
    with open(filepath, 'r', encoding='utf-8') as f:
        source = f.read()
        
    tree = ast.parse(source)
    functions = [n for n in ast.walk(tree) if isinstance(n, (ast.FunctionDef, ast.AsyncFunctionDef))]
    
    with open(filepath, 'rb') as f:
        tokens = tokenize.tokenize(f.readline)
        comments = [tok for tok in tokens if tok.type == tokenize.COMMENT]
        
    for func in functions:
        doc = ast.get_docstring(func)
        if doc:
            print(f"DOCSTRING: {filepath}:{func.lineno}")
        start = func.lineno
        end = func.end_lineno
        for comment in comments:
            if start <= comment.start[0] <= end:
                if 'type:' not in comment.string:
                    print(f"COMMENT: {filepath}:{comment.start[0]}: {comment.string}")

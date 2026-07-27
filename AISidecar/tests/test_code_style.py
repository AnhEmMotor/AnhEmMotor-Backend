import ast
import tokenize
import os
import glob
from pathlib import Path

def test_no_comments_or_docstrings_in_functions():
    base_dir = Path(__file__).parent.parent
    self_path = Path(__file__).resolve()
    py_files = []
    for root, dirs, files in os.walk(base_dir):
        dirs[:] = [d for d in dirs if d not in ('venv', '.venv', '__pycache__')]
        for f in files:
            if f.endswith('.py'):
                filepath = Path(root) / f
                if filepath.resolve() != self_path:
                    py_files.append(filepath)

    errors = []

    for filepath in py_files:
        with open(filepath, 'r', encoding='utf-8') as f:
            source = f.read()

        tree = ast.parse(source)
        functions = [n for n in ast.walk(tree) if isinstance(n, (ast.FunctionDef, ast.AsyncFunctionDef))]

        for func in functions:
            if ast.get_docstring(func):
                errors.append(f"{filepath}: Function '{func.name}' has a docstring at line {func.lineno}")

        with open(filepath, 'rb') as f:
            tokens = tokenize.tokenize(f.readline)
            comments = [tok for tok in tokens if tok.type == tokenize.COMMENT]

        for func in functions:
            start = func.lineno
            end = func.end_lineno
            for comment in comments:
                if start <= comment.start[0] <= end:
                    errors.append(f"{filepath}: Function '{func.name}' has a comment '{comment.string}' at line {comment.start[0]}")

    if errors:
        error_msg = "Found comments or docstrings inside functions:\n" + "\n".join(errors)
        import pytest
        pytest.fail(error_msg, pytrace=False)

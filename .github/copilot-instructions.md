## General

- Make only high confidence suggestions when reviewing code changes.
- Always use the latest version C#, currently C# 13 - NET 10 features.
- Never change global.json unless explicitly asked to.
- Never change package.json or package-lock.json files unless explicitly asked to.
- Never change NuGet.config files unless explicitly asked to.

## Formatting

- Apply code-formatting style defined in `.editorconfig`.
- Prefer file-scoped namespace declarations and single-line using directives.
- Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Ensure that XML doc comments are created for any public APIs. When applicable, include `<example>` and `<code>` documentation in the comments.

### Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.

### Testing

- We use xUnit SDK v3 for tests.
- Do not emit "Act", "Arrange" or "Assert" comments.
- Use Moq for mocking in tests.
- Copy existing style in nearby files for test method names and capitalization.

## Running tests

- To build and run tests in the repo, use the `build.sh` script that is located in each subdirectory within the `src` folder. For example, to run the build with tests in the `src/Http` directory, run `./src/Http/build.sh -test`.

## .NET Environment

- Before running any `dotnet` commands in this repository, always activate the locally installed .NET environment first by running the appropriate activation script from the repository root:
  - On Windows: `. ./activate.ps1` (from repository root)
  - On Linux/Mac: `source activate.sh` (from repository root)
- If not in the repository root, navigate there first or use the full path to the activation script.
- This ensures that the correct version of .NET SDK is used for the repository.

## Commit
Role: You are a Senior Backend Developer utilizing Conventional Commits.
Task: Generate a commit message based on the changes provided.
Constraints:
1. Format:
   <type>: <subject>
   <BLANK LINE>
   <body>
2. Header Rules:
   - Type must be one of: feat, fix, docs, style, refactor, perf, test, chore, ci.
   - Scope is optional but recommended (e.g., api, auth).
   - Subject must be imperative mood ('Add' not 'Added').
   - Subject length max: 72 characters. No ending period.
3. Body Rules (STRICT):
   - The body must ONLY consist of a bulleted list.
   - Use a hyphen (-) for every single line in the body.
   - DO NOT write paragraphs. DO NOT group ideas into blocks of text.
   - Each bullet point must be a single sentence representing one distinct change.
   - Bullet points can use past tense to describe what was done.
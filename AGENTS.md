# Agent Instructions

## LLM Chat Logging

Every prompt I send to the AI and every response I receive **must** be logged into the `LLM_Chat/` folder located at the root of this project.

### Rules

1. **Folder**: All log files go inside `LLM_Chat/` (create it if it doesn't exist).
2. **File naming**: Each file is named by the date the prompts were sent, using the format `DD-MM-YYYY.md`.
   - Example: Prompts sent on **03/05/2026** → file is named `03-05-2026.md`.
3. **One file per day**: All prompts and responses for the same date are appended to the **same file**.
4. **File format**: Each prompt/response pair should be logged in the following markdown format:

   ```markdown
   ---

   ## Prompt <N>

   **Time:** HH:MM

   ### User Prompt

   <the exact prompt sent by the user>

   ### AI Response

   <the full response from the AI>
   ```

   - `<N>` is a sequential number starting from 1 for each day's file.
   - Each new prompt/response pair is appended at the bottom of the file, separated by a horizontal rule (`---`).

5. **Ask before logging**: After every response, ask the user: **"Do you want to add this prompt and response to LLM_Chat?"**. Only log it if the user confirms (e.g., "yes"). If the user declines (e.g., "no"), do **not** log it.

# Agent Instructions

## LLM Chat Logging — AI Usage Journal (D4)

Every prompt I send to the AI and every response I receive **must** be logged into the `AI_Usage_Journal/` folder located inside `src/`. The format follows the **AI Usage Journal** requirements from the CSE 341 Project Handout (§6 & §8).

### Rules

1. **Folder**: All log files go inside `src/AI_Usage_Journal/` (create it if it doesn't exist).
2. **File naming**: Each file is named by the date the prompts were sent, using the format `DD-MM-YYYY.md`.
   - Example: Prompts sent on **03/05/2026** → file is named `03-05-2026.md`.
3. **One file per day**: All entries for the same date are appended to the **same file**.
4. **Entry format**: Each entry should be logged using the following template, which matches the project handout's required fields:

   ```markdown
   ---

   ## Entry <N>

   | Field | Content |
   | :---- | :---- |
   | **Entry #** | <N> of <total> |
   | **Date** | YYYY-MM-DD |
   | **Phase** | Design / Lexer / Parser / Type checker / Interpreter / Testing / Writing |
   | **AI tool** | e.g., Claude Opus 4.6 / ChatGPT-4o / Copilot / Gemini (include version if known) |
   | **Goal** | One sentence — what you were trying to achieve. |

   ### Prompt

   <the exact prompt sent by the user, verbatim>

   ### Response (key part)

   <verbatim code/text excerpt, or a tight summary with direct quotes of the important lines>

   ### Accepted

   <what you kept from the AI response, and why>

   ### Rejected / Modified

   <what you threw away or changed, and why — be specific: bug, style issue, wrong abstraction, hallucinated API, etc.>

   ### Errors You Caught

   <any mistakes the AI made — how did you detect them? what would have happened if you hadn't?>

   ### Reflection

   <2–3 sentences: what did you learn about the problem or about how to use AI from this session?>
   ```

   - `<N>` is a sequential number starting from 1 for each day's file.
   - Each new entry is appended at the bottom of the file, separated by a horizontal rule (`---`).

5. **Minimum entry counts**:
   - **Part 1 (by 8 May):** Minimum 4 substantive entries. Must include **Experiment E1** (ask AI to generate EBNF grammar).
   - **Part 2 (by 22 May):** Minimum 6 additional entries. Must include **Experiments E2** (name vs. structural equivalence) and **E3** (AI-generated type checker).

6. **Grading criteria** (from handout):
   - **Honesty**: Fabricated entries = academic dishonesty.
   - **Depth of reflection**: "AI gave correct answer, I used it" = minimum credit. Discuss *why* it was correct, what could have gone wrong, how you verified.
   - **Critical evaluation**: At least 3 documented cases where the AI was wrong, unclear, or suboptimal.

7. **Ask before logging**: After every response, ask the user: **"Do you want to add this prompt and response to AI_Usage_Journal?"**. Only log it if the user confirms (e.g., "yes"). If the user declines (e.g., "no"), do **not** log it.

8. **User-written Errors & Reflection**: When the user confirms logging, do **not** write the **Errors You Caught** and **Reflection** sections yourself. Instead, follow this workflow:
   - First, show the user what each field means:
     > **Errors You Caught**: Any mistakes the AI made. How did you detect them? What would have happened if you hadn't?
     >
     > **Reflection (2–3 sentences)**: What did you learn about the problem — or about how to use AI — from this session?
   - Ask the user to provide their text for **Errors You Caught**.
   - Then ask the user to provide their text for **Reflection**.
   - Fix any grammar mistakes in the user's text before adding it to the journal entry.
   - Log the full entry with the user's (grammar-corrected) text in those two sections.

9. **Follow the textbook (Sebesta)**: All suggestions, design decisions, terminology, and explanations related to the project **must** be grounded in **Sebesta's "Concepts of Programming Languages" (12th ed.)**. The book chapters are available in the project:
   - **First**, check `src/book_chapters/MD-File-Version/` — these are concise markdown summaries of each chapter.
   - **If** the needed information is not found in the MD summaries, **then** look in `src/book_chapters/PDF-File-Version/` — these contain the original full textbook chapters.
   - Always cite the relevant **section number** (e.g., §5.5, §6.14) when referencing a concept from the book.
   - Do **not** invent terminology or design rationale that contradicts the book. If unsure, verify against the book files before responding.

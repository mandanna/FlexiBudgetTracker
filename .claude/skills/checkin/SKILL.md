---
name: checkin
description: Commit and push the current git repo's STAGED changes to GitHub, drafting a Conventional-Commits message the user confirms first. Use when the user wants to check in / commit / push their staged work. Triggers on "check in my changes", "checkin", "commit and push my staged changes", "push my staged work", "do my daily check-in".
---

# Check-in: commit + push staged changes

Commits and pushes the **staged** changes in whatever git repo the user is currently working in, one commit, with a message the user approves before anything happens. Human-in-the-loop on every commit and every push.

Scope: operate ONLY on the repo the current working directory belongs to (found via `git rev-parse --show-toplevel`). Never touch any other repo, even ones nearby on disk.

## Step 1 — Locate the repo

Run `git rev-parse --show-toplevel`. If it fails (not inside a git repo), tell the user and stop — do nothing else. Run every later git command against this repo root.

## Step 2 — Show current state

Report, briefly:
- current branch (`git rev-parse --abbrev-ref HEAD`)
- upstream if set (`git rev-parse --abbrev-ref --symbolic-full-name @{u}` — may be absent)
- counts: staged / modified-but-unstaged / untracked

If the branch is `main` or `master`, mention it (informational — the user may well want main here; it is not a blocker).

## Step 3 — Branch on whether anything is staged

Check `git diff --cached --name-only`.

### 3a — Nothing staged
List the modified and untracked files and **ask the user what to stage** for this commit. They can:
- name the files/paths they want, and you run `git add <those paths>`, or
- stage themselves and re-run the skill.

Do **NOT** stage everything on their behalf. Choosing what belongs in the commit is the user's call. After they've staged, continue to Step 3b. If they stage nothing, stop.

### 3b — Staged changes exist
1. Read the staged diff: `git diff --cached` (and `git diff --cached --stat` for a quick overview).
2. Draft ONE **Conventional-Commits** message that describes the staged change feature-wise or fix-wise:
   - `feat:` new capability, `fix:` bug fix, `refactor:`, `chore:`, `docs:`, `test:`, etc.
   - A concise subject line; add a short body only if the change needs explaining.
3. Show the drafted message to the user and let them edit it. **Commit only after they confirm.**
4. Commit with the co-author trailer this environment requires:

   ```
   <type>: <subject>

   <optional body>

   Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>
   ```

   Use a HEREDOC for the `-m` body so multi-line formatting survives.

Keep it to a single commit for the staged set. If the staged changes clearly mix unrelated concerns (e.g. a feature AND an unrelated fix), point that out and offer to split them — but only if the user wants that; the default is one commit.

## Step 4 — Push

- If an upstream is set: `git push`.
- If no upstream: `git push -u origin <current-branch>`.

Pushing sends the work to GitHub — an outward action. Show the user exactly what will be pushed (which branch → which remote, and how many commits ahead) and get a yes before running.

**Never** force-push. **Never** rewrite history (no amend/rebase/reset) as part of this skill.

## Step 5 — Report

Confirm the result: the new commit's short hash and subject, and that the push reached the remote (branch → remote URL).

## Hard rules

- Never commit or push without showing what will happen and getting explicit confirmation.
- Never `git add -A` / `git add .` for the user without them choosing the files.
- Never force-push, amend, rebase, or reset.
- Only ever act on the current repo — never loop over sibling repos.

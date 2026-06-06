# GitHub Upload Starter

Use this checklist for the first public upload.

## 1) Review Files

- Confirm `.gitignore` is correct
- Confirm `README.md` and policy files are present
- Confirm no private credentials are in tracked files

## 2) Optional: Create a New Repository on GitHub

Create an empty repository named `OpenMakaiRanch` under your account.

## 3) Initialize and Push (if not already initialized)

```powershell
git init
git add .
git commit -m "chore: initial project starter docs and scaffold"
git branch -M main
git remote add origin https://github.com/Noa3/OpenMakaiRanch.git
git push -u origin main
```

## 4) If Repository Already Exists Locally

```powershell
git add .
git commit -m "docs: add project target and GitHub starter files"
git push
```

## 5) Post-Upload Checks

- Verify README renders correctly on GitHub
- Verify issue templates appear in New Issue screen
- Verify PR template appears in new pull requests
- Verify `SECURITY.md` is visible in Security policy view

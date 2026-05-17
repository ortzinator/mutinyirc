# Contributing to Mutiny

## One-time local setup

Line endings are normalized to LF via `.gitattributes`. The repository was
renormalized in a single mechanical commit. To keep `git blame` readable on the
command line, configure Git to ignore that commit:

```sh
git config blame.ignoreRevsFile .git-blame-ignore-revs
```

This only affects local CLI `git blame`. GitHub, GitLab, and Gitea read
`.git-blame-ignore-revs` automatically, so no setup is needed for their web UIs.

## Building

```sh
dotnet build MutinyIRC.sln
dotnet test MutinyIRC.sln
```
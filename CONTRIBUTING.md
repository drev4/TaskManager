# Contributing

This is primarily a personal project, but it follows the same branching and commit conventions I'd use on a team, mostly as practice for keeping a clean history.

## Branches

- `main` — stable, deployable
- `develop` — integration branch, base for new work
- `feature/<name>`, `bugfix/<name>` — branch from `develop`, merge back into it
- `hotfix/<name>` — branch from `main`, merged into both `main` and `develop`

```bash
git checkout develop
git pull origin develop
git checkout -b feature/add-notifications
# ...work, commit...
git push -u origin feature/add-notifications
```

## Commit messages

[Conventional Commits](https://www.conventionalcommits.org/): `type(scope): subject`, e.g.

```
feat(auth): add Azure AD B2C login flow
fix(api): fix CORS policy for local development
refactor(tasks): move filtering logic into the service layer
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`.

## Before opening a PR

```bash
# backend
dotnet test
dotnet format

# frontend
npm run test
npm run lint
npm run type-check
```

Rebase onto `develop` before merging; squash-merge is preferred to keep `develop`'s history readable.

## Code style

- Backend: standard [C# conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions), async/await for I/O, XML doc comments on public APIs.
- Frontend: [Vue style guide](https://vuejs.org/style-guide/), Composition API, TypeScript everywhere.

## License

Contributions are made under the project's MIT License.

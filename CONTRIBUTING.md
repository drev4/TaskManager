# Contributing to TaskManager

## Branching Strategy

This project follows a simplified GitFlow branching model:

### Main Branches

- **`main`** - Production-ready code
  - Always stable and deployable
  - Protected branch (requires PR approval)
  - Deployed to production environment

- **`develop`** - Integration branch for development
  - Contains latest development changes
  - Base branch for new features
  - Deployed to staging/development environment

### Supporting Branches

#### Feature Branches
- **Naming**: `feature/feature-name`
- **Base branch**: `develop`
- **Merge back to**: `develop`
- **Purpose**: Develop new features

```bash
# Create a new feature branch
git checkout develop
git pull origin develop
git checkout -b feature/add-notifications

# Work on your feature...
git add .
git commit -m "Add notification system"

# Push to remote
git push -u origin feature/add-notifications

# Create Pull Request to develop
```

#### Bugfix Branches
- **Naming**: `bugfix/bug-description`
- **Base branch**: `develop`
- **Merge back to**: `develop`
- **Purpose**: Fix bugs found in development

```bash
# Create a bugfix branch
git checkout develop
git pull origin develop
git checkout -b bugfix/fix-login-error

# Fix the bug...
git add .
git commit -m "Fix login authentication error"

# Push and create PR
git push -u origin bugfix/fix-login-error
```

#### Hotfix Branches
- **Naming**: `hotfix/critical-fix`
- **Base branch**: `main`
- **Merge back to**: Both `main` AND `develop`
- **Purpose**: Emergency fixes for production

```bash
# Create a hotfix branch
git checkout main
git pull origin main
git checkout -b hotfix/critical-security-patch

# Apply the fix...
git add .
git commit -m "Security: Fix XSS vulnerability"

# Push and create PRs to both main and develop
git push -u origin hotfix/critical-security-patch
```

#### Release Branches
- **Naming**: `release/v1.0.0`
- **Base branch**: `develop`
- **Merge back to**: Both `main` AND `develop`
- **Purpose**: Prepare a new production release

```bash
# Create a release branch
git checkout develop
git pull origin develop
git checkout -b release/v1.0.0

# Finalize release (version bump, changelog, etc.)
git add .
git commit -m "Bump version to 1.0.0"

# Push and create PRs to main and develop
git push -u origin release/v1.0.0
```

## Commit Message Guidelines

Follow the Conventional Commits specification:

### Format
```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types
- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation changes
- **style**: Code style changes (formatting, missing semicolons, etc.)
- **refactor**: Code refactoring
- **perf**: Performance improvements
- **test**: Adding or updating tests
- **build**: Build system or dependency changes
- **ci**: CI/CD pipeline changes
- **chore**: Other changes that don't modify src or test files

### Examples
```bash
feat(auth): Add Azure AD B2C authentication

Implement user authentication using Azure AD B2C with MSAL.
- Add login/logout functionality
- Store tokens securely
- Redirect to callback page after login

Closes #123

---

fix(api): Fix CORS policy for localhost development

The API was blocking requests from localhost:3001 due to CORS policy.
Added the origin to AllowedOrigins in appsettings.json.

---

docs(readme): Update installation instructions

Add detailed steps for SQL Server LocalDB setup and seed data.

---

refactor(tasks): Extract task filtering logic to service

Move filtering logic from controller to service layer for better
separation of concerns and testability.
```

## Pull Request Process

1. **Create a branch** from the appropriate base branch
2. **Make your changes** following the code style guidelines
3. **Write tests** for new functionality
4. **Update documentation** if needed
5. **Commit your changes** with descriptive messages
6. **Push to your branch** and create a Pull Request
7. **Request review** from team members
8. **Address feedback** and make changes if requested
9. **Merge** once approved (squash and merge recommended)

### PR Title Format
```
<type>(<scope>): <description>
```

Example:
```
feat(dashboard): Add real-time task updates with SignalR
```

### PR Description Template
```markdown
## Description
Brief description of the changes

## Type of Change
- [ ] New feature
- [ ] Bug fix
- [ ] Breaking change
- [ ] Documentation update

## Related Issues
Closes #123

## Testing
How has this been tested?

## Screenshots (if applicable)
Add screenshots here

## Checklist
- [ ] Code follows project style guidelines
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No breaking changes
```

## Code Style Guidelines

### Backend (.NET)
- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use XML documentation comments for public APIs
- Keep methods small and focused
- Use async/await for I/O operations
- Follow SOLID principles

### Frontend (Vue/TypeScript)
- Follow [Vue.js Style Guide](https://vuejs.org/style-guide/)
- Use TypeScript for type safety
- Use Composition API over Options API
- Keep components small and reusable
- Use Tailwind utility classes

## Development Workflow

### Starting New Work
```bash
# 1. Update develop branch
git checkout develop
git pull origin develop

# 2. Create feature branch
git checkout -b feature/your-feature-name

# 3. Make changes and commit regularly
git add .
git commit -m "feat(scope): description"

# 4. Push to remote
git push -u origin feature/your-feature-name

# 5. Create Pull Request on GitHub
```

### Keeping Your Branch Updated
```bash
# Regularly sync with develop
git checkout develop
git pull origin develop
git checkout feature/your-feature-name
git merge develop

# Or use rebase for cleaner history
git rebase develop
```

### Before Merging
```bash
# 1. Ensure all tests pass
npm run test          # Frontend
dotnet test          # Backend

# 2. Check code quality
npm run lint         # Frontend
dotnet format        # Backend

# 3. Update from develop
git checkout develop
git pull origin develop
git checkout feature/your-feature-name
git rebase develop

# 4. Push and create PR
git push -f origin feature/your-feature-name
```

## Branch Protection Rules

### `main` branch
- Require pull request reviews (at least 1 approval)
- Require status checks to pass
- No direct pushes allowed
- Require branches to be up to date before merging

### `develop` branch
- Require pull request reviews (optional for minor changes)
- Require status checks to pass
- Allow force pushes for maintainers only

## Getting Help

- Check the [README.md](README.md) for project setup
- Review existing issues and PRs
- Ask questions in pull request comments
- Contact the maintainers

## License

By contributing, you agree that your contributions will be licensed under the project's MIT License.

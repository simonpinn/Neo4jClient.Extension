# GitHub Repository Setup Guide

This guide explains how to configure the GitHub repository for automated CI/CD and NuGet publishing.

## Repository Secrets

The following secrets need to be configured in your GitHub repository settings.

### Setting Up Secrets

1. Go to your repository on GitHub
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**

### Required Secrets

#### NUGET_API_KEY

The NuGet API key is required for publishing packages to NuGet.org.

**How to obtain a NuGet API key:**

1. Go to [NuGet.org](https://www.nuget.org/)
2. Sign in with your Microsoft account
3. Click on your username → **API Keys**
4. Click **Create** to generate a new API key
5. Configure the key:
   - **Key Name**: `Neo4jClient.Extension-GitHub-Actions` (or your preferred name)
   - **Package Owner**: Select your account
   - **Scopes**: Select **Push** and **Push new packages and package versions**
   - **Glob Pattern**: `Neo4jClient.Extension*` (to restrict to this package)
   - **Expiration**: Choose an appropriate expiration (recommended: 365 days)
6. Click **Create**
7. **IMPORTANT**: Copy the generated API key immediately (you won't be able to see it again)

**Adding the secret to GitHub:**

1. In your GitHub repository, go to **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Name: `NUGET_API_KEY`
4. Value: Paste the API key you copied from NuGet.org
5. Click **Add secret**

### Optional Secrets

#### CODECOV_TOKEN (Optional)

If you want to track code coverage with Codecov:

1. Go to [codecov.io](https://codecov.io/)
2. Sign in with GitHub
3. Add your repository
4. Copy the upload token
5. Add as a GitHub secret named `CODECOV_TOKEN`

## GitHub Actions Workflows

The repository includes two GitHub Actions workflows:

### 1. CI Workflow (`.github/workflows/ci.yml`)

**Triggers:**
- Push to `master`, `develop`, or `feature/**` branches
- Pull requests to `master` or `develop`

**What it does:**
- Builds the solution
- Runs unit tests
- Starts Neo4j container and runs integration tests
- Creates NuGet packages (on push only)
- Uploads artifacts

**No secrets required** - This workflow runs on all branches and PRs.

### 2. Release Workflow (`.github/workflows/release.yml`)

**Triggers:**
- Push of version tags (e.g., `v1.2.3`)

**What it does:**
- Builds and tests the solution
- Creates NuGet packages with version from tag
- Publishes to NuGet.org
- Creates GitHub release with release notes
- Uploads packages to GitHub release

**Required secret:** `NUGET_API_KEY`

## Creating a Release

To publish a new version to NuGet.org:

1. **Ensure all changes are merged to master**
   ```bash
   git checkout master
   git pull origin master
   ```

2. **Create and push a version tag**
   ```bash
   # GitVersion will automatically determine the version
   # But you can override by creating a specific tag:
   git tag v1.2.3
   git push origin v1.2.3
   ```

3. **GitHub Actions will automatically:**
   - Build and test the code
   - Create NuGet packages
   - Publish to NuGet.org (using `NUGET_API_KEY`)
   - Create a GitHub release
   - Attach packages to the release

4. **Monitor the release**
   - Go to **Actions** tab to watch the workflow
   - Check **Releases** tab for the published release
   - Verify on [NuGet.org](https://www.nuget.org/packages/Neo4jClient.Extension/)

## Branch Protection Rules (Recommended)

To ensure code quality, configure branch protection:

1. Go to **Settings** → **Branches**
2. Click **Add rule**
3. Branch name pattern: `master`
4. Enable:
   - ☑ Require a pull request before merging
   - ☑ Require status checks to pass before merging
     - Select: `build-and-test` (from CI workflow)
   - ☑ Require branches to be up to date before merging
   - ☑ Include administrators (optional but recommended)
5. Click **Create**

Repeat for `develop` branch.

## Environment Protection Rules (Optional)

For additional security on releases:

1. Go to **Settings** → **Environments**
2. Click **New environment**
3. Name: `production`
4. Configure:
   - **Required reviewers**: Add yourself or trusted maintainers
   - **Wait timer**: Optional delay before deployment
5. Update `release.yml` to use the environment:
   ```yaml
   jobs:
     release:
       environment: production
   ```

## Status Badge

Add this badge to your README to show build status:

```markdown
[![Build Status](https://img.shields.io/github/actions/workflow/status/simonpinn/Neo4jClient.Extension/ci.yml?branch=master)](https://github.com/simonpinn/Neo4jClient.Extension/actions)
```

## Troubleshooting

### Release workflow fails with "401 Unauthorized"

- Check that `NUGET_API_KEY` secret is set correctly
- Verify the API key hasn't expired on NuGet.org
- Ensure the API key has push permissions for the package

### Package push is "forbidden"

- Verify package ID ownership on NuGet.org
- Check that the API key glob pattern includes your package name
- Ensure you're the owner/co-owner of the package on NuGet.org

### GitVersion not working correctly

- Ensure repository is cloned with full history (`fetch-depth: 0`)
- Check `GitVersion.yml` configuration
- Verify branch names match the patterns in `GitVersion.yml`

### Integration tests fail in CI

- Check Neo4j service health in workflow logs
- Verify connection string environment variables
- Ensure sufficient wait time for Neo4j startup

## Next Steps

After setting up secrets:

1. ✅ Push changes to trigger CI workflow
2. ✅ Verify CI workflow passes
3. ✅ Create a test tag to verify release workflow (optional)
4. ✅ Monitor first release to NuGet.org
5. ✅ Configure branch protection rules

## Support

For issues with GitHub Actions or NuGet publishing, check:
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [NuGet Publishing Guide](https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [GitVersion Documentation](https://gitversion.net/docs/)

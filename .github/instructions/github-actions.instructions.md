# GitHub Actions CI/CD Best Practices for PhxInject

CI/CD pipeline standards for PhxInject using GitHub Actions. Covers build automation, testing workflows, release processes, and deployment patterns.

## Repository Setup

### Required Secrets

Configure in GitHub repository settings → Secrets and variables → Actions:

- `NUGET_API_KEY` - For publishing packages to NuGet.org
- `GITHUB_TOKEN` - Automatically provided by GitHub Actions

### Branch Protection

Enable for `main` branch:

- [ ] Require pull request reviews before merging
- [ ] Require status checks to pass before merging
- [ ] Require branches to be up to date before merging
- [ ] Require linear history
- [ ] Include administrators

## Workflow Structure

### Core Workflows

**1. Build and Test** (`build-test.yml`) - Runs on every push and PR:

```yaml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage"
    
    - name: Upload coverage
      uses: codecov/codecov-action@v3
      with:
        file: '**/coverage.cobertura.xml'
```

**2. Package and Publish** (`publish.yml`) - Runs on release tags:

```yaml
name: Publish NuGet

on:
  push:
    tags:
      - 'v*.*.*'

jobs:
  publish:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Pack
      run: dotnet pack --configuration Release --output ./artifacts
    
    - name: Publish to NuGet
      run: dotnet nuget push ./artifacts/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
```

**3. Code Quality** (`code-quality.yml`) - Runs on PR:

```yaml
name: Code Quality

on:
  pull_request:
    branches: [ main ]

jobs:
  lint:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Format check
      run: dotnet format --verify-no-changes --verbosity diagnostic
    
    - name: Analyzer warnings
      run: dotnet build --configuration Release -warnaserror
```

## Security Best Practices

### Use OIDC Instead of Long-Lived Secrets

For Azure deployments:

```yaml
permissions:
  id-token: write
  contents: read

- name: Azure Login
  uses: azure/login@v1
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

### Pin Action Versions by SHA

```yaml
# ✅ GOOD: Pin to commit SHA
- uses: actions/checkout@f43a0e5ff2bd294095638e18286ca9a3d1956744 # v4.1.1

# ❌ AVOID: Unpinned or tag-based
- uses: actions/checkout@v4
- uses: actions/checkout@main
```

### Least Privilege Permissions

```yaml
permissions:
  contents: read       # Read repository contents
  pull-requests: write # Comment on PRs
  # Don't grant unnecessary permissions
```

### Secure Secret Handling

```yaml
# ✅ GOOD: Use GitHub secrets
- name: Deploy
  env:
    API_KEY: ${{ secrets.API_KEY }}
  run: ./deploy.sh

# ❌ AVOID: Hardcoded secrets
- name: Deploy
  env:
    API_KEY: "sk_live_abc123..." # Never do this!
  run: ./deploy.sh
```

## Optimization Patterns

### Caching Dependencies

```yaml
- name: Cache NuGet packages
  uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json') }}
    restore-keys: |
      ${{ runner.os }}-nuget-

- name: Restore dependencies
  run: dotnet restore
```

### Matrix Strategies

Test across multiple configurations:

```yaml
strategy:
  matrix:
    os: [windows-latest, ubuntu-latest, macos-latest]
    dotnet-version: ['6.0.x', '7.0.x', '8.0.x']
    
runs-on: ${{ matrix.os }}

steps:
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: ${{ matrix.dotnet-version }}
```

### Conditional Execution

```yaml
# Only run on PR
- name: Comment coverage
  if: github.event_name == 'pull_request'
  uses: actions/github-script@v6
  with:
    script: |
      github.rest.issues.createComment({
        issue_number: context.issue.number,
        owner: context.repo.owner,
        repo: context.repo.repo,
        body: 'Coverage: 87%'
      })

# Only run on main branch
- name: Deploy
  if: github.ref == 'refs/heads/main'
  run: ./deploy.sh
```

### Parallel Jobs

```yaml
jobs:
  test-unit:
    runs-on: windows-latest
    steps: [...]
  
  test-integration:
    runs-on: windows-latest
    steps: [...]
  
  # Wait for both
  publish:
    needs: [test-unit, test-integration]
    runs-on: windows-latest
    steps: [...]
```

## Testing Strategies

### Unit Tests

```yaml
- name: Run unit tests
  run: dotnet test --filter "FullyQualifiedName~UnitTests" --configuration Release
```

### Integration Tests

```yaml
- name: Run integration tests
  run: dotnet test --filter "FullyQualifiedName~IntegrationTests" --configuration Release
  env:
    INTEGRATION_TEST_MODE: true
```

### Snapshot Tests

```yaml
- name: Run snapshot tests
  run: dotnet test --filter "Category=Snapshot" --configuration Release

- name: Check for snapshot changes
  run: |
    if git diff --exit-code '*.verified.*'; then
      echo "Snapshots match"
    else
      echo "Snapshots differ - review changes"
      exit 1
    fi
```

### Code Coverage

```yaml
- name: Test with coverage
  run: dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

- name: Generate coverage report
  uses: danielpalme/ReportGenerator-GitHub-Action@5.2.0
  with:
    reports: 'coverage/**/coverage.cobertura.xml'
    targetdir: 'coverage-report'
    reporttypes: 'HtmlInline;Cobertura'

- name: Upload coverage to Codecov
  uses: codecov/codecov-action@v3
  with:
    files: ./coverage-report/Cobertura.xml
    fail_ci_if_error: true

- name: Verify coverage threshold
  run: |
    $coverage = [xml](Get-Content ./coverage-report/Cobertura.xml)
    $lineRate = [double]$coverage.coverage.'line-rate'
    if ($lineRate -lt 0.85) {
      Write-Error "Coverage $($lineRate * 100)% below 85% threshold"
      exit 1
    }
```

## Deployment Patterns

### Semantic Versioning

Use Git tags for versions:

```bash
# Create release
git tag -a v1.2.3 -m "Release 1.2.3"
git push origin v1.2.3
```

Workflow extracts version from tag:

```yaml
- name: Get version
  id: version
  run: echo "VERSION=${GITHUB_REF#refs/tags/v}" >> $GITHUB_OUTPUT

- name: Pack with version
  run: dotnet pack -p:Version=${{ steps.version.outputs.VERSION }}
```

### Blue-Green Deployment

```yaml
- name: Deploy to staging
  run: ./deploy.sh --environment staging

- name: Run smoke tests
  run: ./smoke-tests.sh --url https://staging.example.com

- name: Swap to production
  if: success()
  run: ./swap.sh --from staging --to production

- name: Rollback on failure
  if: failure()
  run: ./rollback.sh
```

### Canary Deployment

```yaml
- name: Deploy 10% canary
  run: ./deploy.sh --canary 10

- name: Monitor metrics
  run: ./monitor.sh --duration 10m --threshold 0.01

- name: Roll out to 100%
  if: success()
  run: ./deploy.sh --canary 100

- name: Rollback canary
  if: failure()
  run: ./rollback.sh --canary
```

## Artifact Management

### Upload Build Artifacts

```yaml
- name: Build
  run: dotnet build --configuration Release

- name: Upload artifacts
  uses: actions/upload-artifact@v3
  with:
    name: packages
    path: |
      **/bin/Release/*.nupkg
      **/bin/Release/*.snupkg
    retention-days: 7
```

### Download Artifacts

```yaml
- name: Download artifacts
  uses: actions/download-artifact@v3
  with:
    name: packages
    path: ./artifacts
```

## Notification Patterns

### Slack Notifications

```yaml
- name: Notify Slack on failure
  if: failure()
  uses: slackapi/slack-github-action@v1
  with:
    webhook-url: ${{ secrets.SLACK_WEBHOOK }}
    payload: |
      {
        "text": "Build failed: ${{ github.repository }} - ${{ github.ref }}"
      }
```

### GitHub Issue Creation

```yaml
- name: Create issue on failure
  if: failure()
  uses: actions/github-script@v6
  with:
    script: |
      github.rest.issues.create({
        owner: context.repo.owner,
        repo: context.repo.repo,
        title: 'Build failed on ${{ github.ref }}',
        body: 'Workflow: ${{ github.workflow }}\nRun: ${{ github.run_id }}'
      })
```

## Troubleshooting

### Debug Logging

```yaml
- name: Enable debug
  run: echo "ACTIONS_STEP_DEBUG=true" >> $GITHUB_ENV

- name: Run with verbose
  run: dotnet build --verbosity diagnostic
```

### SSH into Runner

For complex debugging:

```yaml
- name: Setup tmate session
  uses: mxschmitt/action-tmate@v3
  if: failure()
```

### Artifact Inspection

```yaml
- name: List build output
  if: always()
  run: |
    Get-ChildItem -Recurse ./bin
    Get-ChildItem -Recurse ./obj
```

## PhxInject Specific Workflows

### Generator Testing Workflow

```yaml
name: Test Generator

on: [push, pull_request]

jobs:
  test-generator:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Test generator
      run: dotnet test Phx.Inject.Generator.Tests --configuration Release
    
    - name: Verify snapshot tests
      run: |
        if (git diff --exit-code '**/Snapshots/**/*.verified.cs') {
          Write-Output "✓ Snapshots match"
        } else {
          Write-Error "✗ Snapshots differ - review changes"
          exit 1
        }
    
    - name: Test performance
      run: dotnet test Phx.Inject.Generator.Tests --filter "Category=Performance"
```

### Documentation Build

```yaml
name: Build Documentation

on:
  push:
    branches: [ main ]
    paths:
      - 'Documentation/**'
      - 'src/**/*.cs'

jobs:
  build-docs:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup DocFX
      run: dotnet tool install -g docfx
    
    - name: Build docs
      run: docfx Documentation/docfx.json
    
    - name: Deploy to GitHub Pages
      uses: peaceiris/actions-gh-pages@v3
      with:
        github_token: ${{ secrets.GITHUB_TOKEN }}
        publish_dir: ./Documentation/_site
```

## Validation Checklist

Before merging workflow changes:

- [ ] **Workflows tested** - Verify on feature branch
- [ ] **Secrets configured** - All required secrets present
- [ ] **Permissions minimal** - Only necessary permissions granted
- [ ] **Actions pinned** - Use commit SHAs for stability
- [ ] **Cache configured** - Dependencies cached for speed
- [ ] **Tests comprehensive** - Unit, integration, snapshot
- [ ] **Coverage enforced** - 85% minimum threshold
- [ ] **Failures notify** - Team alerted on build failures
- [ ] **Artifacts uploaded** - Build outputs preserved
- [ ] **Documentation updated** - README reflects CI/CD process

## Questions?

- For testing patterns: See [Testing Standards](testing.instructions.md)
- For security guidelines: See [Security Standards](security.instructions.md)
- For performance optimization: See [Performance Standards](performance.instructions.md)

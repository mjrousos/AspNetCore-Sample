Param(
    [Parameter(Mandatory=$false, HelpMessage="Docker registry to push/pull images")]
    [string] $registry,

    [Parameter(Mandatory=$false, HelpMessage="User name for accessing Docker registry")]
    [string] $dockerUser,

    [Parameter(Mandatory=$false)][string] $imageTag,
    [Parameter(Mandatory=$false)][string] $appInsightsKey,
    [parameter(Mandatory=$false)][bool]$buildImages=$true,
    [parameter(Mandatory=$false)][string]$dockerOrg="aspnetcoresample"
)

if (-not $appInsightsKey) {
    Write-Warning "No Application Insights key provided. Telemetry will not be recorded."
}

if (-not $imageTag) {
    $imageTag = "latest"
    Write-Warning "No image tag provided. Using 'latest'"
}

# Login to Docker registry (and create registry-key secret) if needed
if (-not [string]::IsNullOrEmpty($dockerUser)) {
    Write-Host "Logging into $registry as $dockerUser" -ForegroundColor Cyan

    $dockerSecurePassword = Read-Host -Prompt "Enter Docker registry password" -AsSecureString
    $dockerPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($dockerSecurePassword))

    if (-not $LastExitCode -eq 0) {
        Write-Error "Login failed"
        exit
    }

    # Remove existing Docker secrets
    kubectl.exe get secret registry-key
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Deleting existing Docker registry secret from Kubernetes cluster" -ForegroundColor Cyan
        kubectl.exe delete secret registry-key
    }

    # Creat Docker secrets needed by our Kubernetes deployments
    # Note that creating Docker secrets was briefly broken in kubectl v1.9.0.
    # Make sure you've upgraded to >= 1.9.1 for this to work.
    Write-Host "Creating Docker registry secret in Kubernetes cluster"
    kubectl.exe create secret docker-registry registry-key `
        --docker-server=https://$registry `
        --docker-username=$dockerUser `
        --docker-password=$dockerPassword `
        --docker-email=not@used.com
}

# Build  docker images if needed
if ($buildImages) {
    Write-Host "Building Docker images tagged with '$imageTag'" -ForegroundColor Cyan
    $env:TAG=$imageTag
    docker-compose -p .. -f ../docker-compose.yml build

    Write-Host "Pushing images to $registry/$dockerOrg..." -ForegroundColor Cyan
    $services = ("customersapi", "customersmvc")

    foreach ($service in $services) {
        $imageFqdn = "$registry/$dockerOrg/$service"
        docker tag ${service}:$imageTag ${imageFqdn}:$imageTag
        docker push ${imageFqdn}:$imageTag
    }
}

# Remove existing config and secrets
kubectl.exe get secret aspnetcoredemo-secrets
if ($LASTEXITCODE -eq 0) {
    Write-Host "Deleting existing app secrets from Kubernetes cluster" -ForegroundColor Cyan
    kubectl.exe delete secret aspnetcoredemo-secrets
}

if (-not [string]::IsNullOrEmpty($appInsightsKey)) {
    Write-Host "Creating AppInsights key secret in Kubernetes cluster" -ForegroundColor Cyan

    kubectl.exe create secret generic aspnetcoredemo-secrets `
        --from-literal=AppInsightsKey=$appInsightsKey
}

# No config maps to set for this app.
# Config maps are configuration settings (like secrets) except that they're
# not kept secret. They're useful for application settings that need to be easily
# changed but that don't contain sensitive information.

# Create deployments/services
Write-Host "Creating deployments and services" -ForegroundColor Cyan

# Insert correct image names into deployments.yml
$registryPath = ""
if (-not [string]::IsNullOrEmpty($registry)) {
    $registryPath = "$registry/"
}
Get-Content $PSScriptRoot\deployments.yml `
    | ForEach-Object{$_ -replace "<CustomersMvcImage>", "$registryPath$dockerOrg/customersmvc:$imageTag"} `
    | ForEach-Object{$_ -replace "<CustomersApiImage>", "$registryPath$dockerOrg/customersapi:$imageTag"} `
    > $PSScriptRoot\deployments.processed.yml

# Apply (create or update) deployments and services
kubectl.exe apply -f $PSScriptRoot\deployments.processed.yml -f $PSScriptRoot\services.yml

# Clean up auto-generated file
Remove-Item $PSScriptRoot\deployments.processed.yml

Write-Host "Done" -ForegroundColor Cyan

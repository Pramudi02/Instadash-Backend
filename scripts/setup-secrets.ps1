# Azure Key Vault Secret Setup Script for InstaDash Backend
# This script helps you set up the required secrets in Azure Key Vault after deployment

param(
    [Parameter(Mandatory=$false)]
    [string]$KeyVaultName
)

Write-Host "InstaDash Backend - Azure Key Vault Setup" -ForegroundColor Green
Write-Host "==========================================="

# Check if Azure CLI is installed
try {
    az --version | Out-Null
} catch {
    Write-Host "Error: Azure CLI is not installed. Please install it first." -ForegroundColor Red
    exit 1
}

# Check if user is logged in
try {
    az account show | Out-Null
} catch {
    Write-Host "Please log in to Azure CLI first:" -ForegroundColor Yellow
    Write-Host "az login"
    exit 1
}

# Get Key Vault name from parameter, environment, or prompt
if (-not $KeyVaultName) {
    $KeyVaultName = $env:AZURE_KEY_VAULT_NAME
}

if (-not $KeyVaultName) {
    $KeyVaultName = Read-Host "Enter your Azure Key Vault name"
}

if (-not $KeyVaultName) {
    Write-Host "Error: Key Vault name is required" -ForegroundColor Red
    exit 1
}

Write-Host "Setting up secrets in Key Vault: $KeyVaultName" -ForegroundColor Green

# Function to set a secret
function Set-Secret {
    param(
        [string]$SecretName,
        [string]$SecretDescription,
        [bool]$IsSensitive = $true
    )
    
    Write-Host $SecretDescription -ForegroundColor Yellow
    
    if ($IsSensitive) {
        $SecretValue = Read-Host -AsSecureString
        $SecretValue = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecretValue))
    } else {
        $SecretValue = Read-Host
    }
    
    if ($SecretValue) {
        try {
            az keyvault secret set --vault-name $KeyVaultName --name $SecretName --value $SecretValue | Out-Null
            Write-Host "✓ Secret '$SecretName' set successfully" -ForegroundColor Green
        } catch {
            Write-Host "✗ Failed to set secret '$SecretName'" -ForegroundColor Red
        }
    } else {
        Write-Host "⚠ Skipped empty secret '$SecretName'" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Setting up required secrets..."
Write-Host "=============================="

# MongoDB Connection String
Set-Secret -SecretName "mongodb-connection-string" -SecretDescription "Enter your MongoDB connection string (e.g., mongodb+srv://username:password@cluster.mongodb.net/):" -IsSensitive $true

# JWT Key
Set-Secret -SecretName "jwt-key" -SecretDescription "Enter a secure JWT signing key (at least 32 characters):" -IsSensitive $true

# Email Password
Set-Secret -SecretName "email-password" -SecretDescription "Enter your email service password/app password:" -IsSensitive $true

Write-Host ""
Write-Host "Secret setup completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Update your application if needed"
Write-Host "2. Restart your Container App to pick up the new secrets"
Write-Host "3. Test your application endpoints"
Write-Host ""
Write-Host "To restart your Container App:"
Write-Host "az containerapp restart --name `$env:AZURE_CONTAINER_APP_NAME --resource-group `$env:AZURE_RESOURCE_GROUP_NAME"
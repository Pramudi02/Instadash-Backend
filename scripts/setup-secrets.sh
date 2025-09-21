#!/bin/bash

# Azure Key Vault Secret Setup Script for InstaDash Backend
# This script helps you set up the required secrets in Azure Key Vault after deployment

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}InstaDash Backend - Azure Key Vault Setup${NC}"
echo "=================================================="

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo -e "${RED}Error: Azure CLI is not installed. Please install it first.${NC}"
    exit 1
fi

# Check if user is logged in
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}Please log in to Azure CLI first:${NC}"
    echo "az login"
    exit 1
fi

# Get Key Vault name from environment or prompt
if [ -z "$AZURE_KEY_VAULT_NAME" ]; then
    echo -e "${YELLOW}Enter your Azure Key Vault name:${NC}"
    read -r AZURE_KEY_VAULT_NAME
fi

if [ -z "$AZURE_KEY_VAULT_NAME" ]; then
    echo -e "${RED}Error: Key Vault name is required${NC}"
    exit 1
fi

echo -e "${GREEN}Setting up secrets in Key Vault: $AZURE_KEY_VAULT_NAME${NC}"

# Function to set a secret
set_secret() {
    local secret_name="$1"
    local secret_description="$2"
    local is_sensitive="$3"
    
    echo -e "${YELLOW}$secret_description${NC}"
    if [ "$is_sensitive" = "true" ]; then
        read -s -r secret_value
        echo  # Add newline after hidden input
    else
        read -r secret_value
    fi
    
    if [ -n "$secret_value" ]; then
        if az keyvault secret set --vault-name "$AZURE_KEY_VAULT_NAME" --name "$secret_name" --value "$secret_value" > /dev/null; then
            echo -e "${GREEN}✓ Secret '$secret_name' set successfully${NC}"
        else
            echo -e "${RED}✗ Failed to set secret '$secret_name'${NC}"
        fi
    else
        echo -e "${YELLOW}⚠ Skipped empty secret '$secret_name'${NC}"
    fi
}

echo ""
echo "Setting up required secrets..."
echo "=============================="

# MongoDB Connection String
set_secret "mongodb-connection-string" "Enter your MongoDB connection string (e.g., mongodb+srv://username:password@cluster.mongodb.net/):" true

# JWT Key
set_secret "jwt-key" "Enter a secure JWT signing key (at least 32 characters):" true

# Email Password
set_secret "email-password" "Enter your email service password/app password:" true

echo ""
echo -e "${GREEN}Secret setup completed!${NC}"
echo ""
echo "Next steps:"
echo "1. Update your application if needed"
echo "2. Restart your Container App to pick up the new secrets"
echo "3. Test your application endpoints"
echo ""
echo "To restart your Container App:"
echo "az containerapp restart --name \$AZURE_CONTAINER_APP_NAME --resource-group \$AZURE_RESOURCE_GROUP_NAME"
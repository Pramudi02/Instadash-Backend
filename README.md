# InstaDash Backend - Azure Deployment

This repository contains the backend API for the InstaDash application, ready for deployment to Azure Container Apps using Azure Developer CLI (azd).

## 🏗️ Architecture

The application is deployed using the following Azure services:

- **Azure Container Apps**: Hosts the .NET 8 Web API
- **Azure Container Registry**: Stores the application container images
- **Azure Key Vault**: Securely stores secrets and configuration
- **Azure Log Analytics**: Centralized logging and monitoring
- **Azure Application Insights**: Application performance monitoring
- **Managed Identity**: Secure authentication between Azure services

## 📋 Prerequisites

Before deploying, ensure you have:

1. **Azure Subscription** with appropriate permissions
2. **Azure Developer CLI (azd)** installed
3. **Docker** installed and running
4. **MongoDB** service (MongoDB Atlas recommended)
5. **Email service** credentials (Gmail with app password recommended)

### Install Required Tools

```bash
# Install Azure Developer CLI
# Windows (using winget)
winget install microsoft.azd

# macOS (using Homebrew)
brew tap azure/azd && brew install azd

# Linux
curl -fsSL https://aka.ms/install-azd.sh | bash
```

## 🚀 Quick Deployment

### 1. Clone and Setup

```bash
git clone <your-repo-url>
cd Instadash-Backend
```

### 2. Initialize Azure Developer Environment

```bash
# Initialize a new azd environment
azd init

# Login to Azure
azd auth login
```

### 3. Deploy to Azure

```bash
# Deploy infrastructure and application
azd up
```

During deployment, you'll be prompted to:
- Select an Azure subscription
- Choose a region (e.g., East US 2)
- Provide an environment name (e.g., "instadash-prod")

### 4. Configure Secrets

After deployment, you need to set up the required secrets in Azure Key Vault:

**Option A: Using the provided script (Windows)**
```powershell
.\scripts\setup-secrets.ps1
```

**Option B: Using the provided script (Linux/macOS)**
```bash
chmod +x scripts/setup-secrets.sh
./scripts/setup-secrets.sh
```

**Option C: Manual setup using Azure CLI**
```bash
# Get your Key Vault name from azd output
KEYVAULT_NAME=$(azd env get-values | grep AZURE_KEY_VAULT_NAME | cut -d'=' -f2)

# Set MongoDB connection string
az keyvault secret set --vault-name $KEYVAULT_NAME --name "mongodb-connection-string" --value "your-mongodb-connection-string"

# Set JWT signing key (generate a secure 32+ character string)
az keyvault secret set --vault-name $KEYVAULT_NAME --name "jwt-key" --value "your-secure-jwt-key-32-chars-minimum"

# Set email password (app password for Gmail)
az keyvault secret set --vault-name $KEYVAULT_NAME --name "email-password" --value "your-email-app-password"
```

### 5. Restart the Application

After setting secrets, restart the container app to load the new configuration:

```bash
# Get your container app name
CONTAINER_APP_NAME=$(azd env get-values | grep AZURE_CONTAINER_APP_NAME | cut -d'=' -f2)
RESOURCE_GROUP_NAME=$(azd env get-values | grep AZURE_RESOURCE_GROUP_NAME | cut -d'=' -f2)

# Restart the container app
az containerapp restart --name $CONTAINER_APP_NAME --resource-group $RESOURCE_GROUP_NAME
```

## 🔧 Configuration

### Required Secrets

The application requires the following secrets to be configured in Azure Key Vault:

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `mongodb-connection-string` | MongoDB connection string | `mongodb+srv://user:pass@cluster.mongodb.net/` |
| `jwt-key` | JWT signing key (32+ chars) | `your-very-secure-jwt-secret-key-32-chars` |
| `email-password` | Email service password | `your-gmail-app-password` |

### Environment Variables

The application uses the following environment variables (automatically configured):

- `ASPNETCORE_ENVIRONMENT`: Set to "Production"
- `MongoDBSettings__*`: MongoDB configuration
- `JwtSettings__*`: JWT authentication configuration
- `EmailSettings__*`: Email service configuration

## 📊 Monitoring and Logging

- **Application Insights**: Monitor application performance and errors
- **Log Analytics**: Centralized logging for all application logs
- **Azure Monitor**: Set up alerts and dashboards

Access these through the Azure Portal using the resource group created by azd.

## 🔒 Security Features

- **Managed Identity**: No stored credentials for Azure service access
- **Key Vault**: Secure secret storage with RBAC
- **HTTPS Only**: All communication encrypted in transit
- **CORS Configuration**: Properly configured cross-origin requests
- **Private Container Registry**: Images stored securely in ACR

## 🛠️ Development Commands

```bash
# View application logs
azd logs

# Get deployment status
azd show

# Redeploy after code changes
azd deploy

# Tear down all resources
azd down
```

## 🌐 API Endpoints

Once deployed, your API will be available at the URL provided in the azd output. Key endpoints include:

- `GET /swagger` - API documentation
- `POST /api/auth/login` - User authentication
- `GET /api/orders` - Orders management
- `GET /api/inventory` - Inventory management
- `POST /api/chat` - Real-time chat functionality

## 🐛 Troubleshooting

### Common Issues

1. **Container fails to start**
   - Check that all secrets are properly configured in Key Vault
   - Verify MongoDB connection string is correct
   - Check application logs: `azd logs`

2. **Database connection errors**
   - Ensure MongoDB service is running and accessible
   - Verify connection string format and credentials
   - Check network connectivity from Azure to your MongoDB service

3. **Authentication errors**
   - Verify JWT key is set and at least 32 characters
   - Check JWT configuration in appsettings

4. **Email service errors**
   - Verify SMTP credentials and settings
   - Ensure Gmail app password is correctly configured

### Get Help

- Check application logs: `azd logs`
- View container app logs in Azure Portal
- Check Application Insights for errors and performance issues

## 📚 Additional Resources

- [Azure Developer CLI Documentation](https://docs.microsoft.com/azure/developer/azure-developer-cli/)
- [Azure Container Apps Documentation](https://docs.microsoft.com/azure/container-apps/)
- [MongoDB Atlas Documentation](https://docs.atlas.mongodb.com/)
- [ASP.NET Core Deployment](https://docs.microsoft.com/aspnet/core/host-and-deploy/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test locally using Docker
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
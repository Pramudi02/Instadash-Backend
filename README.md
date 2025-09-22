# InstaDash Backend - ASP.NET Core 8.0 Web API

This repository contains the backend API for the InstaDash application, built with ASP.NET Core 8.0 and deployed to Azure Web App Service using Azure Developer CLI (azd).

## 🏗️ Architecture

The application is deployed using the following Azure services:

- **Azure Web App Service**: Hosts the ASP.NET Core 8.0 Web API
- **Azure Key Vault**: Securely stores secrets and configuration
- **Azure Application Insights**: Application performance monitoring and diagnostics
- **Managed Identity**: Secure authentication between Azure services

## �️ Technology Stack

- **Framework**: ASP.NET Core 8.0 Web API
- **Language**: C# 12.0
- **Database**: MongoDB Atlas
- **Authentication**: JWT Bearer Tokens
- **Real-time Communication**: SignalR
- **Background Jobs**: Hangfire with MongoDB storage
- **Documentation**: Swagger/OpenAPI
- **Email Service**: SMTP (Gmail)
- **PDF Generation**: QuestPDF
- **Security**: BCrypt for password hashing

## �📋 Prerequisites

Before deploying, ensure you have:

1. **Azure Subscription** with appropriate permissions (Azure for Students recommended)
2. **Azure Developer CLI (azd)** installed
3. **MongoDB Atlas** account and cluster
4. **Email service** credentials (Gmail with app password recommended)
5. **Git** for version control

### Install Required Tools

```bash
# Install Azure Developer CLI
# Windows (using winget)
winget install microsoft.azd

# macOS (using Homebrew)
brew tap azure/azd && brew install azd

# Linux
curl -fsSL https://aka.ms/install-azd.sh | bash

# Verify installation
azd version
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
- Select an Azure subscription (choose "Azure for Students" if available)
- Choose a region (Central India recommended for better performance)
- Provide an environment name (e.g., "instadash-prod")

### 4. Configure Secrets

After deployment, you need to set up the required secrets in Azure Key Vault:

**Option A: Manual setup using Azure CLI**
```bash
# Get your Key Vault name from azd output
azd env get-values

# Set MongoDB connection string (use your actual MongoDB Atlas connection string)
az keyvault secret set --vault-name "kvue5te3lwwi77g" --name "mongodb-connection-string" --value "mongodb+srv://your-username:your-password@cluster.mongodb.net/?retryWrites=true&w=majority"

# Set JWT signing key (generate a secure 32+ character string)
az keyvault secret set --vault-name "kvue5te3lwwi77g" --name "jwt-key" --value "your-very-secure-jwt-secret-key-at-least-32-characters-long"

# Set email password (Gmail app password)
az keyvault secret set --vault-name "kvue5te3lwwi77g" --name "email-password" --value "your-gmail-app-password"
```

**Option B: Using PowerShell script (if available)**
```powershell
# If setup script exists
.\scripts\setup-secrets.ps1
```

### 5. Restart the Application

After setting secrets, restart the web app to load the new configuration:

```bash
# Restart the web app
az webapp restart --name "appue5te3lwwi77g" --resource-group "instadash-backend"
```

## 🔧 Configuration

### Required Secrets in Azure Key Vault

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `mongodb-connection-string` | MongoDB Atlas connection string | `mongodb+srv://user:pass@cluster.mongodb.net/` |
| `jwt-key` | JWT signing key (32+ chars) | `your-very-secure-jwt-secret-key-32-chars` |
| `email-password` | Gmail app password | `abcd-efgh-ijkl-mnop` |

### Environment Variables (Auto-configured)

- `ASPNETCORE_ENVIRONMENT`: Production
- `MongoDBSettings__ConnectionString`: @Microsoft.KeyVault(...)
- `MongoDBSettings__DatabaseName`: ab-uom
- `JwtSettings__Key`: @Microsoft.KeyVault(...)
- `JwtSettings__Issuer`: sdp-auth-server
- `EmailSettings__SmtpServer`: smtp.gmail.com
- `EmailSettings__SenderEmail`: your-email@gmail.com

## 📊 Monitoring and Logging

- **Application Insights**: Real-time performance monitoring and error tracking
- **Azure Monitor**: Built-in metrics and alerts
- **Diagnostic Logs**: Application logs available in Azure Portal

Access monitoring through the Azure Portal using the `instadash-backend` resource group.

## 🔒 Security Features

- **Managed Identity**: Secure access to Azure Key Vault
- **Azure Key Vault**: Encrypted secret storage
- **HTTPS Only**: All communication encrypted
- **JWT Authentication**: Secure API access
- **CORS Configuration**: Properly configured for frontend integration
- **BCrypt Password Hashing**: Secure password storage

## 🛠️ Development Commands

```bash
# View deployment status
azd show

# Get environment values
azd env get-values

# Redeploy application after code changes
azd deploy

# View application logs
az webapp log tail --name "appue5te3lwwi77g" --resource-group "instadash-backend"

# Restart the web app
az webapp restart --name "appue5te3lwwi77g" --resource-group "instadash-backend"

# Tear down all resources
azd down
```

## 🌐 API Endpoints

Once deployed, your API will be available at: `https://appue5te3lwwi77g.azurewebsites.net`

**Key endpoints include:**

- `GET /test-env` - Environment diagnostics
- `GET /test-database-connection` - MongoDB connectivity test
- `GET /api/Sales` - Sales data management
- `GET /api/Inventory` - Inventory management
- `GET /api/Orders` - Order processing
- `POST /api/Auth/login` - User authentication
- `POST /api/Chat` - Real-time chat functionality
- `GET /api/Customers` - Customer management
- `GET /api/Dashboard` - Analytics and reporting

## 🐛 Troubleshooting

### Common Issues

1. **Application fails to start (500.30 error)**
   - Check that all secrets are properly configured in Key Vault
   - Verify managed identity has access to Key Vault
   - Check Application Insights for detailed error messages

2. **Database connection errors**
   - Ensure MongoDB Atlas cluster is running and accessible
   - Verify connection string format and credentials
   - Check network connectivity from Azure to MongoDB Atlas

3. **Authentication errors**
   - Verify JWT key is set and at least 32 characters long
   - Check JWT configuration in environment variables

4. **Email service errors**
   - Verify Gmail app password is correctly configured
   - Ensure sender email matches the Gmail account

### Diagnostic Steps

```bash
# Check application settings
az webapp config appsettings list --name "appue5te3lwwi77g" --resource-group "instadash-backend"

# View recent logs
az webapp log tail --name "appue5te3lwwi77g" --resource-group "instadash-backend"

# Test API endpoints
curl https://appue5te3lwwi77g.azurewebsites.net/test-env
curl https://appue5te3lwwi77g.azurewebsites.net/test-database-connection
```

### Get Help

- Check Application Insights in Azure Portal for detailed error tracking
- View web app logs using `az webapp log tail`
- Test diagnostic endpoints for quick health checks
- Review the `PRODUCTION-GUIDE.md` and `MONITORING.md` files for detailed troubleshooting

## 📚 Additional Resources

- [Azure Developer CLI Documentation](https://docs.microsoft.com/azure/developer/azure-developer-cli/)
- [Azure Web App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [ASP.NET Core 8.0 Documentation](https://docs.microsoft.com/aspnet/core/)
- [MongoDB Atlas Documentation](https://docs.atlas.mongodb.com/)
- [Azure Key Vault Documentation](https://docs.microsoft.com/azure/key-vault/)
- [Application Insights Documentation](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview)

## 📖 Project Documentation

- `PRODUCTION-GUIDE.md` - Complete production deployment guide
- `MONITORING.md` - Monitoring setup and troubleshooting
- `.github/workflows/deploy.yml` - CI/CD pipeline configuration

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch from `deploy`
3. Make your changes
4. Test locally using `dotnet run`
5. Ensure all tests pass with `dotnet test`
6. Submit a pull request to the `deploy` branch

### Local Development Setup

```bash
# Clone the repository
git clone <your-repo-url>
cd Instadash-Backend

# Restore dependencies
dotnet restore

# Run the application locally
dotnet run

# Run tests
dotnet test

# Build for production
dotnet build --configuration Release
```

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
# 🚀 InstaDash Backend - Production Deployment Guide

## ✅ Current Status
- **✅ Application Deployed**: Azure Web App Service running successfully
- **✅ Database Connected**: MongoDB Atlas integration working
- **✅ Security Configured**: JWT authentication and Key Vault secrets
- **✅ Monitoring Enabled**: Application Insights configured
- **✅ CI/CD Ready**: GitHub Actions workflow created

## 🌐 Production URLs
- **Backend API**: https://appue5te3lwwi77g.azurewebsites.net
- **Health Check**: https://appue5te3lwwi77g.azurewebsites.net/test-env
- **Database Test**: https://appue5te3lwwi77g.azurewebsites.net/test-database-connection

## 🔧 Azure Resources
- **Resource Group**: `instadash-backend`
- **Web App**: `appue5te3lwwi77g`
- **Key Vault**: `kvue5te3lwwi77g`
- **Application Insights**: `aiue5te3lwwi77g`
- **Subscription**: Azure for Students (84fd47c7-6b32-4376-b8ed-c323a455703a)

## 🔐 Security Configuration

### Secrets in Azure Key Vault
- **mongodb-connection-string**: MongoDB Atlas connection
- **jwt-key**: JWT signing key for authentication
- **email-password**: Gmail app password for notifications

### Authentication
- **JWT Bearer Authentication**: Configured and working
- **Managed Identity**: System-assigned identity for Key Vault access
- **CORS Policy**: Configured for localhost:4200 (Angular frontend)

## 📦 CI/CD Pipeline

### GitHub Actions Workflow
Location: `.github/workflows/deploy.yml`

**Triggers**:
- Push to `deploy` or `main` branch
- Pull request to `deploy` branch
- Manual trigger via workflow_dispatch

**Steps**:
1. Checkout code
2. Setup .NET 8
3. Restore dependencies
4. Build application
5. Run tests
6. Publish application
7. Deploy to Azure Web App

### Required Secrets
Add to GitHub repository secrets:
- `AZURE_WEBAPP_PUBLISH_PROFILE`: Download from Azure portal

## 🛠️ Custom Domain Setup (Optional)

### Steps to configure custom domain:
1. **Purchase domain** from a registrar
2. **Add custom domain** in Azure Web App settings
3. **Configure DNS** records:
   ```
   Type: CNAME
   Name: api (or your subdomain)
   Value: appue5te3lwwi77g.azurewebsites.net
   ```
4. **Enable SSL certificate** (Azure provides free SSL)

### Example configuration:
- Custom Domain: `api.yourdomain.com`
- SSL Certificate: Auto-managed by Azure
- Redirect HTTP to HTTPS: Enabled

## 📊 Monitoring & Alerts

### Application Insights Configured
- **Automatic telemetry**: Requests, dependencies, exceptions
- **Custom telemetry**: Available via ILogger
- **Query language**: KQL for advanced analytics

### Recommended Alerts
- Availability < 95%
- Response time > 5 seconds
- Exception count > 10 per 15 minutes
- CPU usage > 80%
- Memory usage > 80%

## 🔄 Scaling Configuration

### Current Setup
- **Tier**: Basic B1 (1 vCPU, 1.75 GB RAM)
- **Auto-scaling**: Not configured (manual scaling available)

### Scaling Options
1. **Scale Up**: Increase to Standard or Premium tier
2. **Scale Out**: Add multiple instances
3. **Auto-scaling rules**: Based on CPU, memory, or custom metrics

## 🐛 Troubleshooting

### Common Issues
1. **500.30 errors**: Check Key Vault access and managed identity
2. **Database connection**: Verify MongoDB connection string in Key Vault
3. **JWT errors**: Ensure JWT key is properly configured
4. **CORS issues**: Update CORS policy for frontend domain

### Diagnostic Commands
```bash
# Check app settings
az webapp config appsettings list --name appue5te3lwwi77g --resource-group instadash-backend

# View logs
az webapp log tail --name appue5te3lwwi77g --resource-group instadash-backend

# Restart application
az webapp restart --name appue5te3lwwi77g --resource-group instadash-backend
```

## 🔧 Environment-Specific Configuration

### Production Environment Variables
```
ASPNETCORE_ENVIRONMENT=Production
MongoDBSettings__ConnectionString=@Microsoft.KeyVault(...)
JwtSettings__Key=@Microsoft.KeyVault(...)
EmailSettings__Password=@Microsoft.KeyVault(...)
```

### MongoDB Configuration
- **Database**: ab-uom
- **Connection**: MongoDB Atlas cluster
- **Security**: Username/password authentication
- **Collections**: sales, customers, inventory, orders, etc.

## 📈 Performance Optimization

### Implemented
- **JSON serialization**: System.Text.Json
- **Connection pooling**: MongoDB driver default
- **JWT caching**: Built-in .NET JWT middleware
- **Static file serving**: wwwroot enabled

### Recommendations
- **Response caching**: For read-heavy endpoints
- **Database indexing**: Optimize MongoDB queries
- **CDN**: For static assets
- **Application Gateway**: For advanced routing

## 🔒 Security Hardening

### Current Security
- ✅ HTTPS only enforced
- ✅ JWT authentication implemented
- ✅ Secrets in Key Vault
- ✅ Managed identity for service-to-service auth

### Additional Recommendations
- **API rate limiting**: Prevent abuse
- **Input validation**: Comprehensive validation
- **Security headers**: HSTS, CSP, etc.
- **Dependency scanning**: Regular security updates

## 🚀 Next Steps

1. **Configure custom domain** and SSL certificates
2. **Set up monitoring alerts** for production health
3. **Implement auto-scaling** based on demand
4. **Create staging environment** for testing
5. **Set up backup strategy** for Key Vault and configurations
6. **Document API endpoints** for frontend integration
7. **Performance testing** under load
8. **Security audit** and penetration testing

## 📞 Support & Maintenance

### Regular Tasks
- Monitor Application Insights dashboards
- Review and rotate secrets quarterly
- Update dependencies monthly
- Performance testing before major releases
- Backup verification weekly

### Emergency Contacts
- Azure Support: Via Azure portal
- MongoDB Atlas Support: Via Atlas dashboard
- Critical Issue Response: Check Application Insights first

---

**🎉 Congratulations! Your InstaDash Backend is production-ready and successfully deployed to Azure!**
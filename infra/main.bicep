targetScope = 'resourceGroup'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string = ''

// Generate a unique token for resource naming
var resourceToken = uniqueString(subscription().id, resourceGroup().id, location, environmentName)

// Define resource name prefixes
var abbrs = {
  keyVaultVaults: 'kv'
  logAnalyticsWorkspaces: 'log'
  managedIdentityUserAssignedIdentities: 'id'
  insightsComponents: 'ai'
  webSites: 'app'
  appServicePlans: 'asp'
}

// Tags that should be applied to all resources
var tags = {
  'azd-env-name': environmentName
}

// User-assigned managed identity
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${abbrs.managedIdentityUserAssignedIdentities}${resourceToken}'
  location: location
  tags: tags
}

// Log Analytics workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${abbrs.logAnalyticsWorkspaces}${resourceToken}'
  location: location
  tags: tags
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

// Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${abbrs.insightsComponents}${resourceToken}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// Key Vault for storing secrets
resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: '${abbrs.keyVaultVaults}${resourceToken}'
  location: location
  tags: tags
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: managedIdentity.properties.principalId
        permissions: {
          secrets: ['get', 'list']
        }
      }
    ]
    enableRbacAuthorization: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
}

// Add access policy for the user if principalId is provided
resource userAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2022-07-01' = if (!empty(principalId)) {
  name: 'add'
  parent: keyVault
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: principalId
        permissions: {
          secrets: ['get', 'list', 'set', 'delete']
        }
      }
    ]
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: '${abbrs.appServicePlans}${resourceToken}'
  location: location
  tags: tags
  sku: {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    family: 'B'
    capacity: 1
  }
  properties: {
    reserved: false
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: '${abbrs.webSites}${resourceToken}'
  location: location
  tags: union(tags, { 'azd-service-name': 'backend' })
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'MongoDBSettings__ConnectionString'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=mongodb-connection-string)'
        }
        {
          name: 'MongoDBSettings__DatabaseName'
          value: 'ab-uom'
        }
        {
          name: 'MongoDBSettings__AdminDatabaseName'
          value: 'admin-db'
        }
        {
          name: 'MongoDBSettings__UserDetailsdatabase'
          value: 'UserDetails'
        }
        {
          name: 'MongoDBSettings__UserDetailsCollectionName'
          value: 'userdetailscollection'
        }
        {
          name: 'MongoDBSettings__PasswordResetCollectionName'
          value: 'passwordresets'
        }
        {
          name: 'MongoDBSettings__UsersCollectionName'
          value: 'instadash'
        }
        {
          name: 'MongoDBSettings__HoneycombCollectionName'
          value: 'honeycomb-db'
        }
        {
          name: 'JwtSettings__Key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=jwt-key)'
        }
        {
          name: 'JwtSettings__Issuer'
          value: 'sdp-auth-server'
        }
        {
          name: 'JwtSettings__Audience'
          value: 'sdp-app-users'
        }
        {
          name: 'JwtSettings__ExpiresInMinutes'
          value: '60'
        }
        {
          name: 'EmailSettings__SmtpServer'
          value: 'smtp.gmail.com'
        }
        {
          name: 'EmailSettings__SmtpPort'
          value: '587'
        }
        {
          name: 'EmailSettings__SenderEmail'
          value: 'allybees23@gmail.com'
        }
        {
          name: 'EmailSettings__SenderName'
          value: 'Allybees Support'
        }
        {
          name: 'EmailSettings__Password'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=email-password)'
        }
        {
          name: 'EmailSettings__UseSsl'
          value: 'true'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
      ]
      cors: {
        allowedOrigins: [
          'http://localhost:4200'
          'https://localhost:4200'
        ]
        supportCredentials: true
      }
    }
  }
}

// Outputs required by AZD
output RESOURCE_GROUP_ID string = resourceGroup().id
output AZURE_KEY_VAULT_NAME string = keyVault.name
output AZURE_KEY_VAULT_ENDPOINT string = keyVault.properties.vaultUri
output BACKEND_URL string = 'https://${webApp.properties.defaultHostName}'
output AZURE_WEB_APP_NAME string = webApp.name

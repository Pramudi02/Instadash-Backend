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
  containerRegistries: 'cr'
  containerApps: 'ca'
  containerAppsEnvironments: 'cae'
  managedIdentityUserAssignedIdentities: 'id'
  insightsComponents: 'ai'
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

// Container Registry
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: '${abbrs.containerRegistries}${resourceToken}'
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
}

// Role assignment for managed identity to access container registry
resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, managedIdentity.id, '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Container Apps Environment
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: '${abbrs.containerAppsEnvironments}${resourceToken}'
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// Container App for the backend API
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: '${abbrs.containerApps}${resourceToken}'
  location: location
  tags: union(tags, { 'azd-service-name': 'backend' })
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 10000
        corsPolicy: {
          allowedOrigins: ['http://localhost:4200', 'https://localhost:4200']
          allowedMethods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS']
          allowedHeaders: ['*']
          allowCredentials: true
        }
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: managedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'mongodb-connection-string'
          keyVaultUrl: '${keyVault.properties.vaultUri}secrets/mongodb-connection-string'
          identity: managedIdentity.id
        }
        {
          name: 'jwt-key'
          keyVaultUrl: '${keyVault.properties.vaultUri}secrets/jwt-key'
          identity: managedIdentity.id
        }
        {
          name: 'email-password'
          keyVaultUrl: '${keyVault.properties.vaultUri}secrets/email-password'
          identity: managedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
          name: 'backend'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:10000'
            }
            {
              name: 'MongoDBSettings__ConnectionString'
              secretRef: 'mongodb-connection-string'
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
              secretRef: 'jwt-key'
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
              value: 'your-email@gmail.com'
            }
            {
              name: 'EmailSettings__SenderName'
              value: 'InstaDash App'
            }
            {
              name: 'EmailSettings__Password'
              secretRef: 'email-password'
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
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 10
      }
    }
  }
  dependsOn: [
    acrPullRoleAssignment
  ]
}

// Outputs required by AZD
output RESOURCE_GROUP_ID string = resourceGroup().id
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.properties.loginServer
output AZURE_CONTAINER_REGISTRY_NAME string = containerRegistry.name
output AZURE_KEY_VAULT_NAME string = keyVault.name
output AZURE_KEY_VAULT_ENDPOINT string = keyVault.properties.vaultUri
output BACKEND_URL string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output AZURE_CONTAINER_APP_NAME string = containerApp.name
output AZURE_CONTAINER_APP_ENVIRONMENT_NAME string = containerAppsEnvironment.name
param projectName string = 'wgcci-accounting'
param location string = resourceGroup().location
param sku string = 'B1'
param sqlAdmin string = 'sqladmin'
@secure()
param sqlPassword string
var sqlServerName='${projectName}-sql'
var dbName='${projectName}-db'
var planName='${projectName}-plan'
var webName='${projectName}-api'
resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {name: planName location: location sku: {name: sku tier:'Basic' size:'B1' capacity:1}}
resource web 'Microsoft.Web/sites@2023-12-01' = {name: webName location: location properties:{serverFarmId: plan.id httpsOnly: true} identity:{type:'SystemAssigned'}}
resource sql 'Microsoft.Sql/servers@2022-05-01-preview' = {name: sqlServerName location: location properties:{administratorLogin: sqlAdmin administratorLoginPassword: sqlPassword publicNetworkAccess:'Enabled'}}
resource db 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {name: '${sql.name}/${dbName}' location: location sku:{name:'Basic'}}
resource conn 'Microsoft.Web/sites/config@2023-12-01' = {name: '${web.name}/connectionstrings' properties:{DefaultConnection:{value:'Server=tcp:${sql.name}.database.windows.net,1433;Initial Catalog=${dbName};User ID=${sqlAdmin};Password=${sqlPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;' type:'SQLAzure'}} dependsOn:[db,web]}
output webUrl string = 'https://${web.name}.azurewebsites.net'

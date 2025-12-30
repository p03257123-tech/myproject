# WGCCI Accounting — .NET 8 + Azure App Service

- Roles: Admin, Bursar, Accountant, Auditor
- Budgets & Forecasts, FX rates + revaluation stub, Tax codes, Bank import/rules, Audit log
- Swagger at `/swagger`

## Local
```bash
dotnet run --project WGCCI.Accounting.Api/WGCCI.Accounting.Api.csproj
```

## Azure deploy
1. Create resource group + deploy infra:
```bash
az group create -n rg-wgcci -l eastus
az deployment group create -g rg-wgcci -f infra/main.bicep -p sqlPassword='YourStrong!Passw0rd'
```
2. Create a Web App publish profile and store it in GitHub secret `AZURE_WEBAPP_PUBLISH_PROFILE`.
3. Push to `main` to trigger `.github/workflows/azure-deploy.yml`.

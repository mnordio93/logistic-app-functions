# Development Guide — Functions

## Requisiti

- **.NET SDK 10** — `~/.dotnet/dotnet` (stesso del backend)
- **Azure Functions Core Tools v4** — `npm install -g azure-functions-core-tools@4 --unsafe-perm true`
- **Azurite** (opzionale, per `AzureWebJobsStorage` locale) — `npm install -g azurite`
- Un'istanza **SQL Server** accessibile in locale (o Azure SQL via VPN/tunnel)
- **Playwright browser binaries** — installate una volta dopo il primo build (vedi sotto)

## Setup locale

### 1. Configurare `local.settings.json`

Copiare il template e impostare la connection string e le credenziali Vecon:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "SqlConnectionString": "Server=localhost;Database=logisticapp;User Id=sa;Password=<password>;TrustServerCertificate=True",
    "Vecon__Username": "test@test.it",
    "Vecon__Password": "<password>"
  }
}
```

> `local.settings.json` è in `.gitignore` — non viene mai committato.

### 2. Avviare Azurite (se non si usa un Azure Storage reale)

```bash
azurite --silent --location /tmp/azurite --debug /tmp/azurite/debug.log
```

### 2b. Installare i browser Playwright (prima volta)

Dopo aver fatto il primo `dotnet build`, installare i binari di Chromium:

```bash
export PATH="$HOME/.dotnet:$PATH"
cd functions/LogisticApp.Functions
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

Se `pwsh` non è disponibile, usare `dotnet tool install -g Microsoft.Playwright.CLI` e poi `playwright install chromium`.

### 3. Avviare le function

```bash
export PATH="$HOME/.dotnet:$PATH"
cd functions/LogisticApp.Functions
func start
```

Le function sono accessibili su `http://localhost:7071`.

### Eseguire una function manualmente (HTTP trigger o admin)

```bash
# Invocare il timer trigger via admin endpoint
curl -X POST http://localhost:7071/admin/functions/ProcessDeliveriesFunction \
  -H "Content-Type: application/json" \
  -d '{}'
```

## Build e test

```bash
export PATH="$HOME/.dotnet:$PATH"
cd functions/LogisticApp.Functions

dotnet restore
dotnet build
dotnet publish -c Release -o ./publish
```

## Aggiungere una nuova function

1. Creare `Functions/[NomeFunzione].cs` seguendo il pattern di `ProcessDeliveriesFunction`
2. Usare primary constructor per iniettare `FunctionsDbContext` e `ILogger<T>`
3. Registrare servizi aggiuntivi in `Program.cs` se necessario
4. Aggiornare `docs/ARCHITECTURE.md`

## Deploy su Azure

Il deploy avviene automaticamente via GitHub Actions al push su `main`.
Vedi `.github/workflows/deploy-functions.yml`.

### Secrets e variabili richiesti nel repository GitHub

| Nome | Tipo | Valore |
|------|------|--------|
| `AZURE_CREDENTIALS` | Secret | JSON service principal (vedi sotto) |
| `AZURE_FUNCTIONAPP_NAME` | Secret | Nome della Function App su Azure |
| `AZURE_FUNCTIONAPP_RESOURCE_GROUP` | Secret | Resource group della Function App |
| `SQL_CONNECTION_STRING` | Secret | Connection string SQL Server di produzione |
| `VECON_USERNAME` | Secret | Username per il portale webapp.vecon.it |
| `VECON_PASSWORD` | Secret | Password per il portale webapp.vecon.it |

### Creare il service principal Azure

```bash
az ad sp create-for-rbac \
  --name "logistic-app-functions-deploy" \
  --role contributor \
  --scopes /subscriptions/<SUB_ID>/resourceGroups/<RG_NAME> \
  --sdk-auth
```

Copiare l'output JSON come valore del secret `AZURE_CREDENTIALS`.

### Impostare la connection string sull'app Azure

```bash
az functionapp config appsettings set \
  --name <FUNCTIONAPP_NAME> \
  --resource-group <RG_NAME> \
  --settings "SqlConnectionString=<CONNECTION_STRING>"
```

## Passare a un DB reale (produzione)

La `SqlConnectionString` deve puntare allo stesso database SQL Server usato dal backend API.
Per Azure SQL Database la stringa tipica è:

```
Server=tcp:<server>.database.windows.net,1433;Database=logisticapp;
Authentication=Active Directory Default;Encrypt=True;
```

Con Managed Identity (consigliato in produzione) non è necessaria la password nella stringa.

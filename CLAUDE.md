# LogisticApp — Functions — Contesto per Claude Code

## Cos'è questo progetto
Azure Functions in C# per i processi batch di LogisticApp.
Fa parte del monorepo `logistic-app/` insieme al backend API (`../backend/`) e al frontend React (`../frontend/`).

## Stack
- **Azure Functions v4** — isolated worker model
- **.NET 10** — `net10.0`
- **EF Core 10** con provider **SQL Server** — legge/scrive sul DB condiviso con il backend API
- **Microsoft.Azure.Functions.Worker** 2.51.0
- **Microsoft.Azure.Functions.Worker.Sdk** 2.0.7

## Struttura cartelle

```
LogisticApp.Functions/
├── Functions/          # Classi Azure Function (un file per function)
├── Data/
│   └── FunctionsDbContext.cs   # DbContext minimale per le function
├── Models/
│   └── Delivery.cs     # Modelli di dominio (speculari al backend)
├── host.json           # Configurazione runtime Azure Functions
├── local.settings.json # Configurazione locale (NON committare — in .gitignore)
└── Program.cs          # Host builder, DI setup
```

## Avvio locale

Prerequisiti: Azure Functions Core Tools installato (`npm install -g azure-functions-core-tools@4`).

```bash
export PATH="$HOME/.dotnet:$PATH"
cd functions/LogisticApp.Functions

# Aggiornare local.settings.json con la connection string SQL reale
func start
```

## Configurazione

Il valore `SqlConnectionString` in `local.settings.json` (sviluppo) o nell'App Setting di Azure (produzione) deve puntare allo stesso database SQL usato dal backend API.

## Modelli

I modelli in `Models/` replicano solo i campi necessari alle function dal backend.
Se il backend aggiorna lo schema, aggiornare anche qui.

## DeliveryStatus

```csharp
public enum DeliveryStatus
{
    Created,       // record appena creato
    InsertedOk,    // inserimento riuscito
    InsertedError, // inserimento con errori
    NotInserted,   // non ancora inserito
    Error          // errore generico
}
```

## Aggiungere una nuova function

1. Creare un file in `Functions/[NomeFunzione].cs`
2. Iniettare `FunctionsDbContext` e `ILogger<T>` via primary constructor
3. Decorare il metodo con `[Function(nameof(...))]` e il trigger appropriato
4. Registrare eventuali servizi aggiuntivi in `Program.cs`

## Docs

- `docs/ARCHITECTURE.md` — struttura, flusso, pattern, come aggiungere function
- `docs/DEVELOPMENT.md` — setup locale, build, deploy, secrets GitHub richiesti

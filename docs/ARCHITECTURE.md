# Architecture — Functions

## Struttura cartelle

```
LogisticApp.Functions/
├── Functions/                      # Una classe per Azure Function
│   └── ProcessDeliveriesFunction.cs
├── Data/
│   └── FunctionsDbContext.cs       # DbContext EF Core — solo le entità necessarie
├── Models/
│   └── Delivery.cs                 # Modelli di dominio (speculari al backend API)
├── Services/
│   ├── IVeconLoginService.cs       # Interfaccia login browser automation
│   └── VeconLoginService.cs        # Implementazione Selenium WebDriver
├── host.json                       # Configurazione runtime Azure Functions
├── local.settings.json             # Variabili locali (NON committato)
└── Program.cs                      # Host builder — DI, configurazione
```

## Modello di esecuzione

```
Timer / Trigger
      │
      ▼
Azure Functions Runtime (isolated worker)
      │
      ▼
[NomeFunzione].Run(...)
      │
      ├── FunctionsDbContext (EF Core → SQL Server)
      └── ILogger<T> → Application Insights / console
```

Il progetto usa il modello **isolated worker** (non in-process): la function gira in un processo .NET separato dal runtime Functions. Questo garantisce piena compatibilità con .NET 10 e consente di usare le ultime versioni di EF Core.

## DbContext

`FunctionsDbContext` è un DbContext minimale che mappa solo le tabelle necessarie ai batch (`Deliveries`, `Clients`, `ContainerEntry`). Non estende né dipende dal `AppDbContext` del backend API — i modelli in `Models/` sono una copia consapevole.

**Motivazione:** le function devono poter essere deployate e aggiornate indipendentemente dall'API. Il disaccoppiamento evita che una modifica al backend rompa il build delle function.

## Sincronizzazione dei modelli

Se il backend aggiorna `Delivery`, `Client` o `DeliveryStatus`, aggiornare i corrispondenti file in `Models/` e `Data/FunctionsDbContext.cs`.

## Servizi

### VeconLoginService

`Services/VeconLoginService.cs` — automazione browser tramite **Selenium WebDriver** per il portale `webapp.vecon.it`.

```
IVeconLoginService.LoginAsync()
      │
      ▼
Selenium ChromeDriver (headless)
      │
      ├── Naviga a https://webapp.vecon.it/login
      ├── Compila username (config: Vecon:Username)
      ├── Compila password (config: Vecon:Password)
      └── Clicca il bottone di submit
```

Registrato come **scoped** in `Program.cs`. Le credenziali sono lette da `IConfiguration` — in locale da `local.settings.json`, in produzione dagli App Settings Azure (`Vecon__Username` / `Vecon__Password`).

### Flusso ProcessDeliveriesFunction — stato Created

```
Delivery.Status == Created
      │
      ▼
VeconLoginService.LoginAsync()
      │
      ├── successo → log "Login completato"
      └── errore   → log errore, delivery non aggiornata (retry al prossimo tick)
```

## Aggiungere una nuova Function

1. Creare `Functions/[NomeFunzione].cs`
2. Iniettare dipendenze via primary constructor (`FunctionsDbContext`, `ILogger<T>`, servizi custom)
3. Decorare il metodo con `[Function(nameof(...))]` e il trigger appropriato
4. Registrare eventuali servizi aggiuntivi in `Program.cs`

### Trigger disponibili

| Trigger | Package | Uso tipico |
|---------|---------|------------|
| Timer | `Extensions.Timer` (già incluso) | processi batch periodici |
| Queue | `Extensions.Storage.Queues` | elaborazione messaggi asincroni |
| Service Bus | `Extensions.ServiceBus` | code enterprise |
| HTTP | `Extensions.Http` | endpoint leggeri senza API completa |

## DeliveryStatus

```csharp
public enum DeliveryStatus
{
    Created,        // record appena creato dal frontend
    InsertedOk,     // inserimento completato con successo
    InsertedError,  // inserimento eseguito ma con errori
    NotInserted,    // container non ancora inserito
    Error           // errore generico
}
```

Il campo è persistito come stringa (`HasConversion<string>()`) per leggibilità diretta nel DB.

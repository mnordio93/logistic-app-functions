using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LogisticApp.Functions.Data;
using LogisticApp.Functions.Models;

namespace LogisticApp.Functions.Functions;

public class ProcessDeliveriesFunction(FunctionsDbContext db, ILogger<ProcessDeliveriesFunction> logger)
{
    /// <summary>
    /// Legge tutte le delivery con status diverso da InsertedOk e le elabora.
    /// Il trigger è ogni 5 minuti: "0 */5 * * * *"
    /// </summary>
    [Function(nameof(ProcessDeliveriesFunction))]
    public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo)
    {
        logger.LogInformation("ProcessDeliveries avviata — {Time}", DateTime.UtcNow);

        var deliveries = await db.Deliveries
            .Include(d => d.Client)
            .Include(d => d.ContainerNumbers)
            .Where(d => d.Status != DeliveryStatus.InsertedOk)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync();

        if (deliveries.Count == 0)
        {
            logger.LogInformation("Nessuna delivery da elaborare");
            return;
        }

        var byStatus = deliveries
            .GroupBy(d => d.Status)
            .Select(g => $"{g.Key}={g.Count()}")
            .ToList();

        logger.LogInformation(
            "Trovate {Count} delivery da elaborare — {ByStatus}",
            deliveries.Count,
            string.Join(", ", byStatus));

        foreach (var delivery in deliveries)
        {
            logger.LogInformation(
                "Delivery {Id} | Order: {Order} | Client: {Client} | Status: {Status} | Container(s): {Containers}",
                delivery.Id,
                delivery.DeliveryOrder,
                delivery.Client?.CompanyName ?? "N/A",
                delivery.Status,
                string.Join(", ", delivery.ContainerNumbers.Select(c => c.Number)));

            switch (delivery.Status)
            {
                case DeliveryStatus.Created:
                    // TODO: avviare il processo di inserimento
                    break;

                case DeliveryStatus.NotInserted:
                    // TODO: riprovare l'inserimento
                    break;

                case DeliveryStatus.InsertedError:
                    // TODO: notificare l'errore / tentare correzione
                    break;

                case DeliveryStatus.Error:
                    // TODO: gestire errore generico
                    break;
            }
        }

        logger.LogInformation("ProcessDeliveries completata");
    }
}

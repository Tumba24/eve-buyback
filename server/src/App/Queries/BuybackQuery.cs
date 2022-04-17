using EveBuyback.Domain;
using MediatR;

namespace EveBuyback.App;

public record BuybackQuery(string stationName, IEnumerable<BuybackItem> Items) : IRequest<decimal>;

public record BuybackItem(string ItemTypeName, int Volume);

internal class BuybackQueryHandler : IRequestHandler<BuybackQuery, decimal>
{
    public async Task<decimal> Handle(BuybackQuery query, CancellationToken token)
    {
        await Task.CompletedTask;

        var aggregate = new StationOrderSummaryAggregate(
            itemTypeIdLookup: new Dictionary<string, int>(),
            orderSummaryLookup: new Dictionary<int, OrderSummary>(),
            station: new Station(10000002, 60003760, "Jita")
        );

        var currentDateTime = DateTime.UtcNow;

        foreach (var item in query.Items)
            aggregate.RefreshOrderSummary(item.ItemTypeName, item.Volume, currentDateTime);

        var invalidEvents = aggregate.DomainEvents?
            .Where(e => e is InvalidOrderSummaryNoticedEvent)?
            .Select(e => e as InvalidOrderSummaryNoticedEvent) ?? Enumerable.Empty<InvalidOrderSummaryNoticedEvent>();

        foreach (var invalidEvent in invalidEvents)
        {
            if (invalidEvent == null)
                throw new InvalidOperationException();
            
            aggregate.UpdateOrderSummary(
                invalidEvent.Item,
                1000000, 
                new Order[0], 
                currentDateTime);
        }

        return 2.0m;
    }
}
using System.Collections.Concurrent;
using LogiTrack.Domain.Aggregates.Trucks.Events;
using Mediator;

namespace LogiTrack.Api.IntegrationTests.Infrastructure;

// Handler de prueba: guarda los eventos recibidos para comprobar que se publican tras el SaveChanges.
public sealed class TruckStatusChangedSpy : INotificationHandler<TruckStatusChanged>
{
    public static ConcurrentQueue<TruckStatusChanged> Received { get; } = new();

    public ValueTask Handle(TruckStatusChanged notification, CancellationToken cancellationToken)
    {
        Received.Enqueue(notification);
        return ValueTask.CompletedTask;
    }
}

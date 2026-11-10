using Mediator;

namespace LogiTrack.Domain.SharedKernel;

// Hereda de INotification (ADR 0002): el dominio solo conoce ese marcador, no el mecanismo de despacho.
public interface IDomainEvent : INotification
{
}

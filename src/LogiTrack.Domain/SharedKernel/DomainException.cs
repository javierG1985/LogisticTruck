namespace LogiTrack.Domain.SharedKernel;

public sealed class DomainException(string message) : Exception(message);

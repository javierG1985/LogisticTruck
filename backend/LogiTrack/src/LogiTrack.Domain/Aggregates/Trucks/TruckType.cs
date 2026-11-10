namespace LogiTrack.Domain.Aggregates.Trucks;

// Empieza en 1 a propósito: el 0 de un int sin valor no es un tipo válido, así que una petición sin tipo se rechaza.
public enum TruckType
{
    Light = 1,
    Rigid,
    Articulated,
}

namespace LogiTrack.Api.Controllers.Drivers.Requests;

// Cuerpo del PUT de contacto: reemplazo completo de los datos de contacto. El id viene en la ruta y la versión en If-Match.
public sealed record UpdateDriverRequest(
    string FirstName,
    string LastName,
    string Address,
    string Phone,
    string Email,
    string? ProfilePhotoUrl);

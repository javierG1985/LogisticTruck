using System.Linq.Expressions;
using LogiTrack.Domain.SharedKernel;

namespace LogiTrack.Application.Common.Sorting;

// Ordenación por query string ("plate,-createdAt"; el guion indica descendente).
// Cada recurso declara sus campos permitidos una sola vez: el validador y el handler usan el mismo mapa.
public sealed class SortMap<T>
    where T : Entity
{
    private readonly string _defaultSort;
    private readonly Dictionary<string, Func<IOrderedQueryable<T>?, IQueryable<T>, bool, IOrderedQueryable<T>>> _fields = new(StringComparer.Ordinal);

    public SortMap(string defaultSort)
    {
        _defaultSort = defaultSort;
    }

    public IEnumerable<string> FieldNames => _fields.Keys;

    public SortMap<T> Field<TKey>(string name, Expression<Func<T, TKey>> key)
    {
        _fields[name] = (ordered, source, descending) => ordered is null
            ? (descending ? source.OrderByDescending(key) : source.OrderBy(key))
            : (descending ? ordered.ThenByDescending(key) : ordered.ThenBy(key));

        return this;
    }

    // Vacío o con solo campos conocidos. Un campo desconocido se rechaza en el validador.
    public bool Accepts(string? sort) => ParseTokens(sort).All(token => _fields.ContainsKey(token.Field));

    public IOrderedQueryable<T> Apply(IQueryable<T> source, string? sort)
    {
        var requested = ParseTokens(sort).Where(token => _fields.ContainsKey(token.Field)).ToList();
        var tokens = requested.Count > 0 ? requested : ParseTokens(_defaultSort).ToList();

        IOrderedQueryable<T>? ordered = null;

        foreach (var (field, descending) in tokens)
        {
            if (_fields.TryGetValue(field, out var apply))
            {
                ordered = apply(ordered, source, descending);
            }
        }

        var sorted = ordered ?? throw new InvalidOperationException("El orden por defecto no usa ningún campo registrado.");

        // Desempate por Id para que la paginación sea estable entre páginas.
        return sorted.ThenBy(entity => entity.Id);
    }

    private static IEnumerable<(string Field, bool Descending)> ParseTokens(string? sort) =>
        (sort ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => (Field: token.TrimStart('-'), Descending: token.StartsWith('-')));
}

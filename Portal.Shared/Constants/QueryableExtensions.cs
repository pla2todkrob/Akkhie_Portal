using AutoMapper;
using System.Linq.Expressions;

namespace Portal.Shared.Constants
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(
    this IQueryable<T> source,
    string orderBy,
    Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(mappingDictionary, nameof(mappingDictionary));
            if (string.IsNullOrWhiteSpace(orderBy)) return source;

            var orderByAfterSplit = orderBy.Split(',');
            foreach (var orderByClause in orderByAfterSplit)
            {
                var trimmedOrderByClause = orderByClause.Trim();
                var orderDescending = trimmedOrderByClause.EndsWith(" desc");
                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(' ');
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedOrderByClause : trimmedOrderByClause[..indexOfFirstSpace];

                if (!mappingDictionary.TryGetValue(propertyName, out PropertyMappingValue? propertyMappingValue))
                    throw new ArgumentException($"Key mapping for {propertyName} is missing");
                ArgumentNullException.ThrowIfNull(propertyMappingValue, nameof(propertyMappingValue));

                foreach (var destinationProperty in propertyMappingValue.DestinationProperties)
                {
                    if (destinationProperty.Revert)
                        orderDescending = !orderDescending;

                    var parameter = Expression.Parameter(typeof(T), "x");
                    var property = Expression.Property(parameter, destinationProperty.Name);
                    var lambda = Expression.Lambda(property, parameter);

                    var methodName = orderDescending ? "OrderByDescending" : "OrderBy";
                    var method = typeof(Queryable).GetMethods()
                        .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                        .MakeGenericMethod(typeof(T), property.Type);

                    var result = method.Invoke(null, [source, lambda]) ?? throw new InvalidOperationException("The method invocation returned null.");
                    source = (IQueryable<T>)result;
                }
            }

            return source;
        }

        public static IQueryable<T> ApplyPaging<T>(
            this IQueryable<T> source,
            int pageNumber,
            int pageSize)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
    }

    public static class MappingExtensions
    {
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source,
            IConfigurationProvider configuration)
        {
            return source.ProjectTo<TDestination>(configuration);
        }
    }

    public class PropertyMappingValue(IEnumerable<PropertyMapping> destinationProperties)
    {
        public IEnumerable<PropertyMapping> DestinationProperties { get; private set; } = destinationProperties;
    }

    public class PropertyMapping(string name, bool revert = false)
    {
        public string Name { get; set; } = name;
        public bool Revert { get; set; } = revert;
    }
}

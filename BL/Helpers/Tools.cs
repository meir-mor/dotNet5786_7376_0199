namespace Helpers;

internal static class Tools
{
    public static string ToStringProperty<T>(this T t)
    {
        return string.Join(", ", t.GetType().GetProperties().Select(p =>
        {
            var value = p.GetValue(t);
            if (value is null) 
                return $"{p.Name}: null";
            var valueType = value.GetType();
            if (valueType.IsArray || typeof(ICollection<>).IsAssignableFrom(valueType))
            {
                var items = value is Array array ? array.Cast<object>() : (ICollection<object>)value;
                return $"{p.Name}: [{string.Join(", ", items.Select(i => i.ToString()))}]";
            }
            return $"{p.Name}: {value}";
        }));
    }
}
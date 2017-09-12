using System;

namespace EntityFrameworkCore.RawSQLExtensions.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsSqlSimpleType(this Type type)
        {
            var t = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                t = Nullable.GetUnderlyingType(type);

            return t.IsPrimitive || t.Equals(typeof(string)) ||
                   t.Equals(typeof(DateTime)) || t.Equals(typeof(DateTimeOffset)) || t.Equals(typeof(TimeSpan)) ||
                   t.Equals(typeof(Guid)) ||
                   t.Equals(typeof(byte[])) || t.Equals(typeof(char[]));
        }
    }
}

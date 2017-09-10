using System;

namespace EntityFrameworkCore.RawSQLExtensions.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive || type.Equals(typeof(string));
        }
    }
}

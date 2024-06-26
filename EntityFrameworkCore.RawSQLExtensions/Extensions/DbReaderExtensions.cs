﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EntityFrameworkCore.RawSQLExtensions.Options;

namespace EntityFrameworkCore.RawSQLExtensions.Extensions;

public static class DbReaderExtensions
{
    public static IDictionary<string, DbColumn> GetSchema<T>(this DbDataReader dr)
    {
        IDictionary<string, DbColumn> valuePairs;
        if (typeof(T).IsTupleType())
        {
            var props = typeof(T).GetRuntimeFields();
            valuePairs = dr.GetColumnSchema().ToDictionary(key => key.ColumnName.ToLower());
        }
        else
        {
            var props = GetRuntimeProperties<T>();
            valuePairs = dr.GetColumnSchema()
                .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
                .ToDictionary(key => key.ColumnName.ToLower());
        }
        return valuePairs;
    }

    public static T MapObject<T>(this DbDataReader dr, IDictionary<string, DbColumn> colMapping)
    {
        if (typeof(T).IsSqlSimpleType())
        {
            var dbValue = dr.GetValue(0);

            if (
                RawSQLExtensionsOptions.AllowStringGuidConversion
                && typeof(T) == typeof(Guid)
                && dbValue is string dbStringValue
            )
            {
                object obj = new Guid(dbStringValue); // case when db is string and property is Guid
                return (T)obj;
            }

            return (T)dbValue;
        }
        else
        {
            T obj = Activator.CreateInstance<T>();
            if (typeof(T).IsTupleType())
            {
                var fields = typeof(T).GetRuntimeFields().ToArray();
                //https://stackoverflow.com/questions/59000557/valuetuple-set-fields-via-reflection
                object xobj = obj;
                for (int i = 0; i < fields.Length; i++)
                {
                    var val = Convert.ChangeType(dr.GetValue(i), fields[i].FieldType);
                    fields[i].SetValue(xobj, val == DBNull.Value ? null : val);
                }
                obj = (T)Convert.ChangeType(xobj, typeof(T));
            }
            else
            {
                IEnumerable<PropertyInfo> props = GetRuntimeProperties<T>();
                foreach (var prop in props)
                {
                    var propName = prop.Name.ToLower();
                    if (colMapping.ContainsKey(propName))
                    {
                        var dbValue = dr.GetValue(colMapping[prop.Name.ToLower()].ColumnOrdinal.Value);

                        var type = Nullable.GetUnderlyingType(prop.PropertyType);
                        if (type != null && type.IsEnum)
                        {
                            dbValue = dbValue == DBNull.Value ? null : Enum.ToObject(type, dbValue);
                        }

                        if (
                            RawSQLExtensionsOptions.AllowStringGuidConversion
                            && prop.PropertyType == typeof(Guid)
                            && dbValue is string dbStringValue
                        )
                        {
                            prop.SetValue(obj, new Guid(dbStringValue)); // case when db is string and property is Guid
                        }
                        else
                        {
                            prop.SetValue(obj, dbValue == DBNull.Value ? null : dbValue);
                        }
                    }
                    else
                    {
                        prop.SetValue(obj, null);
                    }
                }
            }

            return obj;
        }
    }

    public static async Task<IList<T>> ToListAsync<T>(this DbDataReader dr)
    {
        var objList = new List<T>();

        var colMapping = dr.GetSchema<T>();

        if (dr.HasRows)
            while (await dr.ReadAsync())
                objList.Add(dr.MapObject<T>(colMapping));

        return objList;
    }

    public static IList<T> ToList<T>(this DbDataReader dr)
    {
        var objList = new List<T>();
        var colMapping = dr.GetSchema<T>();
        if (dr.HasRows)
            while (dr.Read())
                objList.Add(dr.MapObject<T>(colMapping));

        return objList;
    }

    public static DataTable ToDataTable(this DbDataReader dr)
    {
        DataTable objDataTable = new DataTable();
        for (int intCounter = 0; intCounter < dr.FieldCount; ++intCounter)
        {
            objDataTable.Columns.Add(dr.GetName(intCounter), dr.GetFieldType(intCounter));
        }
        if (dr.HasRows)
        {
            objDataTable.BeginLoadData();
            object[] objValues = new object[dr.FieldCount];
            while (dr.Read())
            {
                dr.GetValues(objValues);
                objDataTable.LoadDataRow(objValues, true);
            }
            objDataTable.EndLoadData();
        }
        return objDataTable;
    }

    public static async Task<DataTable> ToDataTableAsync(this DbDataReader dr)
    {
        DataTable objDataTable = new DataTable();
        for (int intCounter = 0; intCounter < dr.FieldCount; ++intCounter)
        {
            objDataTable.Columns.Add(dr.GetName(intCounter), dr.GetFieldType(intCounter));
        }
        if (dr.HasRows)
        {
            objDataTable.BeginLoadData();
            object[] objValues = new object[dr.FieldCount];
            while (await dr.ReadAsync())
            {
                dr.GetValues(objValues);
                objDataTable.LoadDataRow(objValues, true);
            }
            objDataTable.EndLoadData();
        }
        return objDataTable;
    }

    public static async Task<T> FirstOrDefaultAsync<T>(this DbDataReader dr)
    {
        var colMapping = dr.GetSchema<T>();
        if (dr.HasRows)
            while (await dr.ReadAsync())
                return dr.MapObject<T>(colMapping);

        return default;
    }

    public static T FirstOrDefault<T>(this DbDataReader dr)
    {
        var colMapping = dr.GetSchema<T>();
        if (dr.HasRows)
            while (dr.Read())
                return dr.MapObject<T>(colMapping);

        return default;
    }

    public static async Task<T> SingleOrDefaultAsync<T>(this DbDataReader dr)
    {
        var colMapping = dr.GetSchema<T>();

        T obj = default;
        bool hasResult = false;

        if (dr.HasRows)
            while (await dr.ReadAsync())
            {
                if (hasResult)
                    throw new InvalidOperationException("Sequence contains more than one matching element");

                obj = dr.MapObject<T>(colMapping);
                hasResult = true;
            }

        return obj;
    }

    public static T SingleOrDefault<T>(this DbDataReader dr)
    {
        var colMapping = dr.GetSchema<T>();
        T obj = default;
        bool hasResult = false;

        if (dr.HasRows)
            while (dr.Read())
            {
                if (hasResult)
                    throw new InvalidOperationException("Sequence contains more than one matching element");

                obj = dr.MapObject<T>(colMapping);
                hasResult = true;
            }

        return obj;
    }

    private static IEnumerable<PropertyInfo> GetRuntimeProperties<T>()
    {
        var props = typeof(T).GetRuntimeProperties();
        return props.Where(p => !p.CustomAttributes.Any(ca => ca.AttributeType == typeof(NotMappedAttribute))).ToList();
    }
}

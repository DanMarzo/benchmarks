using DataApp.Annotations;
using System.Data;
using System.Reflection;

namespace DataApp.Generators;

public static class GenerateSQL
{
    public static string InsertSQL<T>(this T _) where T : class
    {
        Type type = typeof(T);
        var columns = GetColumns(type);
        return $"""
            INSERT INTO {GetNameTable(type)} ({string.Join(", ", columns.Select(item => $"\"{item}\""))})
            OUTPUT INSERTED.*
            VALUES ({string.Join(", ", columns.Select(x => $"@{x}"))})
            """;
    }

    public static string SelectSQL<T>(this T _) where T : class
    {
        Type type = typeof(T);
        IsValidClass(type);
        return $"SELECT * FROM {GetNameTable(type)}";
    }

    public static string SelectSQL<T>(this T _, bool includeColumns = false) where T : class
    {
        Type type = typeof(T);
        IsValidClass(type);
        var table = GetNameTable(type);
        return includeColumns ? $"SELECT {string.Join(", ", GetColumns(type))} FROM {table}" : $"SELECT * FROM {table}";
    }

    public static string DeleteSQL<T>(this T _) where T : class
    {
        Type type = typeof(T);
        return $"DELETE FROM {GetNameTable(type)} ";
    }

    public static string UpdateSQL<T>(this T _)
    {
        Type type = typeof(T);
        var table = GetNameTable(type);

        return $"UPDATE {table} ";
    }

    private static IEnumerable<string> GetColumns(Type type)
    {
        IsValidClass(type);
        var columns = new List<string>();
        var listProperties = type.GetProperties();

        foreach (var property in listProperties ?? [])
        {
            var attProp = property.GetCustomAttribute<ColumnDB>();
            if (attProp is not null && attProp.Ignorar)
                continue;
            columns.Add(property.Name);
        }
        return columns;
    }

    public static string GetNameTable(Type type)
    {
        return type.GetCustomAttribute<TableDB>()?
            .Nome ?? type.Name;
    }

    public static string GetNameTable<T>(this T _) where T : class
    {
        Type type = typeof(T);
        var nameTable = type.Name;
        var attributeTableDB = type.GetCustomAttribute<TableDB>();

        if (attributeTableDB is not null)
            nameTable = attributeTableDB.Nome;

        return nameTable;
    }

    private static void IsValidClass(Type type)
    {
        if (!type.IsClass)
            throw new DataException("Somente classes sao aceitas");
    }
}

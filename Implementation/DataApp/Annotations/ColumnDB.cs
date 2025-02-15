namespace DataApp.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnDB : Attribute
{
    public bool Ignorar { get; set; } = false;
}

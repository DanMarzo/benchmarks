namespace DataApp.Annotations;

[AttributeUsage(AttributeTargets.Class)]
public class TableDB : Attribute
{
    public string Nome { get; set; }
}

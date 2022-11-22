namespace AdxUtils.Export;

public class ReaderRecord
{
    public List<string> Fields { get; } = new();

    public List<Type> FieldTypes { get; } = new();

    public List<object> Values { get; } = new();

    public int FieldCount => Fields.Count;
}
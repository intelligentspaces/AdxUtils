namespace AdxUtils.Export;

public class KustoAdminException : Exception
{
    public KustoAdminException(string message) : base(message) { }
    
    public KustoAdminException(string message, Exception innerException) : base(message, innerException) { }
}
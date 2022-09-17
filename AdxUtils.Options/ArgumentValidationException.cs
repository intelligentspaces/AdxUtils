namespace AdxUtils.Options;

public class ArgumentValidationException : Exception
{
    public ArgumentValidationException(string message) : base(message) { }

    public ArgumentValidationException(string message, Exception innerException) : base(message, innerException) { }
}
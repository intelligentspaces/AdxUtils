﻿namespace AdxUtils.Options;

/// <summary>
/// Represents an error that can occur when validating arguments
/// </summary>
public class ArgumentValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ArgumentValidationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public ArgumentValidationException(string message, Exception innerException) : base(message, innerException) { }
}
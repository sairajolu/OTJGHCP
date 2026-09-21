namespace TaskBridge.Application.Exceptions;

/// <summary>Indicates that an audit date range is invalid.</summary>
public sealed class InvalidAuditDateRangeException : ArgumentException
{
    /// <summary>Initializes the exception for an invalid date range.</summary>
    public InvalidAuditDateRangeException()
        : base("The audit query start date must be earlier than or equal to the end date.")
    {
    }
}

namespace TaskBridge.Application.Exceptions;

/// <summary>Indicates that trusted event tenant data differs from the authenticated tenant.</summary>
public sealed class TenantContextMismatchException : UnauthorizedAccessException
{
    /// <summary>Initializes the exception for a tenant mismatch.</summary>
    public TenantContextMismatchException()
        : base("The event organisation does not match the authenticated organisation.")
    {
    }
}

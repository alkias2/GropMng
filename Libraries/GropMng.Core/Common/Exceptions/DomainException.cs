namespace GropMng.Core.Common.Exceptions;

/// <summary>
/// Represents the DomainException component.
/// Defines responsibilities and data relevant to its role in the GropMng solution.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

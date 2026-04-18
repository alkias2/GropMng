namespace GropMng.Core.Domain.Logging;

/// <summary>
/// Represents a log level
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Trace
    /// </summary>
    Trace = 10,

    /// <summary>
    /// Debug
    /// </summary>
    Debug = 20,

    /// <summary>
    /// Information
    /// </summary>
    Information = 30,

    /// <summary>
    /// Warning
    /// </summary>
    Warning = 40,

    /// <summary>
    /// Error
    /// </summary>
    Error = 50,

    /// <summary>
    /// Critical
    /// </summary>
    Critical = 60
}
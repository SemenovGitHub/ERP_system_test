using System.ComponentModel.DataAnnotations;

namespace ERP.Persistence.Configuration;

/// <summary>
/// Configuration settings for MongoDB connection
/// </summary>
public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    /// <summary>
    /// MongoDB connection string
    /// </summary>
    [Required]
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>
    /// Database name
    /// </summary>
    [Required]
    public string DatabaseName { get; init; } = string.Empty;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Socket timeout in seconds
    /// </summary>
    public int SocketTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Maximum connection pool size
    /// </summary>
    public int MaxConnectionPoolSize { get; init; } = 100;

    /// <summary>
    /// Validate configuration values
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new ArgumentException("MongoDB connection string cannot be empty", nameof(ConnectionString));
        
        if (string.IsNullOrWhiteSpace(DatabaseName))
            throw new ArgumentException("MongoDB database name cannot be empty", nameof(DatabaseName));
        
        if (ConnectionTimeoutSeconds <= 0)
            throw new ArgumentException("Connection timeout must be positive", nameof(ConnectionTimeoutSeconds));
        
        if (SocketTimeoutSeconds <= 0)
            throw new ArgumentException("Socket timeout must be positive", nameof(SocketTimeoutSeconds));
        
        if (MaxConnectionPoolSize <= 0)
            throw new ArgumentException("Max connection pool size must be positive", nameof(MaxConnectionPoolSize));
    }
}
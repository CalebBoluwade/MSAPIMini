namespace MS.API.Mini.Exceptions;

/// <summary>
/// Exception thrown when attempting to create or add an entity that already exists.
/// </summary>
[Serializable]
public class DuplicateEntityException : Exception
{
    /// <summary>
    /// Gets the name of the entity type that caused the duplicate error.
    /// </summary>
    public string? EntityType { get; }

    /// <summary>
    /// Gets the identifier or key that caused the duplicate error.
    /// </summary>
    public object? EntityKey { get; }

    /// <summary>
    /// Initializes a new instance of the DuplicateEntityException class.
    /// </summary>
    public DuplicateEntityException()
        : base("An entity with the same key already exists.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the DuplicateEntityException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DuplicateEntityException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DuplicateEntityException class with a specified error message and entity type.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="entityType">The type of entity that caused the duplicate error.</param>
    public DuplicateEntityException(string message, string entityType)
        : base(message)
    {
        EntityType = entityType;
    }

    /// <summary>
    /// Initializes a new instance of the DuplicateEntityException class with entity type and key.
    /// </summary>
    /// <param name="entityType">The type of entity that caused the duplicate error.</param>
    /// <param name="entityKey">The key or identifier that caused the duplicate error.</param>
    public DuplicateEntityException(string entityType, object entityKey)
        : base($"A {entityType} with key '{entityKey}' already exists.")
    {
        EntityType = entityType;
        EntityKey = entityKey;
    }

    /// <summary>
    /// Initializes a new instance of the DuplicateEntityException class with a specified error message, entity type, and key.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="entityType">The type of entity that caused the duplicate error.</param>
    /// <param name="entityKey">The key or identifier that caused the duplicate error.</param>
    public DuplicateEntityException(string message, string entityType, object entityKey)
        : base(message)
    {
        EntityType = entityType;
        EntityKey = entityKey;
    }

    /// <summary>
    /// Initializes a new instance of the DuplicateEntityException class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DuplicateEntityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DuplicateEntityException class with a specified error message, entity details, and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="entityType">The type of entity that caused the duplicate error.</param>
    /// <param name="entityKey">The key or identifier that caused the duplicate error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DuplicateEntityException(string message, string entityType, object entityKey, Exception innerException)
        : base(message, innerException)
    {
        EntityType = entityType;
        EntityKey = entityKey;
    }
}
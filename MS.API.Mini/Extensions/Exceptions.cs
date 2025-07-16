using System;
namespace MS.API.Mini.Extensions;

public class Exceptions
{
    /// <summary>
    /// Exception thrown when a required configuration value is missing or invalid
    /// </summary>
    [Serializable]
    public class ConfigurationMissingException : Exception
    {
        /// <summary>
        /// The name of the missing configuration key
        /// </summary>
        public string? ConfigurationKey { get; }

        // Constructors
        public ConfigurationMissingException() { }

        public ConfigurationMissingException(string message) 
            : base(message) { }

        public ConfigurationMissingException(string message, Exception inner) 
            : base(message, inner) { }

        public ConfigurationMissingException(string message, string configurationKey) 
            : this(message)
        {
            ConfigurationKey = configurationKey;
        }

        public ConfigurationMissingException(string message, string configurationKey, Exception inner) 
            : this(message, inner)
        {
            ConfigurationKey = configurationKey;
        }

        // Required for serialization
        protected ConfigurationMissingException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) 
        {
            ConfigurationKey = info.GetString(nameof(ConfigurationKey));
        }

        public override void GetObjectData(
            System.Runtime.Serialization.SerializationInfo info, 
            System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ConfigurationKey), ConfigurationKey);
        }
    }
}
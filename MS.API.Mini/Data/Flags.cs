namespace MS.API.Mini.Data;

public enum MonitoringStatus
{
    UnknownStatus = 0,
    Healthy = 1,
    Escalation = 2,
    Acknowledged = 3,
    Degraded = 4,
    InvalidConfiguration = 5,
    Scheduled
}

public enum PluginType
{
    HealthCheck = 0,
    Performance,
    Security,
    Custom,
    Agent,
    Unknown
}
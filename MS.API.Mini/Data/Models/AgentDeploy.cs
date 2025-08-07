namespace MS.API.Mini.Data.Models;

public class AgentDeploymentRequest
{
    public required string DeployUser { get; set; }
    public required string DeployPassword { get; set; }
    public required Guid AppOwnerID { get; set; }
    public string AgentPort { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string AgentVersion { get; set; } = string.Empty;
    public required string[] Servers { get; set; } = [];
    
    public string AgentID { get; set; } = string.Empty;
    
    // public required MetricThreshold Threshold { get; set; }
}

public class DeploymentStatus
{
    public required string Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Environment { get; set; }
    public string[] Servers { get; set; }
    public string Output { get; set; }
    public string Error { get; set; }
    public string ErrorMessage { get; set; }
}
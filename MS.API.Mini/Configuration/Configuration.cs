namespace MS.API.Mini.Configuration;

public class AgentConfiguration
{
    public required string GitUsername { get; set; }

    public required string GitToken { get; set; }
    
    public required string GitRepository { get; set; }
    
    public required string DefaultPort { get; set; }

    public required string AgentRepositoryPath { get; set; }
    public required Dictionary<string, LicenseOptions> LicenseOptions { get; set; }
}

public class LicenseOptions
{
    public required int MaxUsers { get; set; }
    public required int Agents { get; set; }
    public required int GracePeriod { get; set; }
}

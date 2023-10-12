namespace ElasticSearchCommon.Settings;

public class ElasticConfiguration
{
    public string ElasticUrl { get; set; }
    public string? DefaultIndex { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }

    public string? ApplicationName { get; set; }
}
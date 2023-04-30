#nullable disable warnings

namespace Remindery.Configuration;

public class DebugConfiguration
{
    public const string SectionName = "Debug";

    public bool LogHttpRequests { get; set; }
}

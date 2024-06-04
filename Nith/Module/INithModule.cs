using NITHlibrary.Nith.Internals;

namespace NITHlibrary.Nith.Module
{
    public interface INithModule
    {
        List<NithParameters> ExpectedArguments { get; set; }
        List<string> ExpectedSensorNames { get; set; }
        List<string> ExpectedVersions { get; set; }
    }
}

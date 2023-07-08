using System.Linq;
using System.Reflection;

namespace CentCom.Common.Util;

public record AssemblyInformation(string Version, string Commit, string CopyrightNotice)
{
    // Note that if this was ever used in a situation where projects had differing version numbers this is 
    // getting the version number/details of CentCom.Common
    public static readonly AssemblyInformation Current = new(typeof(AssemblyInformation).Assembly);

    private AssemblyInformation(Assembly assembly) : this(
        assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(x => x.Key == "MinVerVersion")
            ?.Value,
        assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(x => x.Key == "SourceRevisionId")
            ?.Value,
        assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright)
    {
    }
}

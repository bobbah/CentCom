using CentCom.Common.Util;

namespace CentCom.API.Models;

/// <summary>
/// Contains the status summary of the application.
/// </summary>
/// <param name="Version">The version of the application in semver</param>
/// <param name="Commit">The commit hash of the build that the application is running</param>
/// <param name="CopyrightNotice">The copyright notice for this version of the application</param>
public record AppVersionDTO(string Version, string Commit, string CopyrightNotice)
{
    internal AppVersionDTO(AssemblyInformation assemblyInfo) : this(assemblyInfo.Version, assemblyInfo.Commit,
        assemblyInfo.CopyrightNotice)
    {
    }
}
using System;
using System.Linq;
using System.Reflection;
using CentCom.API.Models;

namespace CentCom.API.Services.Implemented;

internal record AssemblyInformation(string Version, string Commit, string CopyrightNotice)
{
    internal static readonly AssemblyInformation Current = new(typeof(AssemblyInformation).Assembly);

    private AssemblyInformation(Assembly assembly) : this(
        assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(x => x.Key == "MinVerVersion")
            ?.Value,
        assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(x => x.Key == "SourceRevisionId")
            ?.Value,
        assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright)
    {
    }
}

public class AppStatusService : IAppStatusService
{
    private static AppVersionDTO _versionDTO;

    public AppStatusService()
    {
        _versionDTO = new AppVersionDTO(AssemblyInformation.Current);
    }

    /// <inheritdoc />
    public ReadOnlySpan<char> GetVersion() => AssemblyInformation.Current.Version;

    /// <inheritdoc />
    public ReadOnlySpan<char> GetBuildCommit(int maxHashLength = 7)
    {
        if (maxHashLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxHashLength), "Hash length cannot be less than zero!");
        if (string.IsNullOrEmpty(AssemblyInformation.Current.Commit))
            return null;

        var commitSpan = AssemblyInformation.Current.Commit.AsSpan();
        return commitSpan[..(Math.Clamp(maxHashLength, 1, commitSpan.Length) - 1)];
    }

    /// <inheritdoc />
    public ReadOnlySpan<char> GetCopyrightNotice() => AssemblyInformation.Current.CopyrightNotice;

    /// <inheritdoc />
    public AppVersionDTO GetAppVersionDTO() => _versionDTO;
}
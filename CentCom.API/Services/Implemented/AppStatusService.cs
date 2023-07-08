using System;
using CentCom.API.Models;
using CentCom.Common.Util;

namespace CentCom.API.Services.Implemented;

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
        return commitSpan[..Math.Clamp(maxHashLength, 0, commitSpan.Length)];
    }

    /// <inheritdoc />
    public ReadOnlySpan<char> GetCopyrightNotice() => AssemblyInformation.Current.CopyrightNotice;

    /// <inheritdoc />
    public AppVersionDTO GetAppVersionDTO() => _versionDTO;
}
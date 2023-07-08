using System;
using CentCom.API.Models;

namespace CentCom.API.Services;

public interface IAppStatusService
{
    /// <summary>
    /// Get the version for the application.
    /// </summary>
    /// <returns>The current version</returns>
    ReadOnlySpan<char> GetVersion();
    
    /// <summary>
    /// Get the build commit for the application's current version.
    /// </summary>
    /// <param name="maxHashLength">The maximum length of the hash to return</param>
    /// <returns>The commit from the build of this version of the application</returns>
    ReadOnlySpan<char> GetBuildCommit(int maxHashLength = 7);

    /// <summary>
    /// Get the copyright notice for the application's current version.
    /// </summary>
    /// <returns>The copyright notice, if present at build time</returns>
    ReadOnlySpan<char> GetCopyrightNotice();

    /// <summary>
    /// Returns a DTO appropriate for transmission for the current version of the application.
    /// </summary>
    /// <returns>The completed DTO</returns>
    AppVersionDTO GetAppVersionDTO();
}
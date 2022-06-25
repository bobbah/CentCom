using System;
using System.Reflection;

namespace CentCom.Bot;

public static class VersionUtility
{
    private static Version ExecutingVersion => Assembly.GetExecutingAssembly().GetName().Version;
    public static string VersionNumber => ExecutingVersion.ToString(ExecutingVersion.Revision == 0 ? 3 : 4);
    public static string Version => $"CentCom.Bot v{VersionNumber}";
}
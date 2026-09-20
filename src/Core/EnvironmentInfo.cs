using System.Runtime.InteropServices;

namespace Core;

public sealed record EnvironmentReport(
    string Application,
    string Student,
    string Group,
    string OsDescription,
    string OsVersion,
    string ProcessArchitecture,
    string DotNetVersion,
    string Runtime,
    string AppDirectory,
    string CurrentDirectory,
    string Domain,
    string DetectedRid,
    string ReportedRid);

public static class EnvironmentInfo
{
    public static EnvironmentReport Collect() => new(
        Application: "CrossApp",
        Student: "Миськів Вікторія",
        Group: "ФЕІ-37",
        OsDescription: RuntimeInformation.OSDescription,
        OsVersion: Environment.OSVersion.ToString(),
        ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString(),
        DotNetVersion: Environment.Version.ToString(),
        Runtime: RuntimeInformation.FrameworkDescription,
        AppDirectory: AppContext.BaseDirectory,
        CurrentDirectory: Environment.CurrentDirectory,
        Domain: "Склад (товари, партії, залишки, переміщення)",
        DetectedRid: DetectRid(),
        ReportedRid: RuntimeInformation.RuntimeIdentifier);

    private static string DetectRid()
    {
        string os = 
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "unknown";

        string arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "unknown"
        };

        return $"{os}-{arch}";
    }
}
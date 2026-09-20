using System.Text.Json;
using Core;

Console.OutputEncoding = System.Text.Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Contains("--json"))
{
    string jsonString = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = false });
    Console.WriteLine(jsonString);
}
else
{
    Console.WriteLine("CrossApp - практикум з крос-платформного програмування");
    Console.WriteLine($"Студент: {report.Student}, група {report.Group}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription) : {report.OsDescription}");
    Console.WriteLine($"ОС (Environment)   : {report.OsVersion}");
    Console.WriteLine($"Архітектура процесу: {report.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)  : {report.DotNetVersion}");
    Console.WriteLine($"Runtime            : {report.Runtime}");
    Console.WriteLine($"RID (визначено)    : {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET)     : {report.ReportedRid}");
    Console.WriteLine($"Каталог застосунку : {report.AppDirectory}");
    Console.WriteLine($"Поточний каталог   : {report.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область  : {report.Domain}");
}
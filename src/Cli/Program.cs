using System.Runtime.InteropServices;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var info = new
{
    Application = "CrossApp",
    Student = "Миськів Вікторія",
    Group = "ФЕІ-37",
    OSDescription = RuntimeInformation.OSDescription,
    OSVersion = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    AppDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Склад (товари, партії, залишки, переміщення)"
};

if (args.Contains("--json"))
{
    string jsonString = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = false });
    Console.WriteLine(jsonString);
}
else
{
    Console.WriteLine("CrossApp - практикум з крос-платформного програмування");
    Console.WriteLine("Студент: Миськів Вікторія, група ФЕІ-37");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription): {info.OSDescription}");
    Console.WriteLine($"ОС (Environment) : {info.OSVersion}");
    Console.WriteLine($"Архітектура процесу: {info.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR): {info.DotNetVersion}");
    Console.WriteLine($"Runtime            : {info.Runtime}");
    Console.WriteLine($"Каталог застосунку: {info.AppDirectory}");
    Console.WriteLine($"Поточний каталог : {info.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {info.Domain}");
}
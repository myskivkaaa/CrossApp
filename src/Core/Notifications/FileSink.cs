// додаткове 6
namespace Core.Notifications;

public sealed class FileSink : INotificationSink
{
    private readonly string _filePath;

    public FileSink(string filePath)
    {
        _filePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
    }

    public void Notify(string message)
    {
        File.AppendAllText(_filePath, message + Environment.NewLine);
    }
}
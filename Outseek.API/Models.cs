using System.IO;

namespace Outseek.API;

public sealed record Segment(double FromSeconds, double ToSeconds);
public sealed record TimedTextEntry(double FromSeconds, double ToSeconds, string Text);

public interface ITimelineProcessContext
{
    public double Minimum { get; }
    public double Maximum { get; }

    public string GetStorageDirectory(string subdirectory);
}

public sealed class TimelineProcessContext : ITimelineProcessContext
{
    public double Minimum { get; }
    public double Maximum { get; }

    public TimelineProcessContext(double minimum, double maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public string GetStorageDirectory(string? subdirectory)
    {
        string storageDir = Outseek.StorageDirectory;
        if (subdirectory != null)
        {
            storageDir = Path.Join(storageDir, subdirectory);
            Directory.CreateDirectory(storageDir);
        }
        return storageDir;
    }
}

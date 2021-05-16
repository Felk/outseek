namespace Outseek.API
{
    public sealed record Segment(double FromSeconds, double ToSeconds);
    public sealed record TimedTextEntry(double FromSeconds, double ToSeconds, string Text);

    public sealed record TimelineProcessContext(
        double Minimum,
        double Maximum
    );
}

namespace Outseek.AvaloniaClient.Utils
{
    public struct Range
    {
        public double From { get; init; }
        public double To { get; init; }

        public double Size => To - From;

        public Range(double from, double to)
        {
            From = from;
            To = to;
        }

        public static Range operator /(Range range, double divisor) => new(range.From / divisor, range.To / divisor);
        public static Range operator *(Range range, double factor) => new(range.From * factor, range.To * factor);

        public override string ToString() => $"Range({From}, {To})";
    }
}

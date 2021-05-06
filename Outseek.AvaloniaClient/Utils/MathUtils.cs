using System;

namespace Outseek.AvaloniaClient.Utils
{
    public static class MathUtils
    {
        public static double RoundToIncrement(double val, double increment)
        {
            if (increment <= 0) throw new ArgumentException("increment must be positive", nameof(increment));
            double remainder = Math.IEEERemainder(val, increment);
            return remainder < increment / 2
                ? val - remainder
                : val + remainder;
        }
    }
}

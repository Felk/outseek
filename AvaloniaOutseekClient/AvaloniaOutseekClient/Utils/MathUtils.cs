using System;

namespace AvaloniaOutseekClient.Utils
{
    public static class MathUtils
    {
        public static double RoundToIncrement(double val, double increment) =>
            val - Math.IEEERemainder(val, increment);
    }
}

using System;

namespace Aircadia.ObjectModel.Util
{
	public static class ExtensionMethods
    {
        public static bool IsApproximatelyEqualTo(this double initialValue, double value)
        {
            return IsApproximatelyEqualTo(initialValue, value, 0.000000001);
        }

        public static bool IsApproximatelyEqualTo(this double initialValue, double value, double maximumDifferenceAllowed)
        {
            // Handle comparisons of floating point values that may not be exactly the same
            return (Math.Abs(initialValue - value) < maximumDifferenceAllowed);
        }
    }
}

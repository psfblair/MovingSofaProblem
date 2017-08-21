namespace Domain
{
    internal class Utilities
    {
        internal static float DoubleToFloat(double value)
        {
            float result = (float)value;
            if (float.IsPositiveInfinity(result))
            {
                return float.MaxValue;
            }
            else if (float.IsNegativeInfinity(result))
            {
                return float.MinValue;
            }
            else
            {
                return result;
            }
        }
    }
}

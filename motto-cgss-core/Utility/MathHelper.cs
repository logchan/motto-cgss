namespace motto_cgss_core.Utility
{
    public static class MathHelper
    {
        public static int Clamp(int value, int min, int max)
        {
            return value > max ? max : (value < min ? min : value);
        }

        public static double Clamp(double value, double min, double max)
        {
            return value > max ? max : (value < min ? min : value);
        }
    }
}

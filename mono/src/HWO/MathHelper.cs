using System;

namespace HWO
{
    public static class MathHelper
    {
        public static double GetCornerPieceLength(double angle, int radius)
        {
            return 2 * Math.PI * radius * (Math.Abs(angle) / 360d);
        }
    }
}
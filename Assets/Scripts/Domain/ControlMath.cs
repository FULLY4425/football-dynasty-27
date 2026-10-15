using System;

namespace FootballDynasty
{
    public static class ControlMath
    {
        // Radial dead zone: preserves direction and prevents faster diagonal movement.
        public static void Analog(float x, float y, float deadZone, out float ox, out float oy)
        {
            if (float.IsNaN(x) || float.IsNaN(y) || float.IsInfinity(x) || float.IsInfinity(y))
            {
                ox = oy = 0;
                return;
            }

            deadZone = Math.Max(0, Math.Min(.95f, deadZone));
            float length = (float)Math.Sqrt(x * x + y * y);
            if (length <= deadZone)
            {
                ox = oy = 0;
                return;
            }

            float strength = (Math.Min(length, 1) - deadZone) / (1 - deadZone);
            ox = x / length * strength;
            oy = y / length * strength;
        }
    }
}

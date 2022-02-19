using System;
using VRageMath;


namespace BeamSystem
{
    static class ColorExtensions
    {
        internal static void GetSV(this Vector4 color, out float saturation, out float value)
        {
            float max = Math.Max(color.X, Math.Max(color.Y, color.Z));
            float min = Math.Min(color.X, Math.Min(color.Y, color.Z));

            saturation = (max == 0) ? 0f : 1f - (min / max);
            value = max;
        }

        private static Vector3 Hue(float H)
        {
            float R = Math.Abs(H * 6 - 3) - 1;
            float G = 2 - Math.Abs(H * 6 - 2);
            float B = 2 - Math.Abs(H * 6 - 4);
            return (new Vector3(MathHelper.Clamp(R, 0f, 1f), MathHelper.Clamp(G, 0f, 1f), MathHelper.Clamp(B, 0f, 1f)));
        }

        internal static Vector4 HSVtoColor(float hue, float saturation, float value, float weight)
            => new Vector4(((Hue(hue) - 1f) * saturation + 1f) * value, weight);
    }
}
using VRageMath;


namespace BeamSystem
{
    static class VectorExtensions
    {
        internal static Color ToColor(this Vector4 vector)
        {
            return Color.FromNonPremultiplied(vector);
        }

        internal static Vector3I And(this Vector3I a, Vector3I b)
            => new Vector3I(
                x: a.X & b.X,
                y: a.Y & b.Y,
                z: a.Z & b.Z);
    }
}

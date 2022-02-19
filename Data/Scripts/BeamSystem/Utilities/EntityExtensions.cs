using Sandbox.ModAPI;
using VRage.ModAPI;


namespace BeamSystem
{
    static class EntityExtensions
    {
        internal static double GetDistanceSquaredFromCamera(this IMyEntity entity)
        {
            var cam = MyAPIGateway.Session.Camera;
            if (null == cam) return double.MaxValue;
            var position = cam.WorldMatrix.Translation;
            return (position - entity.WorldMatrix.Translation).LengthSquared();
        }
    }
}

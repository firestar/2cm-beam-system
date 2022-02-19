using Sandbox.Game.Entities;
using System;
using VRage.Game.ModAPI;
using VRageMath;


namespace BeamSystem
{
    static class RaycastExtensions
    {
        internal static IMySlimBlock GetHitSubEntity(this IHitInfo hitInfo, double prediction, ref Vector3D dir, Func<IMySlimBlock, bool> pred = null)
        {
            var grid = hitInfo.HitEntity as IMyCubeGrid;
            var pos = hitInfo.Position;
            return grid?.FirstBlock(pos, pos + prediction * dir, pred);
        }

        internal static IMySlimBlock GetHitSubEntity(this IHitInfo hitInfo, double prediction, ref Vector3D dir)
        {
            return (hitInfo.HitEntity as MyCubeGrid)?.GetTargetedBlock(hitInfo.Position + (hitInfo.HitEntity as MyCubeGrid).GridSize * prediction * dir);
        }
    }
}

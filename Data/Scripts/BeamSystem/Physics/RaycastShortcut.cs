using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;


namespace BeamSystem.Physics
{
    static class RaycastShortcut
    {
        internal static bool Cast(
            IMySlimBlock castOwner,
            ref Vector3D castDir, Vector3D castFrom, Vector3D castTo,
            RaycastResult result, bool fastMode)
        {
            if (fastMode)
            {
                result.impact = MyAPIGateway.Physics.CastLongRay(castFrom, castTo, out result.hitInfo, any: false);

                if (result.impact && result.hitInfo.HitEntity is IMyCubeGrid)
                {
                    result.hitBlock = result.hitInfo.GetHitSubEntity(0.5, ref castDir, pred: block => block != castOwner);
                    result.impact = null != result.hitBlock;
                }
            }
            if (!result.impact)
            {
                MyAPIGateway.Physics.CastRay(castFrom, castTo, result.toList, CollisionLayers.CharacterCollisionLayer);
                result.hitBlock = null;
                result.impact = result.toList.Count > 0 && !((result.hitInfo = result.toList[0]).HitEntity is IMyCubeGrid);
                foreach (var info in result.toList)
                {
                    var subEntity = info.GetHitSubEntity(0.5, ref castDir, pred: block => block != castOwner);
                    if (null != subEntity)
                    {
                        result.impact = true;
                        result.hitInfo = info;
                        result.hitBlock = subEntity;
                        break;
                    }
                }
            }
            return result.impact;
        }

    }

    class RaycastResult
    {
        public readonly List<IHitInfo> toList = new List<IHitInfo>();

        public ulong beamSerial;
        public IHitInfo hitInfo;
        public bool impact;
        public IMySlimBlock hitBlock;

        public readonly List<Vector3I> hitCoordinates = new List<Vector3I>();
    }
}
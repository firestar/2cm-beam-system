using BeamSystem.Physics;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    partial class BeamEmitter
    {
        private ulong raycastingCurrent = 0ul;

        private readonly RaycastResult[] raycastResults =
        {
            new RaycastResult(),
            new RaycastResult(),
            new RaycastResult()
        };
        private int nextRaycastResult = 0;
        private RaycastResult raycastResult = null;
        private Vector3D castFrom;
        private Vector3D castTo;
        private Vector3D castDir;
        private Vector3D castUp;
        private Vector3D castLeft;
        private bool offsetRayCastingEnabled;
        private double castBeamLength;
        private ulong beamSerial = 1uL;

        private void _Raycasting(RaycastResult result, bool fastMode = false)
        {
            if (result.beamSerial == this.beamSerial)
            {
                result.impact = false;
                var caster = me.SlimBlock;
                bool useFastMode = fastMode && castBeamLength > 50.0;

                if (!RaycastShortcut.Cast(caster, ref castDir, castFrom, castTo, result, fastMode && useFastMode)
                    && offsetRayCastingEnabled
                    && (RaycastShortcut.Cast(caster, ref castDir, castFrom + castUp, castTo + castUp, result, fastMode && useFastMode) ||
                        RaycastShortcut.Cast(caster, ref castDir, castFrom - castUp, castTo - castUp, result, fastMode && useFastMode) ||
                        RaycastShortcut.Cast(caster, ref castDir, castFrom + castLeft, castTo + castLeft, result, fastMode && useFastMode) ||
                        RaycastShortcut.Cast(caster, ref castDir, castFrom - castLeft, castTo - castLeft, result, fastMode && useFastMode)))
                    ;// Logger.Write("BeamComponent_" + Entity.EntityId, "Offset Casting " + result.beamSerial);
                if (raycastResult == null || raycastResult.beamSerial <= result.beamSerial)
                    raycastResult = result;
            }
        }

        private ulong currentParallelCasting = 0ul;
        private void Raycasting(ulong beamSerial, bool allowParallel)
        {
            bool continueCasting = raycastingCurrent == beamSerial;
            if (continueCasting && currentParallelCasting == beamSerial) return;
            raycastingCurrent = beamSerial;

            CastPrepare();

            var result = raycastResults[nextRaycastResult];
            nextRaycastResult = (nextRaycastResult + 1) % raycastResults.Length;
            result.beamSerial = beamSerial;

            _Raycasting(result, fastMode: true);
        }

        private void CastPrepare()
        {
            castDir = EmitterDirection;
            var from = this.From;
            //from += castDir * -0.04 * gridSize;
            castFrom = from;
            castTo = castFrom + castDir * castBeamLength;
            var offset = vBeamTickness * MathHelper.Lerp(
                Settings.RaycastWidthMin, Settings.RaycastWidthMax,
                (SessionComponents.UpdateSessionComponent.Simulation10Count % 10ul) * 0.1f);
            offsetRayCastingEnabled = offset > Settings.OffsetRaycastThreshold;
            if (offsetRayCastingEnabled)
            {
                var mat = Entity.WorldMatrix;
                castUp = mat.Up * offset;
                castLeft = mat.Left * offset;
            }
        }
    }
}
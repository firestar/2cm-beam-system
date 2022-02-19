using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace BeamSystem.BlockComponents
{
    interface IBeamEmitterOwner
    {
        IMyEntity Entity { get; }
        IMySlimBlock SlimBlock { get; }
        IMySlimBlock ForwardBlock { get; }
        IMySlimBlock BackwardBlock { get; }
        double GridSize { get; }

        Vector3 LinearVelocity { get; }

        Vector3D From { get; }
        Vector3D EmitterDirection { get; }
        BeamComponent GetBeamComponent();

        double BeamStartOffset { get; }
        double VisualMuzzlePosition { get; }
        float GetVisualMuzzleDepth(float visualEmitRate);
    }
}
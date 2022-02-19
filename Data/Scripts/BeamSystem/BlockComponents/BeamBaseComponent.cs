using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;
using VRage.ModAPI;


namespace BeamSystem.BlockComponents
{
    using SessionComponents;


    abstract class BeamBaseComponent : StorageBaseComponent
    {
        internal static BeamComponent GetComponentOf(IMyEntity block) => block?.GameLogic.GetAs<BeamBaseComponent>()?.GetBeamComponent();
        internal static BeamComponent GetComponentOf(IMySlimBlock block) => GetComponentOf(block?.FatBlock);

        private ulong matUpdatedAt = 0ul;
        private Vector3D _emitterDirection;
        private MatrixD _oldMatrix = MatrixD.Identity;
        private Vector3D _from;
        public Vector3D From
        {
            get
            {
                if (matUpdatedAt != UpdateSessionComponent.SimulationCount)
                {
                    var worldMatrix = Entity.WorldMatrix;
                    if (!worldMatrix.EqualsFast(ref _oldMatrix))
                    {
                        _oldMatrix = worldMatrix;
                        _from = worldMatrix.Translation;
                        _emitterDirection = worldMatrix.Forward;
                    }
                    matUpdatedAt = UpdateSessionComponent.SimulationCount;
                }
                return _from;
            }
        }

        public Vector3D EmitterDirection
        {
            get
            {
                if (matUpdatedAt != UpdateSessionComponent.SimulationCount)
                {
                    var worldMatrix = Entity.WorldMatrix;
                    if (!worldMatrix.EqualsFast(ref _oldMatrix))
                    {
                        _oldMatrix = worldMatrix;
                        _from = worldMatrix.Translation;
                        _emitterDirection = worldMatrix.Forward;
                    }
                    matUpdatedAt = UpdateSessionComponent.SimulationCount;
                }
                return _emitterDirection;
            }
        }

        public abstract BeamComponent GetBeamComponent();
    }
}

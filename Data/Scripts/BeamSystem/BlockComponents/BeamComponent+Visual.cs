using VRageMath;


namespace BeamSystem.BlockComponents
{
    using Beams;
    using SessionComponents;


    abstract partial class BeamComponent
    {
        protected double emitVisual;
        private float vBeamTickness;
        private Vector3D vBeamTo;
        private Vector3D vBeamFrom;
        private Vector4 vBeamColor2, vBeamColor3, vBeamColor1;
        private Color emissionColor = Colors.Black;
        private float emissivity;
        private double vBeamLength;

        public override void UpdateAfterSimulation()
        {
            if (IsDedicated)
            {
                if (!NeedsKeepUpdate) CheckNeedsUpdate();
            }
            else
            {
                if (NeedsKeepUpdate)
                    UpdateEmitterAfterSimulation();
                else
                    CheckNeedsUpdate();
                UpdateEmission();
            }
        }

        protected virtual void UpdateEmitterAfterSimulation()
        {
            emitter.UpdateAfterSimulation();
        }

        public override void UpdateAfterSimulation10()
        {
            if (NeedsKeepLazyUpdate)
                LazyUpdate(UpdateSessionComponent.LazyUpdateFrameDuration);
            else
                CheckNeedsUpdate();
        }

        protected virtual void UpdateEmission()
        {
            Color emissionColor;
            float emissivity;
            Beam.GetEmissiveColor(Power, out emissionColor, out emissivity);
            Entity.SetEmissiveParts(Settings.EmissivePartName, emissionColor, emissivity);
        }

        public override void UpdateBeforeSimulation100()
        {
            emitter.UpdateBeforeSimulation100();
        }
    }
}

namespace BeamSystem.BlockComponents
{
    using Beams;
    using Sandbox.Common.ObjectBuilders;
    using VRage.Game.Components;


    [MyEntityComponentDescriptor(
        entityBuilderType: typeof(MyObjectBuilder_Passage),
        useEntityUpdate: true,
        entityBuilderSubTypeNames: new string[] { "2CM_BS_Ion_Range_Barrel_Large", "2CM_BS_Ion_Range_Barrel_Small" })]
    class IonLongRangeBarriel : IonBeamCombiner
    {
        protected override Beam Beam => Beam.LongRangeIonBeam;

        public override double BeamStartOffset => base.BeamStartOffset - 8.25;

        public override double VisualMuzzlePosition => base.VisualMuzzlePosition - 7.25;

        public override float GetVisualMuzzleDepth(float visualEmitRate)
            => base.GetVisualMuzzleDepth(visualEmitRate) - 7.25f;
    }
}
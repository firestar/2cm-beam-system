namespace BeamSystem.BlockComponents
{
    using Beams;
    using Sandbox.Common.ObjectBuilders;
    using VRage.Game.Components;


    [MyEntityComponentDescriptor(
        entityBuilderType: typeof(MyObjectBuilder_UpgradeModule),
        useEntityUpdate: true,
        entityBuilderSubTypeNames: new string[] { "2CM_BS_Laser_Splitter_Large", "2CM_BS_Laser_Splitter_Small" })]
    class LaserBeamSplitter : BeamSplitter
    {
        protected override Beam Beam => Beam.Laser;
    }

}
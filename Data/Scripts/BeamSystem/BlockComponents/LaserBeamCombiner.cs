using Sandbox.Common.ObjectBuilders;
using VRage.Game.Components;


namespace BeamSystem.BlockComponents
{
    using Beams;


    [MyEntityComponentDescriptor(
        entityBuilderType: typeof(MyObjectBuilder_Passage),
        useEntityUpdate: true,
        entityBuilderSubTypeNames: new string[] { "2CM_BS_Laser_Combiner_Large", "2CM_BS_Laser_Combiner_Small" })]
    class LaserBeamCombiner : BeamCombiner
    {
        protected override Beam Beam => Beam.Laser;
    }
}

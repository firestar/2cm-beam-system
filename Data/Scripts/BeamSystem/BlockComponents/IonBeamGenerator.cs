using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using VRage.Game.Components;


namespace BeamSystem.BlockComponents
{
    using Beams;


    [MyEntityComponentDescriptor(
        entityBuilderType: typeof(MyObjectBuilder_UpgradeModule),
        useEntityUpdate: true,
        entityBuilderSubTypeNames: new string[] { "2CM_BS_Ion_Generator_Large", "2CM_BS_Ion_Generator_sv_Large", "2CM_BS_Ion_Generator_sv_Small" })]
    class IonBeamGenerator : BeamGenerator
    {
        private static MySoundPair generatorSound = null;

        protected override Beam Beam => Beam.Ion;

        protected override MySoundPair GeneratorSound
        {
            get
            {
                if (null == generatorSound)
                    generatorSound = new MySoundPair(Settings.IonBeamGeneratorSound);
                return generatorSound;
            }
        }
    }
}

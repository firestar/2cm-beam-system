using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using VRage.Game.Components;


namespace BeamSystem.BlockComponents
{
    using Beams;


    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_UpgradeModule), true,
        "2CM_BS_Laser_Generator_Large",
        "2CM_BS_Laser_Generator_Small")]
    class LaserBeamGenerator : BeamGenerator
    {
        private static MySoundPair generatorSound = null;

        protected override Beam Beam => Beam.Laser;


        protected override MySoundPair GeneratorSound
        {
            get
            {
                if (null == generatorSound)
                    generatorSound = new MySoundPair(Settings.LaserBeamGeneratorSound);
                return generatorSound;
            }
        }
    }
}

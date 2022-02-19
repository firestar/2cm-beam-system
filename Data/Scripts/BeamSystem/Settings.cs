using VRage.Game;
using VRage.Utils;


namespace BeamSystem
{
    static partial class Settings
    {
        internal static readonly MyStringHash ResourceSinkGroup = MyStringHash.GetOrCompute("BeamGenerator");
        internal static readonly MyStringHash ResourceSourceGroup = MyStringHash.GetOrCompute("SolarPanels");

        internal const float BeamSoundDopplerScale = 2f;

        internal const double MinimumPower = 0.001;

        internal const string EmissivePowerPartName = "Emissive";
        internal const string EmissivePartName = "Emissive0";
        internal static readonly string[] EmissivePartNames = { "Emissive0", "Emissive1", "Emissive2", "Emissive3", "Emissive4" };
        internal const string EmissivePartForward = "Emissive1";
        internal const string EmissivePartBackward = "Emissive2";
        internal const float EmissionMultiplier = 10f;

        internal const string FxSparkImpactName = MyParticleEffectsNameEnum.WelderContactPoint;
        internal const string FxExplosionName = "2CM_ColorableExplosion";
        // internal const int FxSparkImpact = (int)MyParticleEffectsIDEnum.WelderSecondary;

        internal const float FxImpactBirthRate = 2.5f;
        internal const float FxImpactScale = 5f;
        internal const int FxImpactMaxCount = 25;

        internal const double LaserGeneratorPower = 8.0;
        internal const double LaserGeneratorCharge = 8.0;
        internal const double LaserCapacity = 500.0;
        internal const double LaserCapacityExtended = LaserCapacity * 10.0;
        internal const float LaserEmitLimitFInv = (float)(1.0 / LaserCapacity);
        internal const float LaserDamageModifier = 100.0f;
        internal const float LaserDamageDirectScale = 0.1f;
        internal const double LaserVoxelVaporizeRate = 5.0;
        internal const double LaserVoxelVaporizeRadiusRate = 0.5;

        internal const float LaserElectricPower = 25f; // 125f; // 2e7f;
        internal const double LaserEffeciency = 0.9; // 0.735091890625;
        internal const double LaserMinPower = 0.1;
        internal const float LaserPhysicsForce = 0.2f;

        internal const double LaserMaximumLinkRange = double.MaxValue;// 25.0;
        internal const float LaserImpactRadiusMultiplier = 0.5f;


        internal const double IonGeneratorPower = 2.5;
        internal const double IonGeneratorCharge = 0.25;
        internal const double IonCapacity = 1000.0;
        internal const double IonMaximumCapacity = 5000.0;
        internal const double IonExtraCapacity = (IonMaximumCapacity - IonCapacity) * 0.375;  //* 0.25;
        internal const double IonEmitLimit = 500.0;
        internal const double IonEmitMin = 10.0;
        internal const float IonDamageModifier = 100.0f; // 200f
        internal const float IonDamageModifierForShields = 3f;
        internal const float IonBeamTickness = 32.75f;
        internal const double IonVoxelVaporizeRate = 5.0;
        internal const double IonVoxelVaporizeRadiusRate = 0.5;

        internal const float IonElectricPower = 125f; // 2500f;// 2e7f;
        internal const double IonEffeciency = 0.99;
        internal const double IonMaxWaste = 50.0;
        internal const double IonReceiveEffeciency = 0.997;
        internal const double IonMinPower = 0.1;
        internal const float IonPhysicsForce = 0.0000023f;

        internal const double IonBeamMinRange = 1200.0;
        internal const double IonBeamMaxRange = 25000.0;

        internal const double IonMaximumLinkRange = double.MaxValue;// 125.0;
        internal const float IonImpactRadiusMultiplier = 1f; // 0.15f;

        internal const float IonElectricityConvertMinimumRequiredFlowRate = 0.675f;
        internal const float IonAntiGravityImpellerMinimumRequiredFlowRate = 0.74f;


        internal const float BeamVisualTicknessMin = 0.1f;
        internal const double BeamLightDistance = 1000D;
        internal const double BeamLightDistanceSquared = BeamLightDistance * BeamLightDistance;
        internal const float BeamLightdistanceFInv = (float)(1.0 / BeamLightDistance);
        internal const int UpdateTick = 10;


        internal const float SIM_FRAME_DURATION = 1f / 60f;

        internal const double ION_CAPACITY_SOUND_MIN_RANGE = 100.0;
        internal const double ION_CAPACITY_SOUND_MAX_RANGE = 150.0;

        internal const double GENERATOR_SOUND_MIN_RANGE = 20;
        internal const double GENERATOR_SOUND_MAX_RANGE = 25;

        internal const string LaserBeamGeneratorSound = "2cmLaserGenerator";
        internal const string IonBeamGeneratorSound = "2cmIonGenerator";

        internal const double VisualBeamEmitLerpSpeed = 0.12;
        internal const double VisualBeamLengthLerpSpeed = 0.33;

        internal const float RaycastWidthMin = 0.015f;
        internal const float RaycastWidthMax = 0.03f;
        internal const float OffsetRaycastThreshold = 0.175f;

        internal const double MinimumEmitVisual = 0.01;
        internal const float PlayerDamageMultiplier = 0.001f;
        internal const float VoxelCutoffScale = 2f;

        internal const float SolarPanelMaxLaserReceiveRate = 1000f;

        internal const ulong IgnoreStartFrames = 10ul;


        // anti gravity impeller
        internal const float AntiGravityImpellarMaxPower = 600000000f; // 100 x hydrogen
        internal const float AntiGravityImpellarForceScale = 5f;
        internal const float AntiGravityMEpsilon = -0.1f;
        internal const float INV_EARTH_GRAVITY = 1f / 9.8f;
    }
}

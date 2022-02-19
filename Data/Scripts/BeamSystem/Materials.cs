using VRage.Utils;


namespace BeamSystem
{
    static class Materials
    {
        internal static readonly MyStringId Laser = MyStringId.GetOrCompute("WeaponLaser");
        internal static readonly MyStringId Beam = MyStringId.GetOrCompute("2cm_WeaponBeam");
        internal static readonly MyStringId BeamStart = MyStringId.GetOrCompute("2cm_WeaponBeam_Start");
        internal static readonly MyStringId BeamGlare = MyStringId.GetOrCompute("2cm_WeaponBeamGlare");
        internal static readonly MyStringId BeamGlareStart = MyStringId.GetOrCompute("2cm_WeaponBeamGlare_Start");
        internal static readonly MyStringId BeamMuzzle = MyStringId.GetOrCompute("2cm_WeaponBeamMuzzle");
        internal static readonly MyStringId BeamMuzzleFlare = MyStringId.GetOrCompute("2cm_WeaponBeamMuzzleFlare");
        internal static readonly MyStringId BeamMuzzleGlare = MyStringId.GetOrCompute("2cm_WeaponBeamMuzzleGlare");
        // internal static readonly MyStringId Beam3D = MyStringId.GetOrCompute("2cm_WeaponBeam3D");
        internal static readonly MyStringId Beam3D = MyStringId.GetOrCompute("Square");
        internal static readonly MyStringId Circle = MyStringId.GetOrCompute("SunFlareCircle");
    }
}

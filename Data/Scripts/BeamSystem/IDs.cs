using System;

namespace BeamSystem
{
    static class IDs
    {
        // internal static readonly Guid Power = new Guid("dc7b3fd7-8498-4a4c-8add-db56efd79c36");
        // internal static readonly Guid CPower = new Guid("a19c3f24-37eb-42ce-8f79-59d54a0b6ea2");
        // internal static readonly Guid OPower = new Guid("e6f361c3-77f1-4ae5-8e28-13661cefce21");
        internal static readonly Guid Storage = new Guid("328a8014-1cf5-41a6-9412-9ca789410f2d");
    }

    static class Keys
    {
        internal static readonly int EntityId = "EntityId".GetHashCode();
        internal static readonly int Power = "Power".GetHashCode();
        internal static readonly int CurrentPower = "CurrentPower".GetHashCode();
        internal static readonly int OutputRate = "OutputRate".GetHashCode();
        internal static readonly int MaxCapacity = "MaxCapacity".GetHashCode();
        internal static readonly int Capacitor = "Capacitor".GetHashCode();
        internal static readonly int Capacitor0 = Capacitor;
        internal static readonly int Capacitor1 = Capacitor + 1;
        internal static readonly int Capacitor2 = Capacitor + 2;
        internal static readonly int Capacitor3 = Capacitor + 3;
        internal static readonly int Capacitor4 = Capacitor + 4;
        internal static readonly int Capacitor5 = Capacitor + 5;
        internal static readonly int AdditionalEmitPower = "AdditionalEmitPower".GetHashCode();
        internal static readonly int PresentCustonData = "PresentCustonData".GetHashCode();
        internal static readonly int Emitting = "Emitting".GetHashCode();
        internal static readonly int EmitCount = "EmitCount".GetHashCode();
        internal static readonly int SplitRatio = "SplitRatio".GetHashCode();
        internal static readonly int ReceiveFromGenerator = "ReceiveFromGenerator".GetHashCode();
    }
}

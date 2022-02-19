namespace BeamSystem.Network
{
    partial class NetworkMessage
    {
        internal float OutputRate
        {
            get { return GetSingle(Key.OutputRate); }
            set { Set(Key.OutputRate, value); }
        }
    }
}
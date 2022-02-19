namespace BeamSystem.Network
{
    partial class NetworkMessage
    {
        internal double SplitRatio
        {
            get { return GetDouble(Key.SplitRatio); }
            set { Set(Key.SplitRatio, value); }
        }
    }
}
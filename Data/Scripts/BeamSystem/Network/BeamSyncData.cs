namespace BeamSystem.Network
{
    partial class NetworkMessage
    {
        internal long EntityID
        {
            get { return GetInt64(Key.EntityID); }
            set { Set(Key.EntityID, value); }
        }

        internal double Power
        {
            get { return GetDouble(Key.Power); }
            set { Set(Key.Power, value); }
        }
    }
}
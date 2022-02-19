namespace BeamSystem.Network
{
    partial class NetworkMessage
    {
        internal int EmitCount
        {
            get { return GetInt32(Key.EmitCount); }
            set { Set(Key.EmitCount, value); }
        }

        internal bool Emitting
        {
            get { return GetBool(Key.Emitting); }
            set { Set(Key.Emitting, value); }
        }
    }
}
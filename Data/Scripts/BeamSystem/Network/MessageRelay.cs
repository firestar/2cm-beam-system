using Sandbox.ModAPI;


namespace BeamSystem.Network
{
    internal delegate void OnReceiveMessageDelegate(NetworkMessage message);
    internal delegate void OnReceivePropertyMessageDelegate(long timestamp, byte property, double value);

    class MessageRelay
    {
        const int SIZE_OF_PROPERTY_MESSAGE = sizeof(byte) + sizeof(long) + sizeof(double);

        private const ushort SyncMessageID = 41731;
        private const ushort ValueMessageID = SyncMessageID + 1;
        private static MessageRelay instance = null;

        internal static void RegisterMessageHandler(OnReceiveMessageDelegate handler)
            => instance.OnReceiveMessage += handler;

        internal static void UnregisterMessageHandler(OnReceiveMessageDelegate handler)
            => instance.OnReceiveMessage -= handler;

        internal static void RegisterMessageHandler(OnReceivePropertyMessageDelegate handler)
            => instance.OnReceivePropertyMessage += handler;

        internal static void UnregisterMessageHandler(OnReceivePropertyMessageDelegate handler)
            => instance.OnReceivePropertyMessage -= handler;

        internal static void Init()
        {
            instance = new MessageRelay();
        }

        internal static void Dispose()
        {
            instance?._Dispose();
            instance = null;
        }

        internal static void SendMessage(NetworkMessage message)
        {
            byte[] data = message.Encode();
            MyAPIGateway.Multiplayer.SendMessageToOthers(SyncMessageID, data);
        }

        internal static void SendMessageToServer(NetworkMessage message)
        {
            byte[] data = message.Encode();
            MyAPIGateway.Multiplayer.SendMessageToServer(SyncMessageID, data);
        }

        internal static void SendMessage(long timestamp, byte property, double value)
        {
            int index = 0;
            byte[] data = new byte[SIZE_OF_PROPERTY_MESSAGE];
            // property
            data[index++] = property;
            // value
            byte[] v = System.BitConverter.GetBytes(value);
            for (var i = 0; i < v.Length; i++)
                data[index++] = v[i];
            // timestamp
            v = System.BitConverter.GetBytes(timestamp);
            for (var i = 0; i < v.Length; i++)
                data[index++] = v[i];
            MyAPIGateway.Multiplayer.SendMessageToServer(ValueMessageID, data);
        }

        event OnReceiveMessageDelegate OnReceiveMessage;
        event OnReceivePropertyMessageDelegate OnReceivePropertyMessage;
        NetworkMessage message = new NetworkMessage();

        private MessageRelay()
        {
            MyAPIGateway.Multiplayer.RegisterMessageHandler(SyncMessageID, ReceiveMessage);
        }

        private void _Dispose()
        {
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(SyncMessageID, ReceiveMessage);
        }

        private void ReceiveMessage(byte[] data)
        {
            try
            {
                message.Decode(data);
            }
            catch { }
            OnReceiveMessage?.Invoke(message);
        }

        private void ReceivePropertyMessage(byte[] data)
        {
            byte property = data[0];
            double value = System.BitConverter.ToDouble(data, 1);
            long timestamp = System.BitConverter.ToInt64(data, 1 + sizeof(double));
            OnReceivePropertyMessage?.Invoke(timestamp, property, value);
        }

    }
}
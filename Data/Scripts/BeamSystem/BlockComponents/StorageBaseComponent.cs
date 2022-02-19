using BeamSystem.Network;
using Sandbox.Game.EntityComponents;
using VRage.Game.ObjectBuilders.ComponentSystem;
using VRage.ObjectBuilders;


namespace BeamSystem.BlockComponents
{
    abstract class StorageBaseComponent : BaseComponent
    {
        protected virtual bool UseNetworkSync => false;

        private DataStorage dataStorage;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            if (Entity.Storage == null)
            {
                Entity.Storage = new MyModStorageComponent();
                Entity.Storage[IDs.Storage] = string.Empty;
            }
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();

            dataStorage = Entity.Storage.GetStorage();
            Load(dataStorage);

            if (UseNetworkSync)
                MessageRelay.RegisterMessageHandler(ProcessNetworkMessage);
        }

        public override void OnRemovedFromScene()
        {
            MessageRelay.UnregisterMessageHandler(ProcessNetworkMessage);
            base.OnRemovedFromScene();
        }

        public override bool IsSerialized() => true;

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            dataStorage.Clear();
            Save(dataStorage);
            Entity.Storage.SetStorage(dataStorage);
            return base.Serialize(copy);
        }

        protected abstract void Load(DataStorage storage);

        protected abstract void Save(DataStorage storage);

        #region Network

        private readonly NetworkMessage networkMessage = new NetworkMessage();
        private long latestSyncedAt = 0L;
        

        private void ProcessNetworkMessage(NetworkMessage message)
        {
            if (message.EntityID == Entity.EntityId)
                if (IsServer)
                {
                    OnReceiveNetworkMessage(networkMessage);
                }
                else if (latestSyncedAt < message.Timestamp)
                {
                    latestSyncedAt = message.Timestamp;
                    OnReceiveNetworkMessage(networkMessage);
                }
        }

        protected virtual void OnReceiveNetworkMessage(NetworkMessage data)
        {
        }

        protected virtual void OnSendNetworkMessage(NetworkMessage data)
        {
            latestSyncedAt = Timestamp;
            data.Timestamp = latestSyncedAt;
            data.EntityID = Entity.EntityId;
        }

        protected void SnedDataToServer()
        {
            if (IsClient)
            {
                latestSyncedAt = Timestamp;
                networkMessage.Clear();
                OnSendNetworkMessage(networkMessage);
                MessageRelay.SendMessageToServer(networkMessage);
            }
        }

        protected void SendDataToClients()
        {
            if (IsServer)
            {
                networkMessage.Clear();
                OnSendNetworkMessage(networkMessage);
                MessageRelay.SendMessage(networkMessage);
            }
        }
        
        #endregion
    }
}
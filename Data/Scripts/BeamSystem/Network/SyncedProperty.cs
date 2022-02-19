using Sandbox.ModAPI;
using VRage.Game.Entity;


namespace BeamSystem.Network
{
    class SyncedProperty<T>
    {
        readonly MyEntity owner;
        long timestamp;
        private T value;

        public SyncedProperty(MyEntity owner)
        {
            this.owner = owner;
        }

        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                timestamp = MyAPIGateway.Session.GameDateTime.Ticks;
            }
        }

        public void SetValueWithoutSync(T value) => this.value = value;
    }
}
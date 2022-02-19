namespace BeamSystem.BlockComponents
{
    using Network;


    abstract partial class BeamComponent
    {
        protected override bool UseNetworkSync => true;

        protected override void OnReceiveNetworkMessage(NetworkMessage data)
        {
            base.OnReceiveNetworkMessage(data);
            // if (IsClient) Power = data.Power;
            CheckNeedsUpdate();
        }

        protected override void OnSendNetworkMessage(NetworkMessage data)
        {
            base.OnSendNetworkMessage(data);
            // data.Power = Power;
        }
    }
}

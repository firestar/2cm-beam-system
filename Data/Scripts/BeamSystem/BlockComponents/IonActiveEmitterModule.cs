using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    using Beams;
    using BeamSystem.Network;

    [MyEntityComponentDescriptor(
        entityBuilderType: typeof(MyObjectBuilder_UpgradeModule),
        useEntityUpdate: true,
        entityBuilderSubTypeNames: new string[] { "2CM_BS_Ion_Active_Emitter_Large", "2CM_BS_Ion_Active_Emitter_Small" })]
    class IonActiveEmitterModule : IonModuleForCombiner, IBeamEmitterOwner
    {
        private IMyFunctionalBlock functionalBlock;
        protected override IMySlimBlock AttachedBlock => BackwardBlock;

        protected override string AttachIndicatorEmissivePartName => Settings.EmissivePowerPartName;

        public override BeamComponent GetBeamComponent() => linkedCombiner;
        private IonBeamCombiner attachedCombiner = null;

        public double BeamStartOffset => linkedCombiner.BeamStartOffset;
        public double VisualMuzzlePosition => linkedCombiner.VisualMuzzlePosition - 2.5;

        public float GetVisualMuzzleDepth(float visualEmitRate)
            => linkedCombiner.GetVisualMuzzleDepth(visualEmitRate) - 2.5f;

        private double splitRatio = 1.0;
        internal double SplitRatio => IsWorking ? splitRatio : 0.0;
        public void SetSplitRatio(float ratio)
        {
            this.splitRatio = ratio;
            linkedCombiner?.NotifyMultipleEmitChanged();
            SnedDataToServer();
        }
        public float GetSplitRatio() => (float)splitRatio;

        private BeamEmitter emitter;

        public bool IsWorking => functionalBlock.IsWorking;
        public string CustonName => functionalBlock.CustomName;

        public bool IsAttached => null != linkedCombiner;
        public float CombinerSplitRatio => null == linkedCombiner ? 0f : (float)linkedCombiner.SplitRatio;
        public void SetCombinerSplitRatio(float value) => linkedCombiner?.SetSplitRatio(value);

        public float CurrentOutputPower => (float)emitter.EmitPower;

        private bool needInitialize = true;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            functionalBlock = CubeBlock as IMyFunctionalBlock;
            emitter = new BeamEmitter(this, Beam.Ion);
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            TerminalControls.IonActiveEmitterTerminalControl.Initialize();
        }

        public override void OnRemovedFromScene()
        {
            functionalBlock.IsWorkingChanged -= OnWorkingChanged;
            if (null != attachedCombiner)
            {
                attachedCombiner.InfoChanged -= OnInfoChanged;
                attachedCombiner = null;
            }
            emitter.OnRemovedFromScene();
            base.OnRemovedFromScene();
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (needInitialize)
            {
                needInitialize = false;
                functionalBlock.IsWorkingChanged += OnWorkingChanged;
            }
            UpdateEmission();
        }

        protected override void Load(DataStorage storage)
        {
            splitRatio = storage.GetDouble(Keys.SplitRatio, 1.0);
        }

        protected override void Save(DataStorage storage)
        {
            emitter.OnBeforeSave();
            storage.Set(Keys.SplitRatio, splitRatio);
        }

        protected override void OnAdd(IonBeamCombiner to)
        {
            if (null != attachedCombiner)
                attachedCombiner.InfoChanged -= OnInfoChanged;
            emitter.OnRemovedFromScene();
            attachedCombiner = to;
            attachedCombiner.InfoChanged += OnInfoChanged;
            attachedCombiner.Add(this);
        }

        private void OnInfoChanged(IonBeamCombiner obj)
        {
            if (IsWorking) functionalBlock.RefreshCustomInfo();
        }

        protected override void OnRemove(IonBeamCombiner from)
        {
            if (null != attachedCombiner)
            {
                attachedCombiner.InfoChanged -= OnInfoChanged;
                attachedCombiner = null;
            }
            emitter.OnRemovedFromScene();
            from.Remove(this);
        }

        protected override void OnAppendCustonInfo(StringBuilder sb)
        {
            if (IsFunctional && null != attachedCombiner)
                attachedCombiner.GetInfoText(this, sb);
            else
                base.OnAppendCustonInfo(sb);
        }

        protected override Color GetAttachedIndicatorColor() => IsWorking ? Colors.White : Colors.Yellow;

        private void OnWorkingChanged(IMyCubeBlock obj)
        {
            Update();
            attachedCombiner?.NotifyMultipleEmitChanged();
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            emitter.UpdateBeforeSimulation100();
        }


        public void DoEmit(double elapsedSeconds, double realEmitPower, double additionalEmitPower, ulong receiveFromGenerator)
        {
            emitter.DoEmit(elapsedSeconds, realEmitPower, additionalEmitPower, receiveFromGenerator);
            UpdateEmission();
            if (emitter.EmitPower > 0.0)
                NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        private const float IonEmitLimitInvF = (float)(1.0 / Settings.IonEmitLimit);
        private void UpdateEmission()
        {
            var emitRate = (float)emitter.EmitPower * IonEmitLimitInvF;
            CubeBlock.SetEmissiveParts(
                emissiveName: Settings.EmissivePartName,
                emissivePartColor: Color.Lerp(Colors.Black, Colors.White, emitRate),
                emissivity: emitRate * EmissionMultiplier);
        }

        public override void UpdateAfterSimulation()
        {
            if (null == attachedCombiner)
                emitter.DoEmit(0.0, 0.0, 0.0, 0ul);
            else
                emitter.UpdateAfterSimulation();
            if (emitter.EmitPower == 0.0)
                NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
        }


        #region Network

        protected override bool UseNetworkSync => true;

        protected override void OnReceiveNetworkMessage(NetworkMessage data)
        {
            base.OnReceiveNetworkMessage(data);
            if (splitRatio != data.SplitRatio)
            {
                splitRatio = data.SplitRatio;
                linkedCombiner?.NotifyMultipleEmitChanged();
            }
        }

        protected override void OnSendNetworkMessage(NetworkMessage data)
        {
            base.OnSendNetworkMessage(data);
            data.SplitRatio = splitRatio;
        }

        #endregion
    }
}
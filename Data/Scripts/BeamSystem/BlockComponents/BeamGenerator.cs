using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    using Beams;
    using GridComponents;
    using Network;


    abstract class BeamGenerator : BeamComponent
    {
        internal IMyUpgradeModule Block { get; private set; }

        public override bool IsBeamReceivable(Beam beam) => false;

        internal bool IsWorking => Block.IsWorking;

        protected override double EmitRate => Power;
        protected override bool NeedsKeepLazyUpdate
        {
            get
            {
                return IsWorking || currentPower > 0D || base.NeedsKeepLazyUpdate;
            }
        }

        protected abstract MySoundPair GeneratorSound { get; }

        internal float CurrentPower => (float)currentPower;

        private readonly System.Text.StringBuilder customInfo = new System.Text.StringBuilder();
        double currentPower = 0D;
        double targetPower = 0D;
        double powerDelta = 0f;
        double defPowerMultiplier = 1.0;
        MyFixedPoint requiredPower = MyFixedPoint.Zero;
        internal MyFixedPoint consumePower => IsWorking ? requiredPower : MyFixedPoint.Zero;
        private MyEntity3DSoundEmitter soundEmitter;

        double GeneratorMaxCharging => Beam.GeneratorMaxCharging * defPowerMultiplier;
        double GeneratorChargingSpeed => Beam.GeneratorChargingSpeed * defPowerMultiplier;
        double RequiredElectricPower => Beam.RequiredElectricPower * defPowerMultiplier;

        /// <summary>
        /// 0...1
        /// </summary>
        internal float OutputRate { get; private set; }

        internal double OutputPower => (double)requiredPower;

        private void UpdatePowerIndicator(float powerRatio)
        {
            customInfo.Clear();
            if (consumePower == MyFixedPoint.Zero)
            {
                Entity.SetEmissiveParts(Settings.EmissivePowerPartName, Color.Black, 0f);
                customInfo.Append("Power Consume: ");
                MyValueFormatter.AppendWorkInBestUnit(0f, customInfo);
            }
            else if (powerRatio >= 1f)
            {
                Entity.SetEmissiveParts(Settings.EmissivePowerPartName, Color.White, 2f);
                customInfo.Append("Power Consume: ");
                MyValueFormatter.AppendWorkInBestUnit((float)consumePower, customInfo);
            }
            else if (powerRatio > 0f)
            {
                Entity.SetEmissiveParts(Settings.EmissivePowerPartName, Color.OrangeRed, 2f);
                customInfo.Append("Required Power: ");
                MyValueFormatter.AppendWorkInBestUnit((float)consumePower, customInfo);
                customInfo.AppendLine();
                customInfo.Append("Power Consume: ");
                MyValueFormatter.AppendWorkInBestUnit((float)consumePower * powerRatio, customInfo);
                customInfo.AppendLine();
                customInfo.Append("Current operating rate: ").AppendFormat("{0:P}", powerRatio).AppendLine();
                customInfo.AppendLine("Warning: There is not enough power.");
            }
            else
            {
                customInfo.Append("Warning: There is not enough power.");
                Entity.SetEmissiveParts(Settings.EmissivePowerPartName, Color.Red, 2f);
            }
            Block.RefreshCustomInfo();
        }
        private void UpdateCustomInfo(GridResourceSink sink) => UpdatePowerIndicator(sink.PowerRatio);

        /// <summary>
        /// </summary>
        /// <param name="value">0...1</param>
        internal void SetOutputRate(float value)
        {
            OutputRate = value;
            targetPower = value * GeneratorMaxCharging;
            requiredPower = (MyFixedPoint)(OutputRate * RequiredElectricPower);
            UpdateCustomInfo(ResourceSink);
            SnedDataToServer();
        }

        protected GridResourceSink ResourceSink => GridResourceSink.GetResourceSinkOf(CubeGrid, this);


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            Block = Entity as IMyUpgradeModule;

            var def = SlimBlock.BlockDefinition as MyUpgradeModuleDefinition;
            defPowerMultiplier = null == def ? 1.0 : def.Upgrades[0].Modifier;

            var resourceSink = new MyResourceSinkComponent();
            resourceSink.Init(Settings.ResourceSinkGroup,
                maxRequiredInput: float.MaxValue,
                requiredInputFunc: () => ResourceSink?.GetRequiredInput(this) ?? 0f, parent: (MyCubeBlock)Entity);
            Block.ResourceSink = resourceSink;
            resourceSink.Update();

            Block.AppendingCustomInfo += AppendingCustomInfo;

            if (!IsDedicated)
            {
                soundEmitter = new MyEntity3DSoundEmitter(Entity as MyEntity, true);
                soundEmitter.CanPlayLoopSounds = true;
            }

            NeedsUpdate |= VRage.ModAPI.MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        private void AppendingCustomInfo(IMyTerminalBlock block, System.Text.StringBuilder sb) => sb.Append(customInfo);

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();

            ResourceSink.Add(this);

            Block.IsWorkingChanged += OnWorkingChanged;
            OnWorkingChanged(Block);
        }

        protected override void InitializeTerminalControl() => TerminalControls.BeamGeneratorTerminalControl.Initialize();

        public override void OnRemovedFromScene()
        {
            soundEmitter?.StopSound(forced: true, cleanUp: true);
            base.OnRemovedFromScene();
        }

        protected override void OnSendNetworkMessage(NetworkMessage data)
        {
            base.OnSendNetworkMessage(data);
            data.OutputRate = this.OutputRate;
        }

        protected override void OnReceiveNetworkMessage(NetworkMessage data)
        {
            SetOutputRate(data.OutputRate);
            base.OnReceiveNetworkMessage(data);
        }

        protected override void Load(DataStorage storage)
        {
            base.Load(storage);
            currentPower = storage.GetDouble(Keys.CurrentPower, 0.0);
            SetOutputRate(storage.GetSingle(Keys.OutputRate, 0f));
        }

        protected override void Save(DataStorage storage)
        {
            base.Save(storage);
            storage.Set(Keys.CurrentPower, currentPower);
            storage.Set(Keys.OutputRate, OutputRate);
        }

        private void OnWorkingChanged(IMyCubeBlock obj)
        {
            if (IsWorking)
            {
                powerDelta = GeneratorChargingSpeed;
            }
            else
            {
                powerDelta = -GeneratorChargingSpeed;
                Entity.SetEmissiveParts(Settings.EmissivePowerPartName, Color.Black, 0f);
                soundEmitter?.StopSound(false);
            }
            CheckNeedsUpdate();
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (!IsWorking)
                Entity.SetEmissiveParts(Settings.EmissivePowerPartName, Color.Black, 0f);
        }

        protected override void OnLazyUpdate(double elapsedSeconds)
        {
            receiveFromGenerator = SessionComponents.UpdateSessionComponent.Simulation10Count + 10ul;
            var sink = ResourceSink;
            if (!IsWorking || consumePower == MyFixedPoint.Zero)
            {
                Entity.SetEmissiveParts(Settings.EmissivePowerPartName, Color.Black, 0f);
                soundEmitter?.StopSound(false);
                UpdatePowerIndicator(1f);
                CheckNeedsUpdate();
            }
            else
            {
                var powerRatio = sink.PowerRatio;
                if (powerRatio > 0f)
                {
                    currentPower = MathHelper.Clamp(currentPower + powerDelta, min: 0.0, max: targetPower * powerRatio);
                    UpdateCustomInfo(sink);
                    if (null != soundEmitter)
                        soundEmitter.VolumeMultiplier = (float)(currentPower / GeneratorMaxCharging);
                    PlayActiveSound();
                    SetPower(currentPower);
                }
                else
                {
                    customInfo.Clear();
                    customInfo.Append("Warning: There is not enough power.");
                    Block.RefreshCustomInfo();
                    Entity.SetEmissiveParts(Settings.EmissivePowerPartName, Color.Red, 2f);
                    currentPower = 0.0;
                    SetPower(0.0);
                    soundEmitter?.StopSound(false);
                }
            }
            base.OnLazyUpdate(elapsedSeconds);
        }

        public override void UpdateBeforeSimulation100()
        {
            if (null != soundEmitter && soundEmitter.IsPlaying && Entity.GetDistanceSquaredFromCamera() < Settings.GENERATOR_SOUND_MAX_RANGE)
                soundEmitter.Update();
        }

        void PlayActiveSound()
        {
            if (null != soundEmitter && !soundEmitter.IsPlaying && Entity.GetDistanceSquaredFromCamera() < Settings.GENERATOR_SOUND_MIN_RANGE)
            {
                soundEmitter.PlaySingleSound(GeneratorSound, stopPrevious: true);
            }
        }

    }
}

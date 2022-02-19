using System;
using System.Collections.Generic;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    using Beams;
    using SessionComponents;


    abstract partial class BeamComponent : BeamBaseComponent, IBeamEmitterOwner
    {
        protected double Power;

        protected double FlowPower;
        protected abstract double EmitRate { get; }
        protected abstract Beam Beam { get; }
        protected virtual double MaxPowerCapacity { get { return double.MaxValue; } }
        protected virtual bool NeedsKeepUpdate { get { return (NeedsKeepLazyUpdate && EmitterHasDrawing) || emitVisual > Settings.MinimumEmitVisual; } }
        protected virtual bool NeedsKeepLazyUpdate { get { return Power > Settings.MinimumPower || emitPower > 0.0; } }
        protected virtual bool UseCustomColor => true;
        public double FlowPowerRate => FlowPower / Beam.Capacity;
        public abstract bool IsBeamReceivable(Beam beam);

        private static readonly HashSet<BeamComponent> travelMap = new HashSet<BeamComponent>();

        private ulong updatedAt;
        private double emitPower;
        internal readonly List<BeamComponent> LinkSenders = new List<BeamComponent>();
        private BeamEmitter emitter;

        private double gridSize;
        private ulong ignoreAdditionalEmitPower = 0ul;
        private double additionalEmitPower = 0.0;

        protected virtual bool EmitterHasDrawing => emitter.HasDrawing;

        public virtual double VisualMuzzlePosition => CubeBlock.LocalAABB.Size.Z * 0.5;

        public virtual float GetVisualMuzzleDepth(float visualEmitRate) => 1f;

        public virtual double BeamStartOffset => 0.0;

        protected ulong receiveFromGenerator = 1ul;


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            gridSize = CubeBlock.CubeGrid.GridSize;
            emitter = new BeamEmitter(this, Beam);
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            updatedAt = UpdateSessionComponent.Simulation10Count;
            CheckNeedsUpdate();
            if (!NeedsUpdate.HasFlag(MyEntityUpdateEnum.EACH_FRAME))
                NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            InitializeTerminalControl();
        }

        protected abstract void InitializeTerminalControl();

        public override void OnRemovedFromScene()
        {
            LinkSenders.Clear();
            emitter.OnRemovedFromScene();
            base.OnRemovedFromScene();
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            UpdateEmission();
            Entity.SetEmissiveParts(Settings.EmissivePartName, emissionColor, emissivity);
        }

        protected virtual void CheckNeedsUpdate()
        {
            if (NeedsKeepUpdate)
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
            else if (NeedsKeepLazyUpdate)
                NeedsUpdate = MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
            else
                NeedsUpdate = MyEntityUpdateEnum.NONE;
        }

        protected override void Load(DataStorage storage)
        {
            Power = storage.GetDouble(Keys.Power, 0.0);
            additionalEmitPower = MathHelperD.Clamp(storage.GetDouble(Keys.AdditionalEmitPower), min: 0.0, max: MaxPowerCapacity * 10.0);
            receiveFromGenerator = storage.GetULong(Keys.ReceiveFromGenerator);
        }

        protected override void Save(DataStorage storage)
        {
            emitter.OnBeforeSave();
            storage.Set(Keys.Power, Power);
            storage.Set(Keys.AdditionalEmitPower, additionalEmitPower);
            if (receiveFromGenerator > UpdateSessionComponent.Simulation10Count)
                storage.Set(Keys.ReceiveFromGenerator,
                    Settings.IgnoreStartFrames + receiveFromGenerator - UpdateSessionComponent.Simulation10Count);
        }

        protected void OnCapacityChanged()
        {
            ignoreAdditionalEmitPower = UpdateSessionComponent.Simulation10Count + 10ul;
            if (UpdateSessionComponent.DoUpdate && Power > MaxPowerCapacity)
                Power = MaxPowerCapacity;
        }

        public void ReceivePowerFrom(double power, double additionalPower, ulong fromGenerator, Vector3D enterDir, double linkDistance, bool isHardLink)
        {
            if (ignoreAdditionalEmitPower <= UpdateSessionComponent.Simulation10Count)
                power += additionalPower;
            var angle = enterDir.Dot(EmitterDirection);
            angle = (1.0 - angle) * 0.5;
            power = OnReceivePowerFrom(power, angle, linkDistance, isHardLink);
            AddPower(power, fromGenerator);
        }

        protected virtual double OnReceivePowerFrom(double power, double angle, double linkDistance, bool isHardLink)
            => Beam.ReceiveEffectivity(power, angle, linkDistance, isHardLink);

        internal virtual void AddPower(double power, ulong fromGenerator)
        {
            if (fromGenerator > receiveFromGenerator)
                receiveFromGenerator = fromGenerator;
            SetPower(Power + power);
        }

        protected virtual void SetPower(double power)
        {
            if (UpdateSessionComponent.DoUpdate)
            {
                additionalEmitPower += Math.Max(0.0, power - MaxPowerCapacity);
                Power = MathHelper.Clamp(power, min: 0.0, max: MaxPowerCapacity);
                if (FlowPower < Power) FlowPower = Power;
                CheckNeedsUpdate();
            }
        }
        private void LazyUpdate(HashSet<BeamComponent> travelMap, double elapsedSeconds)
        {
            if (NeedsKeepLazyUpdate)
            {
                {
                    var newAt = UpdateSessionComponent.Simulation10Count;
                    if (updatedAt == newAt) return;
                    updatedAt = newAt;
                }

                if (!travelMap.Contains(this))
                {
                    travelMap.Add(this);
                    for (var count = LinkSenders.Count - 1; count >= 0; --count)
                        LinkSenders[count].LazyUpdate(travelMap, elapsedSeconds);
                    try
                    {
                        OnLazyUpdate(elapsedSeconds);
                    }
                    catch { }
                }
            }
            else
            {
                DoEmit(elapsedSeconds, 0.0, 0.0);
                emitPower = 0.0;
                FlowPower = 0.0;
            }
        }

        public void OnLInkChanged()
        {
            ignoreAdditionalEmitPower = UpdateSessionComponent.Simulation10Count + 10ul;
        }

        protected virtual void DoEmit(double elapsedSeconds, double emitPower, double additionalEmitPower)
            => emitter.DoEmit(elapsedSeconds, emitPower, additionalEmitPower, receiveFromGenerator);

        protected virtual void OnLazyUpdate(double elapsedSeconds)
        {
            FlowPower = Power;
            emitPower = Math.Min(Power, EmitRate);
            DoEmit(elapsedSeconds, emitPower, additionalEmitPower);

            if (UpdateSessionComponent.DoUpdate)
            {
                Power -= emitPower;
                if (receiveFromGenerator < UpdateSessionComponent.Simulation10Count)
                    Power = Beam.Effeciency(Power, elapsedSeconds);
                else
                    Power = Beam.Effeciency(Power, elapsedSeconds * 0.1);
                additionalEmitPower = 0.0;
            }
            UpdateEmission();
            SendDataToClients();
        }

        private void LazyUpdate(double elapsedSeconds)
        {
            LazyUpdate(travelMap, elapsedSeconds);
            travelMap.Clear();
        }

        public override BeamComponent GetBeamComponent() => this;

        #region Utility

        void Log(object message)
            => Logger.Write("BC" + Entity.EntityId, message);

        #endregion

    }
}

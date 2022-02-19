using Sandbox.Common.ObjectBuilders;
using System.Collections.Generic;
using VRage.Game.Components;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    using Beams;
    using BeamSystem.Network;
    using System.Text;


    [MyEntityComponentDescriptor(
        entityBuilderType: typeof(MyObjectBuilder_Passage),
        useEntityUpdate: true,
        entityBuilderSubTypeNames: new string[] { "2CM_BS_Ion_Combiner_Large", "2CM_BS_Ion_Combiner_Small" })]
    partial class IonBeamCombiner : BeamCombiner
    {
        protected override Beam Beam => Beam.Ion;

        private readonly HashSet<IonCapacitor> capacitors = new HashSet<IonCapacitor>();
        private readonly HashSet<IonActiveEmitterModule> emitters = new HashSet<IonActiveEmitterModule>();
        private double capacity = Settings.IonCapacity;


        protected override double MaxPowerCapacity => capacity;

        internal double SplitRatio { get; private set; } = 1.0;

        internal void SetSplitRatio(double value)
        {
            SplitRatio = value;
            NotifyMultipleEmitChanged();
            SnedDataToServer();
        }

        public override double VisualMuzzlePosition => 4.5;
        public override float GetVisualMuzzleDepth(float visualEmitRate)
        {
            if (visualEmitRate <= 0.6f) return 0f;
            visualEmitRate -= 0.6f;
            visualEmitRate *= 2.5f;
            return visualEmitRate;
        }

        private void Recalulate()
        {
            capacity = Settings.IonCapacity;
            foreach (var cap in capacitors)
                capacity += cap?.ExtraCapacity ?? 0.0;
        }

        internal void Add(IonCapacitor capacitor)
        {
            if (!capacitors.Contains(capacitor))
            {
                capacitors.Add(capacitor);
                Recalulate();
                OnCapacityChanged();
            }
        }

        internal void Remove(IonCapacitor capacitor)
        {
            capacitors.Remove(capacitor);
            Recalulate();
            OnCapacityChanged();
        }

        internal void Add(IonActiveEmitterModule emitter)
        {
            emitters.Add(emitter);
            NotifyMultipleEmitChanged();
        }
        internal void Remove(IonActiveEmitterModule emitter)
        {
            emitters.Remove(emitter);
            if (emitters.Count == 0)
                SplitRatio = 1.0;
            NotifyMultipleEmitChanged();
        }

        internal event System.Action<IonBeamCombiner> InfoChanged;
        internal void NotifyMultipleEmitChanged() => InfoChanged?.Invoke(this);
        internal void GetInfoText(IonActiveEmitterModule emitterModule, StringBuilder sb)
        {
            double totalWeight = SplitRatio;
            foreach (var emitter in emitters)
                if (emitter.IsWorking)
                    totalWeight += emitter.SplitRatio;
            totalWeight = totalWeight == 0.0 ? 0.0 : 1.0 / totalWeight;

            double curSplitRatio = totalWeight == 0.0 ? 1.0 : SplitRatio;
            sb.Append("Ion Combiner: ")
                .AppendFormat("{0:0.00}", curSplitRatio);
            if (totalWeight > 0.0) curSplitRatio *= totalWeight;
            sb.Append(" (").AppendFormat("{0:P2}", curSplitRatio).AppendLine(")").AppendLine();

            if (emitterModule.IsWorking)
                sb.Append(emitterModule.CustonName).Append(": ")
                    .AppendFormat("{0:0.00}", emitterModule.SplitRatio)
                    .Append(" (").AppendFormat("{0:P2}", emitterModule.SplitRatio * totalWeight).AppendLine(")").AppendLine();
            else
                sb.Append(emitterModule.CustonName).AppendLine(": Deactive");

            foreach (var emitter in emitters)
                if (emitter != emitterModule)
                    if (emitter.IsWorking)
                        sb.Append(emitter.CustonName).Append(": ")
                            .AppendFormat("{0:0.00}", emitter.SplitRatio)
                            .Append(" (").AppendFormat("{0:P2}", emitter.SplitRatio * totalWeight).AppendLine(")");
                    else if (emitter.IsFunctional)
                        sb.Append(emitter.CustonName).AppendLine(": Deactive");
        }

        public override void Close()
        {
            capacitors.Clear();
            base.Close();
        }

        protected override void Load(DataStorage storage)
        {
            base.Load(storage);

            //caps = new HashSet<long>();
            //long capacitor;
            //for (int i = 0; i < 6; i++)
            //    if (storage.TryGetValue(Keys.Capacitor + i, out capacitor))
            //        caps.Add(capacitor);

            capacity = storage.GetDouble(Keys.MaxCapacity, Settings.IonCapacity);

            SplitRatio = storage.GetDouble(Keys.SplitRatio, 1.0);
        }

        //public override void UpdateOnceBeforeFrame()
        //{
        //    if (null != caps)
        //    {
        //        foreach (var id in caps)
        //        {
        //            var cap = IonCapacitor.GetComponentOf(id);
        //            if (null != cap)
        //            {
        //                capacitors.Add(cap);
        //            }
        //        }
        //        caps = null;
        //        Recalulate();
        //    }
        //    base.UpdateOnceBeforeFrame();
        //}

        protected override void Save(DataStorage storage)
        {
            base.Save(storage);

            storage.Set(Keys.MaxCapacity, capacity);

            //int index = 0;
            //foreach (var capacitor in capacitors)
            //{
            //    storage.Set(Keys.Capacitor + index, capacitor.Entity.EntityId);
            //    ++index;
            //}

            storage.Set(Keys.SplitRatio, SplitRatio);
        }

        protected override void DoEmit(double elapsedSeconds, double emitPower, double additionalEmitPower)
        {
            if (emitters.Count == 0)
                base.DoEmit(elapsedSeconds, emitPower, additionalEmitPower);
            else
            {
                double weightTotal = SplitRatio;
                foreach (var emitter in emitters)
                    weightTotal += emitter.SplitRatio;

                if (weightTotal == 0.0)
                {
                    base.DoEmit(elapsedSeconds, emitPower, additionalEmitPower);
                    foreach (var emitter in emitters)
                        emitter.DoEmit(elapsedSeconds, 0.0, 0.0, receiveFromGenerator);
                }
                else
                {
                    weightTotal = 1.0 / weightTotal;
                    double splitRatio;
                    foreach (var emitter in emitters)
                    {
                        splitRatio = emitter.SplitRatio * weightTotal;
                        emitter.DoEmit(elapsedSeconds, emitPower * splitRatio, additionalEmitPower * splitRatio, receiveFromGenerator);
                    }
                    splitRatio = this.SplitRatio * weightTotal;
                    base.DoEmit(elapsedSeconds, emitPower * splitRatio, additionalEmitPower * splitRatio);
                }
            }
        }


        internal float ChargedPowerRate { get; private set; }

        const float EmissionMultiplier = Settings.EmissionMultiplier * 2f;

        protected override void UpdateEmission()
        {
            base.UpdateEmission();

            double powerRate = FlowPower / MaxPowerCapacity;
            float percentOfCharge = ChargedPowerRate = (float)powerRate;
            UpdateSound(percentOfCharge);
            percentOfCharge *= 5f;
            foreach (var cap in capacitors)
                cap.UpdateVisual(percentOfCharge);

            for (int i = 0; i < 5; i++)
            {
                var emissivity = MathHelper.Clamp(percentOfCharge, 0f, 1f);
                CubeBlock.SetEmissiveParts(
                    emissiveName: Settings.EmissivePartNames[i],
                    emissivePartColor: Color.Lerp(Colors.Black, Colors.White, emissivity),
                    emissivity: (emissivity - 0.5f) * EmissionMultiplier);
                percentOfCharge -= 1f;
            }
        }

        protected override void OnReceiveNetworkMessage(NetworkMessage data)
        {
            base.OnReceiveNetworkMessage(data);
            if (SplitRatio != data.SplitRatio)
            {
                SplitRatio = data.SplitRatio;
                NotifyMultipleEmitChanged();
            }
        }

        protected override void OnSendNetworkMessage(NetworkMessage data)
        {
            base.OnSendNetworkMessage(data);
            data.SplitRatio = SplitRatio;
        }
    }
}

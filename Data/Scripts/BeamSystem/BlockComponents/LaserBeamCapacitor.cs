using Sandbox.Common.ObjectBuilders;
using VRage.Game.Components;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    using Beams;
    using BeamSystem.Network;
    using System.Collections.Generic;
    using VRage.Game.ModAPI;

    [MyEntityComponentDescriptor(
        entityBuilderType: typeof(MyObjectBuilder_Passage),
        useEntityUpdate: true,
        entityBuilderSubTypeNames: new string[] { "2CM_BS_Laser_Capacitor_Large", "2CM_BS_Laser_Capacitor_Small" })]
    class LaserBeamCapacitor : BeamCombiner
    {
        protected override Beam Beam => Beam.Laser;

        protected override double MaxPowerCapacity => Settings.LaserCapacityExtended;

        private bool emitting = false;
        private int emitCount = 3;
        private ulong sendToLinkedCapacitorAt = 0ul;

        protected override double EmitRate
        {
            get
            {
                var emitRate = base.EmitRate;
                if (emitting) return emitRate;
                else return emitRate < 0.1 ? emitRate : 0.1;
            }
        }

        private LaserBeamCapacitor _hardLinkedCapacitor = null;
        private LaserBeamCapacitor HardLinkedCapacitor
        {
            get
            {
                if (null == _hardLinkedCapacitor || !_hardLinkedCapacitor.InScene)
                {
                    _hardLinkedCapacitor = GetComponentOf(ForwardBlock) as LaserBeamCapacitor;
                }
                return _hardLinkedCapacitor;
            }
        }

        protected override void Load(DataStorage storage)
        {
            base.Load(storage);
            emitCount = storage.GetInt(Keys.EmitCount);
            emitting = storage.GetBool(Keys.Emitting);
        }

        protected override void Save(DataStorage storage)
        {
            base.Save(storage);
            storage.Set(Keys.EmitCount, emitCount);
            storage.Set(Keys.Emitting, emitting);
        }

        protected override void OnSendNetworkMessage(NetworkMessage data)
        {
            base.OnSendNetworkMessage(data);
            data.Emitting = emitting;
            data.EmitCount = emitCount;
        }

        protected override void OnReceiveNetworkMessage(NetworkMessage data)
        {
            emitting = data.Emitting;
            emitCount = data.EmitCount;
            base.OnReceiveNetworkMessage(data);
        }

        private void DoEmit()
        {
            if (IsFunctional && !emitting)
            {
                emitting = true;
                HardLinkedCapacitor?.DoEmit();
                emitCount = 0;
            }
        }

        private readonly HashSet<LaserBeamCapacitor> travelMap = new HashSet<LaserBeamCapacitor>();
        internal override void AddPower(double power, ulong fromGenerator)
        {
            travelMap.Clear();
            AddPower(power, fromGenerator, travelMap);
        }
        private void AddPower(double power, ulong fromGenerator, HashSet<LaserBeamCapacitor> travelMap)
        {
            if (travelMap.Contains(this))
            {
                //foreach (var capacitor in travelMap)
                //    power += capacitor.Power;
                //power /= travelMap.Count;
                //foreach (var capacitor in travelMap)
                //{
                //    if (fromGenerator > capacitor.receiveFromGenerator)
                //        capacitor.receiveFromGenerator = fromGenerator;
                //    capacitor.SetPower(power);
                //}
                return;
            }
            travelMap.Add(this);
            power = SendToLinkedCapacitor(power + Power, fromGenerator, travelMap);
            Power = 0.0;
            base.AddPower(power, fromGenerator);

            if (!emitting && Power >= MaxPowerCapacity && --emitCount < 0)
                DoEmit();
        }

        private double SendToLinkedCapacitor(double power, ulong fromGenerator, HashSet<LaserBeamCapacitor> travelMap)
        {
            sendToLinkedCapacitorAt = SessionComponents.UpdateSessionComponent.Simulation10Count;
            var cap = HardLinkedCapacitor;
            if (null != cap && cap.IsFunctional)
            {
                var leftToFull = cap.MaxPowerCapacity - cap.Power;
                if (power > leftToFull)
                {
                    power -= leftToFull;
                    cap.AddPower(leftToFull, fromGenerator, travelMap);
                }
                else if (power > 0.001)
                {
                    cap.AddPower(power - 0.001, fromGenerator, travelMap);
                    power = 0.001;
                }
                cap.emitCount = 3;
            }
            return power;
        }

        protected override void OnLazyUpdate(double elapsedSeconds)
        {
            base.OnLazyUpdate(elapsedSeconds);

            if (SessionComponents.UpdateSessionComponent.DoUpdate
                && sendToLinkedCapacitorAt < SessionComponents.UpdateSessionComponent.Simulation10Count)
            {
                travelMap.Clear();
                Power = SendToLinkedCapacitor(Power, receiveFromGenerator, travelMap);
            }

            if (emitting && Power <= Beam.Capacity * 0.001)
                emitting = false;
        }

        protected override void UpdateEmission()
        {
            base.UpdateEmission();

            float percent = (float)(Power / MaxPowerCapacity * Settings.EmissivePartNames.Length);
            for (int i = 0; i < Settings.EmissivePartNames.Length; i++)
            {
                var p = percent > 1f ? 1f : percent > 0f ? percent : 0f;
                percent -= 1f;
                Entity.SetEmissiveParts(
                    emissiveName: Settings.EmissivePartNames[i],
                    emissivePartColor: Color.Lerp(Colors.Black, Colors.White, p),
                    emissivity: p * 5f);
            }
        }
    }
}

using BeamSystem.Network;
using Sandbox.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace BeamSystem.BlockComponents
{
    abstract class BeamSplitter : BeamCombiner
    {
        private BeamEmitter emitter;

        private double splitRatio = 0.5;
        private IMyFunctionalBlock functionalBlock;
        public float SplitRatio => (float)splitRatio;
        public void SetSplitRatio(float value)
        {
            splitRatio = value;
            functionalBlock.RefreshCustomInfo();
            SnedDataToServer();
        }

        protected bool IsWorking => functionalBlock.IsWorking;
        protected override bool EmitterHasDrawing => base.EmitterHasDrawing || emitter.HasDrawing;


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            functionalBlock = CubeBlock as IMyFunctionalBlock;
            emitter = new BeamEmitter(this, beam: Beam, isForward: false);
            TerminalControls.BeamSplitterTerminalControl.Initialize();
            functionalBlock.IsWorkingChanged += OnWorkingChanged;
            functionalBlock.AppendingCustomInfo += AppendingCustomInfo;
        }

        private void OnWorkingChanged(VRage.Game.ModAPI.IMyCubeBlock obj)
        {
            UpdateEmission();
            functionalBlock.RefreshCustomInfo();
        }

        public override void OnRemovedFromScene()
        {
            functionalBlock.IsWorkingChanged -= OnWorkingChanged;
            functionalBlock.AppendingCustomInfo -= AppendingCustomInfo;
            emitter.OnRemovedFromScene();
            base.OnRemovedFromScene();
        }

        protected override void Load(DataStorage storage)
        {
            base.Load(storage);
            splitRatio = storage.GetDouble(Keys.SplitRatio, defaultValue: 0.5);
        }

        protected override void Save(DataStorage storage)
        {
            emitter.OnBeforeSave();
            base.Save(storage);
            storage.Set(Keys.SplitRatio, splitRatio);
        }

        protected override void OnSendNetworkMessage(NetworkMessage data)
        {
            base.OnSendNetworkMessage(data);
            data.SplitRatio = splitRatio;
        }

        protected override void OnReceiveNetworkMessage(NetworkMessage data)
        {
            base.OnReceiveNetworkMessage(data);
            splitRatio = data.SplitRatio;
            functionalBlock.RefreshCustomInfo();
        }

        protected override double OnReceivePowerFrom(double power, double angle, double linkDistance, bool isHardLink)
        {
            if (IsWorking && splitRatio > 0.0)
                if (splitRatio == 1.0)
                    return Beam.ReceiveEffectivity(power, 1.0 - angle, linkDistance, isHardLink);
                else
                    return Beam.ReceiveEffectivity(power * splitRatio, 1.0 - angle, linkDistance, isHardLink)
                            + Beam.ReceiveEffectivity(power * (1.0 - splitRatio), angle, linkDistance, isHardLink);
            else
                return Beam.ReceiveEffectivity(power, angle, linkDistance, isHardLink);
        }

        protected override void DoEmit(double elapsedSeconds, double emitPower, double additionalEmitPower)
        {
            if (IsWorking)
            {
                double emitPower1 = emitPower * splitRatio;
                double additionalEmitPower1 = additionalEmitPower * splitRatio;
                emitter.DoEmit(elapsedSeconds, emitPower1, additionalEmitPower1, receiveFromGenerator);
                base.DoEmit(elapsedSeconds, emitPower - emitPower1, additionalEmitPower - additionalEmitPower1);
            }
            else
            {
                emitter.DoEmit(elapsedSeconds, 0.0, 0.0, receiveFromGenerator);
                base.DoEmit(elapsedSeconds, emitPower, additionalEmitPower);
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            functionalBlock.RefreshCustomInfo();
        }

        Color indicatorColor = Colors.Lime;
        private void AppendingCustomInfo(IMyTerminalBlock block, System.Text.StringBuilder sb)
        {
            if (IsFunctional)
                if (IsWorking)
                {
                    float v = (float)splitRatio;
                    indicatorColor.G = (byte)(255.0 * v);
                    v *= 3f;
                    Entity.SetEmissiveParts(Settings.EmissivePartBackward, indicatorColor, v);
                    indicatorColor.G = (byte)(255 - indicatorColor.G);
                    Entity.SetEmissiveParts(Settings.EmissivePartForward, indicatorColor, 3f - v);

                    sb.AppendLine("Split: ")
                        .Append(" Forward: ").Append((1.0 - splitRatio).ToString("P")).AppendLine()
                        .Append(" Backward: ").Append(splitRatio.ToString("P")).AppendLine();
                }
                else
                {
                    sb.AppendLine("Stop Split");
                    Entity.SetEmissiveParts(Settings.EmissivePartForward, Colors.Red, 3f);
                    Entity.SetEmissiveParts(Settings.EmissivePartBackward, Colors.Red, 3f);
                }
            else
            {
                Entity.SetEmissiveParts(Settings.EmissivePartForward, Colors.Black, 0f);
                Entity.SetEmissiveParts(Settings.EmissivePartBackward, Colors.Black, 0f);
            }
        }

        protected override void UpdateEmitterAfterSimulation()
        {
            base.UpdateEmitterAfterSimulation();
            emitter.UpdateAfterSimulation();
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            emitter.UpdateBeforeSimulation100();
        }
    }
}
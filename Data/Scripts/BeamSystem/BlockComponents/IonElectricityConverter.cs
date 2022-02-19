using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;


namespace BeamSystem.BlockComponents
{
    [MyEntityComponentDescriptor(
        entityBuilderType: typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_BatteryBlock),
        useEntityUpdate: true,
        entityBuilderSubTypeNames: new string[] { "2CM_BS_Ion_Electricity_Converter_Large", "2CM_BS_Ion_Electricity_Converter_Small" })]
    class IonElectricityConverter : IonModuleForCombiner
    {
        protected override string AttachIndicatorEmissivePartName => Settings.EmissivePowerPartName;
        protected override IMySlimBlock AttachedBlock => ForwardBlock;
        MyBatteryBlockDefinition definition;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            definition = SlimBlock.BlockDefinition as MyBatteryBlockDefinition;
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
        }

        protected override void Load(DataStorage storage)
        {
        }

        protected override void Save(DataStorage storage)
        {
        }

        protected override void OnAdd(IonBeamCombiner to)
        {
        }

        protected override void OnRemove(IonBeamCombiner from)
        {
        }

        const float InvRequiredFlowRate = 1f / (1f - Settings.IonElectricityConvertMinimumRequiredFlowRate);
        protected override void Update()
        {
            base.Update();

            var source = CubeBlock.Components.Get<MyResourceSourceComponent>();
            var block = CubeBlock as MyBatteryBlock;
            if (null == linkedCombiner)
            {
                block.CurrentStoredPower = 0f;
            }
            else
            {
                float rate = (float)linkedCombiner.FlowPowerRate;
                if (rate > Settings.IonElectricityConvertMinimumRequiredFlowRate)
                {
                    rate -= Settings.IonElectricityConvertMinimumRequiredFlowRate;
                    rate *= InvRequiredFlowRate;
                    block.CurrentStoredPower = block.MaxStoredPower * rate * rate;
                }
            }
        }

        // Can't receivable beam even attached.
        public override BeamComponent GetBeamComponent() => null;
    }
}
//using Sandbox.Common.ObjectBuilders;
//using Sandbox.Definitions;
//using Sandbox.Game.EntityComponents;
//using System.Collections.Generic;
//using VRage.Game.Components;
//using VRage.ObjectBuilders;
//using VRage.Utils;


//namespace BeamSystem.BlockComponents
//{
//    using Beams;


//    [MyEntityComponentDescriptor(
//        entityBuilderType: typeof(MyObjectBuilder_SpaceBall),
//        useEntityUpdate: true)]
//    class LaserToElectricityConverterComponent : BeamComponent
//    {
//        protected override Beam Beam => Beam.Laser;

//        internal override bool IsBeamReceivable(Beam beam) => beam == this.Beam && CubeBlock.IsFunctional;

//        protected override double EmitRate => 0D;

//        private MyResourceSourceComponent source;
//        private float maxOutput;

//        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
//        {
//            base.Init(objectBuilder);

//            var def = CubeBlock.SlimBlock.BlockDefinition;
//            MyStringHash resourceGroup;
//            if (def is MySolarPanelDefinition)
//            {
//                var solarPanelDefinition = def as MySolarPanelDefinition;
//                maxOutput = solarPanelDefinition.MaxPowerOutput * Settings.SolarPanelMaxLaserReceiveRate;
//                resourceGroup = solarPanelDefinition.ResourceSourceGroup;
//            }
//            else
//            {
//                maxOutput = 10000f;
//                resourceGroup = MyStringHash.GetOrCompute("SolarPanels");
//            }
//            source = new MyResourceSourceComponent();
//            source.Init(resourceGroup, new List<MyResourceSourceInfo>
//            {
//                new MyResourceSourceInfo
//                {
//                    ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
//                    DefinedOutput = maxOutput,
//                    IsInfiniteCapacity= true,
//                    ProductionToCapacityMultiplier = 36f * Settings.UpdateTick,
//                }
//            });

//            Entity.Components.Add(source);
//        }

//        public override void OnAddedToScene()
//        {
//            base.OnAddedToScene();
//            source.Enabled = true;
//            Logger.Write("SolarPanel" + Entity.EntityId, "Added");
//        }

//        protected override void SetPower(double power)
//        {
//            if (null != source)
//            {
//                var electricity = (float)(power / Beam.GeneratorMaxCharging * Beam.RequiredElectricPower);
//                electricity = System.Math.Min(maxOutput, electricity);
//                source.SetMaxOutput(electricity);
//                source.SetProductionEnabledByType(MyResourceDistributorComponent.ElectricityId, electricity > 0D);
//            }
//            base.SetPower(0D);
//        }
//    }
//}
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    using GridComponents;


    [MyEntityComponentDescriptor(
        entityBuilderType: typeof(MyObjectBuilder_Thrust),
        useEntityUpdate: true,
        entityBuilderSubTypeNames: "2CM_AntiGravityImpellar")]
    class AntiGravityImpellar : MyGameLogicComponent
    {
        internal IMyThrust Thrust = null;
        private MyThrust mThrust = null;
        private Sandbox.ModAPI.Ingame.IMyThrust iThrust = null;
        internal bool IsActive => mThrust.CurrentStrength > 0f;


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();

            mThrust = Entity as MyThrust;
            Thrust = mThrust;
            iThrust = mThrust;
            Thrust.IsWorkingChanged += OnIsWorkingChanged;
            OnIsWorkingChanged(Entity as IMyCubeBlock);
        }

        private void OnAppendingCustomInfo(IMyTerminalBlock block, System.Text.StringBuilder sb)
        {
            sb.Append("Effectiveness: ");
            sb.Append(CurrentEffectiveness * 100f);
            sb.AppendLine("%");
        }

        private void OnIsWorkingChanged(IMyCubeBlock block)
        {
            if (Thrust.IsWorking)
            {
                NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
                Thrust.AppendingCustomInfo += OnAppendingCustomInfo;
                UpdateAfterSimulation100();
            }
            else
            {
                NeedsUpdate = MyEntityUpdateEnum.NONE;
                Thrust.AppendingCustomInfo -= OnAppendingCustomInfo;
            }
        }

        public override void UpdateAfterSimulation100()
        {
            Thrust.RefreshCustomInfo();
        }

        public float CurrentEffectiveness = 0f;

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (IsActive) AntigravityImpellerSystem.Get(Thrust.CubeGrid).Update();
        }
        //public override void UpdateBeforeSimulation()
        //{
        //    base.UpdateBeforeSimulation();
        //    if (IsActive)
        //    {
        //        AntigravityImpellerSystem.Get(Thrust.CubeGrid).Update();


        //        var myGrid = Thrust.CubeGrid;
        //        var system = AntigravityImpellerSystem.Get(myGrid);
        //        system.Update();

        //        Vector3 forward = Thrust.WorldMatrix.Forward;
        //        float angle;
        //        Vector3.Dot(ref forward, ref system.GravityDir, out angle);

        //        if (angle > 0f)
        //        {
        //            float efficiency = angle * angle * system.GravitySpeed;
        //            CurrentEffectiveness = efficiency * Settings.INV_EARTH_GRAVITY;
        //            efficiency *= Settings.AntiGravityImpellarMaxPower;

        //            float thrustMultiplier = system.GravitySpeed * system.Mass * Settings.AntiGravityImpellarForceScale;
        //            if (thrustMultiplier > efficiency)
        //                thrustMultiplier = efficiency;

        //            float fallingSpeed = system.FallingSpeed;
        //            if (fallingSpeed > 0f)
        //            {
        //                Thrust.ThrustMultiplier = thrustMultiplier;
        //                efficiency /= system.Mass;

        //                if (fallingSpeed > efficiency)
        //                    fallingSpeed = -efficiency;
        //                else
        //                    fallingSpeed = -fallingSpeed;
        //                Vector3 vForce = fallingSpeed * gravity;

        //                physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE,
        //                    force: vForce * physics.Mass,
        //                    position: physics.CenterOfMassWorld,
        //                    torque: Vector3.Zero);

        //                var group = MyAPIGateway.GridGroups.GetGroup(myGrid, GridLinkTypeEnum.Physical);
        //                if (group.Count > 1)
        //                    foreach (var grid in group)
        //                        if (grid != myGrid)
        //                        {
        //                            physics = grid.Physics;
        //                            physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE,
        //                                force: vForce * physics.Mass,
        //                                position: physics.CenterOfMassWorld,
        //                                torque: Vector3.Zero);
        //                        }
        //            }
        //            else if (Settings.AntiGravityMEpsilon < fallingSpeed)
        //            {
        //                fallingSpeed -= Settings.AntiGravityMEpsilon;
        //                fallingSpeed /= Settings.AntiGravityMEpsilon;
        //                fallingSpeed *= fallingSpeed;
        //                thrustMultiplier *= fallingSpeed * fallingSpeed;
        //                Thrust.ThrustMultiplier = thrustMultiplier;
        //            }
        //            else
        //                Thrust.ThrustMultiplier = 0f;
        //        }
        //        else
        //        {
        //            CurrentEffectiveness = 0f;
        //            Thrust.ThrustMultiplier = 0f;
        //        }
        //    }
        //}

        private void Log(object message)
            => Logger.Write("AG" + Entity.EntityId, message.ToString());
    }

}
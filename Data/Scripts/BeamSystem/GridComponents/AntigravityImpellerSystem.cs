using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using VRageMath;


namespace BeamSystem.GridComponents
{
    using SessionComponents;
    using BlockComponents;

    class AntigravityImpellerSystem : MyGameLogicComponent
    {
        internal static AntigravityImpellerSystem Get(IMyCubeGrid grid)
        {
            AntigravityImpellerSystem instance;
            if (grid.Components.TryGet(out instance))
                return instance;
            else
            {
                instance = new AntigravityImpellerSystem(grid);
                grid.Components.Add(instance);
                return instance;
            }
        }


        private readonly IMyCubeGrid grid;
        private ulong massUpdatedAt = 0ul, updatedAt = 0ul;
        private float mass;
        private readonly List<AntiGravityImpellar> impellers = new List<AntiGravityImpellar>();

        public float FallingSpeed, GravitySpeed;
        public Vector3 GravityDir;

        private AntigravityImpellerSystem(IMyCubeGrid grid)
        {
            this.grid = grid;
            grid.OnBlockAdded += OnBlockAdded;
            grid.OnBlockRemoved += OnBlockRemoved;
            grid.OnClosing += OnClosing;
            List<IMySlimBlock> slimBlocks = new List<IMySlimBlock>();
            grid.GetBlocks(slimBlocks, block =>
            {
                var impeller = block.FatBlock?.GameLogic.GetAs<AntiGravityImpellar>();
                if (null != impeller)
                {
                    impellers.Add(impeller);
                }
                return false;
            });

        }

        private void OnClosing(VRage.ModAPI.IMyEntity obj)
        {
            impellers.Clear();
        }

        private void OnBlockRemoved(IMySlimBlock obj)
        {
            var impeller = obj.FatBlock?.GameLogic.GetAs<AntiGravityImpellar>();
            if (null != impeller)
            {
                impellers.Remove(impeller);
                Logger.Write("ImpellerSystem_" + Entity.EntityId, $"Remove Impeller({impeller.Entity.EntityId})");
            }
        }

        private void OnBlockAdded(IMySlimBlock obj)
        {
            var impeller = obj.FatBlock?.GameLogic.GetAs<AntiGravityImpellar>();
            if (null != impeller)
            {
                impellers.Add(impeller);
                Logger.Write("ImpellerSystem_" + Entity.EntityId, $"Add Impeller({impeller.Entity.EntityId})");
            }
        }

        public float Mass
        {
            get
            {
                if (massUpdatedAt < UpdateSessionComponent.Simulation10Count)
                {
                    massUpdatedAt = UpdateSessionComponent.Simulation10Count;
                    var groups = MyAPIGateway.GridGroups.GetGroup(this.grid, GridLinkTypeEnum.Physical);
                    mass = 0f;
                    foreach (var grid in groups)
                        mass += grid.Physics.Mass;
                    foreach (var grid in groups)
                        Get(grid).UpdateMass(mass);
                }
                return mass;
            }
        }
        private void UpdateMass(float mass)
        {
            this.mass = mass;
            this.massUpdatedAt = UpdateSessionComponent.Simulation10Count;
        }

        private void ComputeAntigravityForce(ref Vector3 force)
        {
            if (updatedAt < UpdateSessionComponent.SimulationCount && impellers.Count > 0)
            {
                updatedAt = UpdateSessionComponent.SimulationCount;
                var physics = grid.Physics;
                GravityDir = physics.Gravity;
                GravitySpeed = GravityDir.Normalize();
                var velocity = physics.LinearVelocity;
                Vector3.Dot(ref GravityDir, ref velocity, out FallingSpeed);

                var shipMass = Mass;
                var shipMassInv = 1f / shipMass;
                float antigravityForce = GravitySpeed * shipMass * Settings.AntiGravityImpellarForceScale;

                int _case = 0;
                var forceScalar = this.FallingSpeed;
                if (forceScalar <= 0f)
                    if (Settings.AntiGravityMEpsilon < forceScalar)
                    {
                        _case = 1;
                        forceScalar -= Settings.AntiGravityMEpsilon;
                        forceScalar /= Settings.AntiGravityMEpsilon;
                        forceScalar *= forceScalar;
                    }
                    else
                    {
                        _case = 2;
                    }

                foreach (var impeller in impellers)
                    if (impeller.IsActive)
                    {
                        Vector3 forward = impeller.Thrust.WorldMatrix.Forward;
                        float angle;
                        Vector3.Dot(ref forward, ref GravityDir, out angle);

                        if (angle > 0f)
                        {
                            float efficiency = angle * angle * GravitySpeed;
                            impeller.CurrentEffectiveness = efficiency * Settings.INV_EARTH_GRAVITY;
                            efficiency *= Settings.AntiGravityImpellarMaxPower;


                            float thrustMultiplier = antigravityForce > efficiency ? efficiency : antigravityForce;

                            switch (_case)
                            {
                                case 0:
                                    impeller.Thrust.ThrustMultiplier = thrustMultiplier;
                                    efficiency *= shipMassInv;
                                    if (forceScalar > efficiency)
                                        forceScalar -= efficiency;
                                    else
                                        forceScalar = -forceScalar;
                                    force += forceScalar * GravityDir;
                                    break;

                                case 1:
                                    thrustMultiplier *= forceScalar * forceScalar;
                                    impeller.Thrust.ThrustMultiplier = thrustMultiplier;
                                    break;

                                case 2:
                                default:
                                    impeller.Thrust.ThrustMultiplier = 0f;
                                    break;
                            }
                        }
                        else
                        {
                            impeller.CurrentEffectiveness = 0f;
                            impeller.Thrust.ThrustMultiplier = 0f;
                        }
                    }
            }
        }

        public void Update()
        {
            if (updatedAt < UpdateSessionComponent.SimulationCount)
            {
                var group = MyAPIGateway.GridGroups.GetGroup(grid, GridLinkTypeEnum.Physical);
                Vector3 force = Vector3.Zero;
                foreach (var grid in group)
                {
                    var sys = Get(grid);
                    sys.ComputeAntigravityForce(ref force);
                }

                if (!Vector3.IsZero(force))
                    foreach (var grid in group)
                    {
                        var physics = grid.Physics;
                        physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE,
                                force: force * physics.Mass,
                                position: physics.CenterOfMassWorld,
                                torque: Vector3.Zero);
                    }
            }
        }
    }
}

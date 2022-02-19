using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    abstract class BaseComponent : MyGameLogicComponent
    {
        public IMyCubeBlock CubeBlock;
        public IMySlimBlock SlimBlock => CubeBlock.SlimBlock;
        public IMyCubeGrid CubeGrid => CubeBlock.CubeGrid;
        public double GridSize => CubeGrid.GridSize;
        public Vector3 LinearVelocity => CubeGrid.Physics.LinearVelocity;
        internal Vector3I Position => CubeBlock.Position;


        private Vector3I oldPosition = new Vector3I(int.MinValue, int.MinValue, int.MinValue);
        private Vector3I forwardPos, backwardPos, leftPos, rightPos, upPos, downPos;

        public bool InScene => Entity?.InScene ?? false;

        internal bool IsFunctional => !Closed && null != CubeBlock && CubeBlock.InScene && CubeBlock.IsFunctional;

        private void UpdateNeightborBlocks()
        {
            var pos = SlimBlock.Position;
            if (oldPosition != pos)
            {
                oldPosition = pos;
                var ori = SlimBlock.Orientation;
                var max = CubeBlock.Max;
                var min = CubeBlock.Min;
                forwardPos = backwardPos = (max + min) / 2;

                var dir = Base6Directions.GetIntVector(ori.Forward);
                if (dir.X != 0)
                    forwardPos.X = dir.X > 0 ? max.X + 1 : min.X - 1;
                else if (dir.Y != 0)
                    forwardPos.Y = dir.Y > 0 ? max.Y + 1 : min.Y - 1;
                else
                    forwardPos.Z = dir.Z > 0 ? max.Z + 1 : min.Z - 1;

                dir = -dir;
                if (dir.X != 0)
                    backwardPos.X = dir.X > 0 ? max.X + 1 : min.X - 1;
                else if (dir.Y != 0)
                    backwardPos.Y = dir.Y > 0 ? max.Y + 1 : min.Y - 1;
                else
                    backwardPos.Z = dir.Z > 0 ? max.Z + 1 : min.Z - 1;

                dir = Base6Directions.GetIntVector(ori.Left);
                if (dir.X != 0)
                    leftPos.X = dir.X > 0 ? max.X + 1 : min.X - 1;
                else if (dir.Y != 0)
                    leftPos.Y = dir.Y > 0 ? max.Y + 1 : min.Y - 1;
                else
                    leftPos.Z = dir.Z > 0 ? max.Z + 1 : min.Z - 1;

                dir = -dir;
                if (dir.X != 0)
                    rightPos.X = dir.X > 0 ? max.X + 1 : min.X - 1;
                else if (dir.Y != 0)
                    rightPos.Y = dir.Y > 0 ? max.Y + 1 : min.Y - 1;
                else
                    rightPos.Z = dir.Z > 0 ? max.Z + 1 : min.Z - 1;

                dir = Base6Directions.GetIntVector(ori.Up);
                if (dir.X != 0)
                    upPos.X = dir.X > 0 ? max.X + 1 : min.X - 1;
                else if (dir.Y != 0)
                    upPos.Y = dir.Y > 0 ? max.Y + 1 : min.Y - 1;
                else
                    upPos.Z = dir.Z > 0 ? max.Z + 1 : min.Z - 1;

                dir = -dir;
                if (dir.X != 0)
                    downPos.X = dir.X > 0 ? max.X + 1 : min.X - 1;
                else if (dir.Y != 0)
                    downPos.Y = dir.Y > 0 ? max.Y + 1 : min.Y - 1;
                else
                    downPos.Z = dir.Z > 0 ? max.Z + 1 : min.Z - 1;
            }
        }

        public IMySlimBlock ForwardBlock
        {
            get
            {
                UpdateNeightborBlocks();
                return CubeGrid.GetCubeBlock(forwardPos);
            }
        }

        public IMySlimBlock BackwardBlock
        {
            get
            {
                UpdateNeightborBlocks();
                return CubeGrid.GetCubeBlock(backwardPos);
            }
        }

        public IMySlimBlock LeftBlock
        {
            get
            {
                UpdateNeightborBlocks();
                return CubeGrid.GetCubeBlock(leftPos);
            }
        }

        public IMySlimBlock RightBlock
        {
            get
            {
                UpdateNeightborBlocks();
                return CubeGrid.GetCubeBlock(rightPos);
            }
        }

        public IMySlimBlock UpBlock
        {
            get
            {
                UpdateNeightborBlocks();
                return CubeGrid.GetCubeBlock(upPos);
            }
        }

        public IMySlimBlock DownBlock
        {
            get
            {
                UpdateNeightborBlocks();
                return CubeGrid.GetCubeBlock(downPos);
            }
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            CubeBlock = Entity as IMyCubeBlock;
        }

        internal static bool IsServer => MyAPIGateway.Multiplayer.IsServer;

        internal static bool IsClient => !IsServer;

        internal static bool IsDedicated => MyAPIGateway.Multiplayer.IsServer && MyAPIGateway.Utilities.IsDedicated;

        internal static long Timestamp => MyAPIGateway.Session.GameDateTime.Ticks;
    }
}
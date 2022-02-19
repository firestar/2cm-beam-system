using Sandbox.ModAPI;
using System.Text;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    abstract class IonModuleForCombiner : BeamBaseComponent
    {
        protected const float EmissionMultiplier = Settings.EmissionMultiplier * 2f;

        static IonModuleForCombiner()
        {
            //for (int i = 1; i <= 51; i++)
            //    Sounds[i - 1] = new MySoundPair("_2cm_IonCapacitor_" + i);
            for (int i = 0; i < IntDirections.Length; i++)
            {
                var t = Base6Directions.IntDirections[i];
                t.X = t.X == 0.0 ? ~0 : 0;
                t.Y = t.Y == 0.0 ? ~0 : 0;
                t.Z = t.Z == 0.0 ? ~0 : 0;
                IntDirections[i] = t;
            }
        }

        protected IonBeamCombiner linkedCombiner = null;
        protected IMyTerminalBlock terminalBlock = null;
        protected string customInfo = string.Empty;

        protected virtual bool NeedsKeepUpdate => false;

        protected abstract IMySlimBlock AttachedBlock { get; }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            terminalBlock = CubeBlock as IMyTerminalBlock;
            terminalBlock.AppendingCustomInfo += AppendCustomInfo;
            NeedsUpdate = MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void OnRemovedFromScene()
        {
            terminalBlock.AppendingCustomInfo -= AppendCustomInfo;
            if (linkedCombiner != null)
            {
                if (linkedCombiner.InScene)
                    OnRemove(from: linkedCombiner);
                linkedCombiner = null;
            }
            base.OnRemovedFromScene();
        }


        private void AppendCustomInfo(IMyTerminalBlock block, StringBuilder sb) => OnAppendCustonInfo(sb);
        protected virtual void OnAppendCustonInfo(StringBuilder sb) => sb.Append(customInfo);


        private static readonly Vector3I[] IntDirections = new Vector3I[Base6Directions.IntDirections.Length];

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            Update();
        }

        public override void UpdateBeforeSimulation10()
        {
            var update = NeedsUpdate;
            update &= ~MyEntityUpdateEnum.EACH_10TH_FRAME;
            update = MyEntityUpdateEnum.EACH_100TH_FRAME;
            NeedsUpdate = update;

            Update();
        }
        public override void UpdateBeforeSimulation100() => Update();

        int oldInfoState = 0;
        protected virtual void Update()
        {
            Vector3I filter;
            BeamComponent component;
            if (CubeBlock.IsFunctional
                && null != (component = GetComponentOf(AttachedBlock))
                && component is IonBeamCombiner
                && component.Position.And((filter = IntDirections[(int)CubeBlock.Orientation.Forward % 6])) == Position.And(filter))
            {
                if (linkedCombiner != component)
                {
                    linkedCombiner = component as IonBeamCombiner;
                    OnAdd(to: linkedCombiner);
                }
                CubeBlock.SetEmissiveParts(AttachIndicatorEmissivePartName, GetAttachedIndicatorColor(), 2f);
                oldInfoState = 0;
            }
            else
            {
                if (linkedCombiner != null)
                {
                    OnRemove(from: linkedCombiner);
                    linkedCombiner = null;
                }
                if (CubeBlock.IsFunctional)
                {
                    customInfo = "Has no linked combiner";
                    CubeBlock.SetEmissiveParts(AttachIndicatorEmissivePartName, Colors.Red, 2f);
                    if (oldInfoState != 1)
                    {
                        oldInfoState = 1;
                        terminalBlock.RefreshCustomInfo();
                    }
                }
                else
                {
                    customInfo = string.Empty;
                    CubeBlock.SetEmissiveParts(AttachIndicatorEmissivePartName, Colors.Black, 0f);
                    if (oldInfoState != 2)
                    {
                        oldInfoState = 2;
                        terminalBlock.RefreshCustomInfo();
                    }
                }
            }
        }

        protected abstract void OnAdd(IonBeamCombiner to);
        protected abstract void OnRemove(IonBeamCombiner from);

        protected abstract string AttachIndicatorEmissivePartName { get; }

        protected virtual Color GetAttachedIndicatorColor() => Colors.White;

    }
}
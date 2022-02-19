using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;


namespace BeamSystem.SessionComponents
{
    using GridComponents;
    using Network;

    [MySessionComponentDescriptor(updateOrder: MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation)]
    class UpdateSessionComponent : MySessionComponentBase
    {
        const double FRAME_DURATION = 1D / 60D;
        internal const double LazyUpdateFrameDuration = Settings.UpdateTick * FRAME_DURATION;

        private static readonly List<GridResourceSink> gridResourceSinks = new List<GridResourceSink>(128);


        internal static ulong SimulationCount = 0ul;
        internal static ulong Simulation10Count = 0ul;
        internal static bool DoUpdate = false;

        internal static void Add(GridResourceSink gridResourceSink)
        {
            if (!gridResourceSinks.Contains(gridResourceSink))
                gridResourceSinks.Add(gridResourceSink);
        }
        internal static void Remove(GridResourceSink gridResourceSink) => gridResourceSinks.Remove(gridResourceSink);


        private int tick;


        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            MessageRelay.Init();
            SimulationCount = 0ul;
            Simulation10Count = 0ul;
            gridResourceSinks.Clear();
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            MessageRelay.Dispose();
        }

        public override void UpdateBeforeSimulation()
        {
            ++SimulationCount;
            if (tick == 0)
            {
                ++Simulation10Count;
                DoUpdate = Simulation10Count > Settings.IgnoreStartFrames;
                try
                {
                    foreach (var sink in gridResourceSinks)
                        sink.Update();
                }
                catch (System.InvalidOperationException)
                {
                }
            }
        }

        public override void UpdateAfterSimulation()
        {
            tick = (tick + 1) % Settings.UpdateTick;
        }


    }
}

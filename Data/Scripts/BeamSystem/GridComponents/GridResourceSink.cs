using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;


namespace BeamSystem.GridComponents
{
    using BlockComponents;
    using SessionComponents;


    class GridResourceSink : MyGameLogicComponent
    {
        internal static GridResourceSink GetResourceSinkOf(IMyCubeGrid grid, BeamGenerator generator)
        {
            GridResourceSink sink;
            if (grid.Components.TryGet(out sink))
            {
                sink.Add(generator);
                return sink;
            }
            else
            {
                sink = new GridResourceSink(grid, generator);
                grid.Components.Add(sink);
                UpdateSessionComponent.Add(sink);
                return sink;
            }
        }


        private readonly HashSet<BeamGenerator> generators = new HashSet<BeamGenerator>();
        private readonly IMyCubeGrid grid;
        private BeamGenerator activeGenerator = null;


        private MyFixedPoint oldRequiredPower;
        private MyFixedPoint newRequiredPower;

        internal void Add(BeamGenerator generator)
        {
            generators.Add(generator);
        }

        internal void Remove(BeamGenerator generator)
        {
            var resourceSink = (generator.Block as IMyUpgradableBlock).ResourceSink as MyResourceSinkComponent;
            if (null != resourceSink)
            {
                resourceSink.SetInputFromDistributor(MyResourceDistributorComponent.ElectricityId, 0f, false, fireEvents: false);
                resourceSink.Update();
            }
            if (activeGenerator == generator)
            {
                activeGenerator = null;
            }
            generators.Remove(generator);
        }

        private GridResourceSink(IMyCubeGrid grid)
        {
            this.grid = grid;
            oldRequiredPower = MyFixedPoint.MinValue;
            grid.OnBlockAdded += OnBlockAdded;
            grid.OnBlockRemoved += OnBlockRemoved;
            grid.OnClosing += OnClosing;
            grid.OnGridSplit += OnGridSplit;
        }

        private void OnGridSplit(IMyCubeGrid grid1, IMyCubeGrid grid2)
        {
            IMyCubeGrid newGrid = grid == grid1 ? grid2 : grid1;
            GridResourceSink sink;
            if (!newGrid.Components.TryGet(out sink))
            {
                sink = new GridResourceSink(newGrid);
                newGrid.Components.Add(sink);
            }
            UpdateSessionComponent.Add(sink);
        }

        internal GridResourceSink(IMyCubeGrid grid, BeamGenerator generator) : this(grid)
        {
            Add(generator);
        }

        public override void OnAddedToScene()
        {
            UpdateSessionComponent.Add(this);
        }

        public override void OnRemovedFromScene()
        {
            UpdateSessionComponent.Remove(this);
        }

        private void OnClosing(IMyEntity obj)
        {
            grid.OnBlockAdded -= OnBlockAdded;
            grid.OnBlockRemoved -= OnBlockRemoved;
            grid.OnClosing -= OnClosing;
            activeGenerator = null;
            generators.Clear();
        }

        private void OnBlockRemoved(IMySlimBlock obj)
        {
            var component = BeamBaseComponent.GetComponentOf(obj);
            if (null != component && component is BeamGenerator)
            {
                Remove(component as BeamGenerator);
            }
        }

        private void OnBlockAdded(IMySlimBlock obj)
        {
            var component = BeamBaseComponent.GetComponentOf(obj);
            if (null != component && component is BeamGenerator)
            {
                Add(component as BeamGenerator);
            }
        }

        internal float GetRequiredInput(BeamGenerator generator) => generator == activeGenerator ? (float)oldRequiredPower : 0f;

        internal float PowerRatio { get; private set; }

        public void Update()
        {
            if (generators.Count > 0)
            {
                newRequiredPower = MyFixedPoint.Zero;
                try
                {
                    foreach (var generator in generators)
                        newRequiredPower += generator.consumePower;
                }
                catch (System.InvalidOperationException) { return; }

                float requiredPower = (float)newRequiredPower;
                if (oldRequiredPower != newRequiredPower)
                {
                    oldRequiredPower = newRequiredPower;
                    if (null == activeGenerator && generators.Count > 0)
                        activeGenerator = generators.FirstElement();
                    if (null != activeGenerator)
                    {
                        var resourceSink = activeGenerator.Block.ResourceSink as MyResourceSinkComponent;
                        resourceSink.SetMaxRequiredInputByType(
                            MyResourceDistributorComponent.ElectricityId, requiredPower);
                        resourceSink.Update();
                        float vm = resourceSink.ResourceAvailableByType(MyResourceDistributorComponent.ElectricityId);
                        if (requiredPower > 0f)
                            PowerRatio = MathHelper.Clamp(vm / requiredPower, min: 0f, max: 1f);
                        else
                            PowerRatio = 1f;

                        // Logger.Write("GRID_" + Entity.EntityId, $"Update Power Consume: {requiredPower}  {PowerRatio:P}");
                    }
                    else
                    {
                        activeGenerator = null;
                        PowerRatio = 0f;
                    }
                }
                else
                {
                    var resourceSink = activeGenerator?.Block.ResourceSink as MyResourceSinkComponent;
                    if (newRequiredPower == MyFixedPoint.Zero || null == resourceSink)
                    {
                        PowerRatio = 0f;
                    }
                    else
                    {
                        float vm = resourceSink.ResourceAvailableByType(MyResourceDistributorComponent.ElectricityId);
                        if (requiredPower > 0f)
                            PowerRatio = MathHelper.Clamp(vm / requiredPower, min: 0f, max: 1f);
                        else
                            PowerRatio = 1f;
                    }
                }
            }
            else
            {
                PowerRatio = 0f;
            }
        }

    }
}
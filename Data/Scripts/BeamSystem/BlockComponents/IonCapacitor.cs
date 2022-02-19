using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;


namespace BeamSystem.BlockComponents
{
    [MyEntityComponentDescriptor(
        entityBuilderType: typeof(MyObjectBuilder_TerminalBlock),
        useEntityUpdate: true,
        entityBuilderSubTypeNames: new string[] { "2CM_BS_Ion_Capacitor", "2CM_BS_Ion_CapacitorS", "2CM_BS_Ion_CapacitorS_Small" })]
    class IonCapacitor : IonModuleForCombiner
    {
        internal bool PresentCustonData = false;

        internal float ChargedPowerRate => linkedCombiner?.ChargedPowerRate ?? 0f;

        protected override string AttachIndicatorEmissivePartName => Settings.EmissivePowerPartName;
        protected override IMySlimBlock AttachedBlock => ForwardBlock;

        internal double ExtraCapacity;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            ExtraCapacity = Settings.IonExtraCapacity * (CubeBlock.BlockDefinition.SubtypeId.Equals("2CM_BS_Ion_Capacitor") ? 2.0 : 1.0);
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            TerminalControls.IonCapaciterTerminalControl.Initialize();
        }

        protected override void Load(DataStorage storage)
        {
            PresentCustonData = storage.GetBool(Keys.PresentCustonData, false);
        }

        protected override void Save(DataStorage storage)
        {
            storage.Set(Keys.PresentCustonData, PresentCustonData);
        }

        protected override void Update()
        {
            base.Update();
            if (null == linkedCombiner)
                UpdateVisual(0f);
        }

        /// <summary>
        /// </summary>
        /// <param name="k">0..5</param>
        internal void UpdateVisual(float k)
        {
            customInfo = $"Charged: {k * 0.2f:P}";
            if (PresentCustonData)
                terminalBlock.CustomData = (k * 0.2f).ToString("0.0000");
            terminalBlock.RefreshCustomInfo();
            for (int i = 0; i < 5; i++)
            {
                var emissivity = MathHelper.Clamp(k, 0f, 1f);
                CubeBlock.SetEmissiveParts(
                    emissiveName: Settings.EmissivePartNames[i],
                    emissivePartColor: Color.Lerp(Colors.Black, Colors.White, emissivity),
                    emissivity: (emissivity - 0.5f) * EmissionMultiplier);
                k -= 1f;
            }
        }

        protected override void OnAdd(IonBeamCombiner to) => to.Add(capacitor: this);
        protected override void OnRemove(IonBeamCombiner from) => from.Remove(capacitor: this);

        // Can't receivable beam even attached.
        public override BeamComponent GetBeamComponent() => null;
    }
}
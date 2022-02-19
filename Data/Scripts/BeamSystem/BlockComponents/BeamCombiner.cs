namespace BeamSystem.BlockComponents
{
    using Beams;


    abstract class BeamCombiner : BeamComponent
    {
        public override bool IsBeamReceivable(Beam beam) => beam.IsAcceptable && beam.Type == this.Beam.Type && CubeBlock.IsFunctional;

        protected override double EmitRate => Beam.EmitRate(Power);
        protected override double MaxPowerCapacity => Beam.Capacity;
        internal float GetCurrentPower() => (float)Power;

        protected override void InitializeTerminalControl() => TerminalControls.BeamCombinerTerminalControl.Initialize();
    }
}

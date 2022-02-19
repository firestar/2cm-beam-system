using Sandbox.ModAPI;


namespace BeamSystem.TerminalControls
{
    using BlockComponents;


    static class BeamCombinerTerminalControl
    {
        static bool initialized = false;


        internal static bool IsBeamCombiner(IMyTerminalBlock block) => block?.AsCombiner() != null;

        internal static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            var property = MyAPIGateway.TerminalControls.CreateProperty<float, IMyTerminalBlock>(PropertyNames.CurrentPower);
            property.Getter = block => block.AsCombiner()?.GetCurrentPower() ?? 0f;
            property.Enabled = IsBeamCombiner;
            property.Visible = IsBeamCombiner;
            MyAPIGateway.TerminalControls.AddControl<IMyPassage>(property);
        }


        #region Utilities

        static BeamCombiner AsCombiner(this IMyTerminalBlock block)
        {
            return block?.GameLogic?.GetAs<BeamCombiner>();
        }

        #endregion 
    }
}

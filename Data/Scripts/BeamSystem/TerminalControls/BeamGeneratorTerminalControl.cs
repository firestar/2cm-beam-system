using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Utils;


namespace BeamSystem.TerminalControls
{
    using BlockComponents;


    static class BeamGeneratorTerminalControl
    {
        static bool initialized = false;


        internal static bool IsGenerator(IMyTerminalBlock block) => block?.AsGenerator() != null;

        internal static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            var outputPowerSlider = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyUpgradeModule>(PropertyNames.OutputPower);
            outputPowerSlider.Title = MyStringId.GetOrCompute("Output Power");
            outputPowerSlider.Tooltip = MyStringId.GetOrCompute(
$@"Beam Output Power Rate: 0.0 ~ 100.0
Property: {PropertyNames.OutputPower}");
            outputPowerSlider.SetLimits(0f, 100f);
            outputPowerSlider.SupportsMultipleBlocks = true;
            outputPowerSlider.Getter = block => block.AsGenerator()?.OutputRate * 100f ?? 0f;
            outputPowerSlider.Setter = (block, value) => block.AsGenerator()?.SetOutputRate(value * 0.01f);
            outputPowerSlider.Writer = (block, result) => MyValueFormatter.AppendWorkInBestUnit((float)(block.AsGenerator()?.OutputPower ?? 0.0), result);
            outputPowerSlider.Visible = IsGenerator;
            MyAPIGateway.TerminalControls.AddControl<IMyUpgradeModule>(outputPowerSlider);

            var property = MyAPIGateway.TerminalControls.CreateProperty<float, IMyUpgradeModule>(PropertyNames.CurrentPower);
            property.Getter = block => block.AsGenerator()?.CurrentPower ?? 0f;
            property.Enabled = IsGenerator;
            property.Visible = IsGenerator;
        }


        #region Utilities

        static BeamGenerator AsGenerator(this IMyTerminalBlock block)
        {
            return block?.GameLogic?.GetAs<BeamGenerator>();
        }

        #endregion
    }
}

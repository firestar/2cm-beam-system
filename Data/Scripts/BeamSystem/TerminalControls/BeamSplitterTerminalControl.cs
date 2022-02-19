using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Utils;


namespace BeamSystem.TerminalControls
{
    using BlockComponents;


    static class BeamSplitterTerminalControl
    {
        static bool initialized = false;


        internal static bool IsBeamSpliter(IMyTerminalBlock block) => block?.AsSpliter() != null;

        internal static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            var splitterRatioSlider = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyUpgradeModule>(PropertyNames.SplitRatio);
            splitterRatioSlider.Title = MyStringId.GetOrCompute("Split Ratio");
            splitterRatioSlider.Tooltip = MyStringId.GetOrCompute(
$@"Beam Split Ratio: 0.0 ~ 1.0
Property: {PropertyNames.SplitRatio}");
            splitterRatioSlider.SetLimits(0f, 1f);
            splitterRatioSlider.SupportsMultipleBlocks = true;
            splitterRatioSlider.Getter = block => block.AsSpliter()?.SplitRatio ?? 0f;
            splitterRatioSlider.Setter = (block, value) => block.AsSpliter()?.SetSplitRatio(value);
            splitterRatioSlider.Writer = (block, result) => result.Append(block.AsSpliter()?.SplitRatio.ToString("P"));
            splitterRatioSlider.Visible = IsBeamSpliter;
            MyAPIGateway.TerminalControls.AddControl<IMyUpgradeModule>(splitterRatioSlider);
        }


        #region Utilities

        static BeamSplitter AsSpliter(this IMyTerminalBlock block)
        {
            return block?.GameLogic?.GetAs<BeamSplitter>();
        }

        #endregion 
    }
}

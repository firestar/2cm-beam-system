using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Utils;


namespace BeamSystem.TerminalControls
{
    using BlockComponents;


    static class IonActiveEmitterTerminalControl
    {
        static bool initialized = false;


        internal static bool IsEmitter(IMyTerminalBlock block) => block?.AsEmitter() != null;

        internal static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            var property = MyAPIGateway.TerminalControls.CreateProperty<float, IMyUpgradeModule>(PropertyNames.CurrentPower);
            property.Getter = block => block.AsEmitter()?.CurrentOutputPower ?? 0f;
            property.Enabled = IsEmitter;
            property.Visible = IsEmitter;
            MyAPIGateway.TerminalControls.AddControl<IMyUpgradeModule>(property);

            var splitterRatioSlider = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyUpgradeModule>(PropertyNames.SplitRatio);
            splitterRatioSlider.Title = MyStringId.GetOrCompute("Emitter Split Weight");
            splitterRatioSlider.Tooltip = MyStringId.GetOrCompute(
$@"Control this emitter's emit weight
Emit Split Weigjht: 0.0 ~ 1.0
Property: {PropertyNames.SplitRatio}");
            splitterRatioSlider.SetLimits(0f, 1f);
            splitterRatioSlider.SupportsMultipleBlocks = true;
            splitterRatioSlider.Getter = block => block.AsEmitter()?.GetSplitRatio() ?? 0f;
            splitterRatioSlider.Setter = (block, value) => block.AsEmitter()?.SetSplitRatio(value);
            splitterRatioSlider.Writer = (block, result) => result.Append(block.AsEmitter()?.GetSplitRatio().ToString("0.00") ?? "0.00");
            splitterRatioSlider.Visible = IsEmitter;
            MyAPIGateway.TerminalControls.AddControl<IMyUpgradeModule>(splitterRatioSlider);

            splitterRatioSlider = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyUpgradeModule>(PropertyNames.CombinerSplitRatio);
            splitterRatioSlider.Title = MyStringId.GetOrCompute("Combiner Split Weight");
            splitterRatioSlider.Tooltip = MyStringId.GetOrCompute(
$@"Control the attached combiner emit weight
Emit Split Weigjht: 0.0 ~ 1.0
Property: {PropertyNames.SplitRatio}");
            splitterRatioSlider.SetLimits(0f, 1f);
            splitterRatioSlider.SupportsMultipleBlocks = true;
            splitterRatioSlider.Getter = block => block.AsEmitter()?.CombinerSplitRatio ?? 0f;
            splitterRatioSlider.Setter = (block, value) => block.AsEmitter()?.SetCombinerSplitRatio(value);
            splitterRatioSlider.Writer = (block, result) => result.Append(block.AsEmitter()?.CombinerSplitRatio.ToString("0.00"));
            splitterRatioSlider.Visible = block => block.AsEmitter()?.IsAttached ?? false;
            MyAPIGateway.TerminalControls.AddControl<IMyUpgradeModule>(splitterRatioSlider);
        }

        #region Utilities

        static IonActiveEmitterModule AsEmitter(this IMyTerminalBlock block)
        {
            return block?.GameLogic?.GetAs<IonActiveEmitterModule>();
        }

        #endregion 
    }
}

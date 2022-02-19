using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Utils;


namespace BeamSystem.TerminalControls
{
    using BlockComponents;


    static class IonCapaciterTerminalControl
    {
        static bool initialized = false;


        internal static bool IsCapacitor(IMyTerminalBlock block) => block?.AsCapacitor() != null;

        internal static void Initialize()
        {
            if (initialized) return;
            initialized = true;
            {
                var control = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyTerminalBlock>(PropertyNames.PresentChargedRate);
                control.OnText = MyStringId.GetOrCompute("ON");
                control.OffText = MyStringId.GetOrCompute("OFF");
                control.Title = MyStringId.GetOrCompute("Write charged rate on / off");
                control.Tooltip = MyStringId.GetOrCompute(
$@"Write charged rate to custom data
Warning: if use this option, will overwrite Custom Data
Property: ""{PropertyNames.ChargedRate}""");
                control.Getter = block => block.AsCapacitor()?.PresentCustonData ?? false;
                control.Setter = (block, value) => block.AsCapacitor().PresentCustonData = value;
                control.Visible = IsCapacitor;
                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(control);
            }
            {
                var property = MyAPIGateway.TerminalControls.CreateProperty<float, IMyTerminalBlock>(PropertyNames.ChargedRate);
                property.Getter = block => block.AsCapacitor()?.ChargedPowerRate ?? 0f;
                property.Visible = IsCapacitor;
                property.Enabled = IsCapacitor;
                MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(property);
            }
        }


        #region Utilities

        static IonCapacitor AsCapacitor(this IMyTerminalBlock block)
        {
            return block?.GameLogic?.GetAs<IonCapacitor>();
        }

        #endregion
    }
}

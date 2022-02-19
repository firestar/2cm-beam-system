using Sandbox.ModAPI;
using VRage.Utils;

namespace BeamSystem
{
    static class Logger
    {
        internal static void Write(string sender, string mesageText)
        {
            MyAPIGateway.Utilities?.ShowMessage(sender, mesageText);
        }

        internal static void Write(string sender, object message)
        {
            MyAPIGateway.Utilities?.ShowMessage(sender, message.ToString());
        }

        internal static void Write(string sender, string format, params object[] args)
            => Write(sender, string.Format(format, args));

        internal static void Error(string sender, object message)
        {
            Write(sender, message);
            MyLog.Default.Error($"[{sender}]: {message}");
        }
    }
}

using System;
using VRage.Game.Components;
using VRage.Utils;

namespace BeamSystem
{
    static class MyModStorageBaseExtensions
    {
        internal static string Get(this MyModStorageComponentBase storage, Guid key, string defaultValue)
        {
            string value;
            return storage.TryGetValue(key, out value) ? value : defaultValue;
        }

        internal static double Get(this MyModStorageComponentBase storage, Guid key, double defaultValue)
        {
            string str;
            double v;
            return (storage.TryGetValue(key, out str) && double.TryParse(str, out v)) ?
                v : defaultValue;
        }

        internal static bool Retain(this MyModStorageComponentBase storage, Guid key, string defaultValue)
        {
            if (!storage.ContainsKey(key))
            {
                storage[key] = defaultValue;
                return true;
            }
            else return false;
        }

        internal static DataStorage GetStorage(this MyModStorageComponentBase storage)
        {
            var map = new DataStorage();
            string dataStr;
            if (storage.TryGetValue(IDs.Storage, out dataStr))
            {
                map.Decode(dataStr);
            }
            else
                Logger.Write("BM", "Can't found storage data"); 
            return map;
        }

        internal static void SetStorage(this MyModStorageComponentBase storage, DataStorage map)
        {
            string encoded = map.Encode();
            if (string.IsNullOrWhiteSpace(encoded))
            {
                MyLog.Default.WriteLine("Storage is EMPTY");
                storage.RemoveValue(IDs.Storage);
            }
            else
                storage[IDs.Storage] = encoded;
        }
    }
}

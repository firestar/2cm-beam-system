using System.Collections.Generic;
using BitConverter = System.BitConverter;


namespace BeamSystem.Network
{
    partial class NetworkMessage : Dictionary<byte, double>
    {
        internal long Timestamp;
        private byte[] buffer = null;

        internal byte[] Encode()
        {
            int size = sizeof(long) + (sizeof(byte) + sizeof(double)) * Count;
            if (null == buffer || buffer.Length != size)
                buffer = new byte[size];
            int index = 0;
            var va = BitConverter.GetBytes(Timestamp);
            for (var i = 0; i < va.Length; i++)
                buffer[index++] = va[i];
            using (var enumerator = GetEnumerator())
                while (enumerator.MoveNext())
                {
                    var cur = enumerator.Current;
                    buffer[index++] = cur.Key;
                    va = BitConverter.GetBytes(cur.Value);
                    for (var i = 0; i < va.Length; i++)
                        buffer[index++] = va[i];
                }
            return buffer;
        }

        internal bool Decode(byte[] data)
        {
            try
            {
                int index = 0;
                Timestamp = BitConverter.ToInt64(data, 0);
                index += sizeof(long);
                while (index < data.Length)
                {
                    byte key = data[index++];
                    double value = BitConverter.ToDouble(data, index);
                    index += sizeof(double);
                    this[key] = value;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal bool TryGetValue(byte key, out long value)
        {
            double v;
            if (base.TryGetValue(key, out v))
            {
                value = BitConverter.ToInt64(BitConverter.GetBytes(v), 0);
                return true;
            }
            else
            {
                value = 0L;
                return false;
            }
        }
        internal long GetInt64(byte key, long defaultValue = 0L)
        {
            double v;
            return base.TryGetValue(key, out v)
                ? BitConverter.ToInt64(BitConverter.GetBytes(v), 0)
                : defaultValue;
        }
        internal void Set(byte key, long value)
            => Set(key, BitConverter.ToDouble(BitConverter.GetBytes(value), 0));

        internal int GetInt32(byte key, int defaultValue = 0) => (int)GetInt64(key, defaultValue);
        internal void Set(byte key, int value) => Set(key, (long)value);

        internal double GetDouble(byte key, double defaultValue = 0.0)
        {
            double v;
            return base.TryGetValue(key, out v) ? v : defaultValue;
        }
        internal double Set(byte key, double value) => base[key] = value;

        internal bool TryGetValue(byte key, out float value)
        {
            double v;
            if (base.TryGetValue(key, out v))
            {
                value = (float)v;
                return true;
            }
            else
            {
                value = 0f;
                return false;
            }
        }
        internal float GetSingle(byte key, float defaultValue = 0f)
            => (float)GetDouble(key, (double)defaultValue);
        internal void Set(byte key, float value) => Set(key, (double)value);

        internal bool TryGetValue(byte key, out bool value)
        {
            double v;
            if (base.TryGetValue(key, out v))
            {
                value = v != 0.0;
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }
        internal bool GetBool(byte key, bool defaultValue = false) => GetDouble(key) != 0.0;
        internal void Set(byte key, bool value) => Set(key, value ? 1.0 : 0.0);
    }
}
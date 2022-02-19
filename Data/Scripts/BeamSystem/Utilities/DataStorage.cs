using System;
using System.Collections.Generic;
using BitConverter = System.BitConverter;


namespace BeamSystem
{
    class DataStorage : Dictionary<int, double>
    {
        internal DataStorage()
        {

        }

        internal string Encode()
        {
            if (Count > 0)
            {
                byte[] buffer = new byte[(sizeof(int) + sizeof(double)) * Count];
                int index = 0;
                using (var enumerator = GetEnumerator())
                    while (enumerator.MoveNext())
                    {
                        var cur = enumerator.Current;
                        byte[] va = BitConverter.GetBytes(cur.Key);
                        for (var i = 0; i < va.Length; i++)
                            buffer[index++] = va[i];
                        va = BitConverter.GetBytes(cur.Value);
                        for (var i = 0; i < va.Length; i++)
                            buffer[index++] = va[i];
                    }
                return Convert.ToBase64String(buffer);
            }
            else
                return null;
        }
        internal void Decode(string encodedData)
        {
            try
            {
                byte[] data = Convert.FromBase64String(encodedData);
                int index = 0;
                while (index < data.Length)
                {
                    int key = BitConverter.ToInt32(data, index);
                    index += sizeof(int);
                    double value = BitConverter.ToDouble(data, index);
                    index += sizeof(double);
                    this[key] = value;
                }
            }
            catch (Exception error)
            {
                Logger.Write("GetMap", error.ToString());
            }
        }


        internal bool TryGetValue(int key, out ulong value)
        {
            double v;
            if (base.TryGetValue(key, out v))
            {
                value = BitConverter.ToUInt64(BitConverter.GetBytes(v), 0);
                return true;
            }
            else
            {
                value = 0ul;
                return false;
            }
        }
        internal ulong GetULong(int key, ulong defaultValue = 0ul)
        {
            double v;
            return base.TryGetValue(key, out v)
                ? BitConverter.ToUInt64(BitConverter.GetBytes(v), 0)
                : defaultValue;
        }
        internal void Set(int key, ulong value)
            => Set(key, BitConverter.ToDouble(BitConverter.GetBytes(value), 0));

        internal bool TryGetValue(int key, out long value)
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
        internal long GetLong(int key, long defaultValue = 0L)
        {
            double v;
            return base.TryGetValue(key, out v)
                ? BitConverter.ToInt64(BitConverter.GetBytes(v), 0)
                : defaultValue;
        }
        internal void Set(int key, long value)
            => Set(key, BitConverter.ToDouble(BitConverter.GetBytes(value), 0));

        internal int GetInt(int key, int defaultValue = 0) => (int)GetLong(key, defaultValue);
        internal void Set(int key, int value) => Set(key, (long)value);

        internal double GetDouble(int key, double defaultValue = 0.0)
        {
            double v;
            return base.TryGetValue(key, out v) ? v : defaultValue;
        }
        internal double Set(int key, double value) => base[key] = value;

        internal bool TryGetValue(int key, out float value)
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
        internal float GetSingle(int key, float defaultValue = 0f)
            => (float)GetDouble(key, (double)defaultValue);
        internal void Set(int key, float value) => Set(key, (double)value);

        internal bool TryGetValue(int key, out bool value)
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
        internal bool GetBool(int key, bool defaultValue = false) => GetDouble(key) != 0.0;
        internal void Set(int key, bool value) => Set(key, value ? 1.0 : 0.0);
    }
}
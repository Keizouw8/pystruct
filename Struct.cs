﻿using System.Text;
using System.Text.RegularExpressions;

namespace pystruct;

public class Struct
{
    private delegate byte[] delin<T>(T i);
    private delegate object delout(byte[] i, int o);
    private static delin<object> floatin = (object i) => BitConverter.GetBytes((float)i);
    private static delout floatout = (byte[] i, int o) => BitConverter.ToSingle(i, o);
    private static delin<object> intin = (object i) => BitConverter.GetBytes((int)i);
    private static delout intout = (byte[] i, int o) => BitConverter.ToInt32(i, o);
    private static delin<object> strin = (object i) => Encoding.UTF8.GetBytes((string)i);
    private static delout strout = (byte[] i, int o) => Encoding.UTF8.GetString(i);

    private static readonly Dictionary<string, Tuple<int, delin<object>, delout>> dti = new Dictionary<string, Tuple<int, delin<object>, delout>>()
    {
        { "f", new Tuple<int, delin<object>, delout>(4, floatin, floatout) },
        { "i", new Tuple<int, delin<object>, delout>(4, intin, intout)},
        { "s", new Tuple<int, delin<object>, delout>(1, strin, strout) }
    };

    public static byte[] pack(string datatypes, object[] items)
    {
        string[] dts = Regex.Split(datatypes, $"(?<=[{string.Join("", dti.Keys)}])").Where(s => s != string.Empty).ToArray();
        byte[] result = new byte[0];
        if (dts.Length != items.Length)
        {
            throw new ArgumentException("There are more items than provided datatypes");
        }
        for (int i = 0; i < dts.Length; i++)
        {
            string type = dts[i][dts[i].Length - 1].ToString();
            if (type == "s")
            {
                string origin = ((string)items[i]).Substring(0, dts[i].Length == 1 ? 1 : int.Parse(dts[i].Substring(0, dts[i].Length - 1)));
                if (dts.Length != 1) origin += new string(new char[origin.Length % 4 == 0 ? 0 : 4 - origin.Length % 4]);
                result = result.Concat(dti[type].Item2(origin)).ToArray();
            }
            else
            {
                result = result.Concat(dti[type].Item2(items[i])).ToArray();
            }
        }
        return result;
    }

    public static object[] unpack(string datatypes, byte[] data)
    {
        if (calcsize(datatypes) != data.Length) throw new ArgumentException("Buffer size inconsistent with data");
        string[] dts = Regex.Split(datatypes, $"(?<=[{string.Join("", dti.Keys)}])").Where(s => s != string.Empty).ToArray();
        object[] result = new object[0];
        for (int i = 0; i < dts.Length; i++)
        {
            string type = dts[i][dts[i].Length - 1].ToString();
            if (type == "s")
            {
                int origin = dts[i].Length == 1 ? 1 : int.Parse(dts[i].Substring(0, dts[i].Length - 1));
                Console.WriteLine(Convert.ToHexString(data));
                result = result.Append(dti[type].Item3(data.Take(origin).ToArray(), 0)).ToArray();
                if (dts.Length != 1) origin += origin % 4 == 0 ? 0 : 4 - origin % 4;
                data = data.Skip(origin).ToArray();
            }
            else
            {
                result = result.Append(dti[type].Item3(data.Take(dti[type].Item1).ToArray(), 0)).ToArray();
                data = data.Skip(dti[type].Item1).ToArray();
            }
        }
        return result;
    }

    public static int calcsize(string datatypes)
    {
        string[] dts = Regex.Split(datatypes, $"(?<=[{string.Join("", dti.Keys)}])").Where(s => s != string.Empty).ToArray();
        int total = 0;
        for (int i = 0; i < dts.Length; i++)
        {
            if(dts[i].EndsWith("s"))
            {
                int origin = dts[i].Length == 1 ? 1 : int.Parse(dts[i].Substring(0, dts[i].Length - 1));
                total += dts.Length == 1 ? origin : origin + 4 - origin % 4;
            }
            else
            {
                total += dti[dts[i]].Item1;
            }
        }
        return total;
    }
}
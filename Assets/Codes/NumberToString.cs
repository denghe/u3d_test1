using System;
using System.Text;

public static partial class Helpers {

    public static string[] NumNames = new string[] {
        "",
        "万",
        "亿",
        "兆",
        "京",
        "垓",
        "秭",
        "穰",
        "沟",
        "涧",
        "正",
        "载",
        "极",
        "恒河沙",
        "阿僧祇",
        "那由他",
        "不可思议",
        "无量",
        "大数",
        // ... 继续造？
    };

    public static void ToDrawStringCN(double d, ref StringBuilder o) {
        o.Clear();
        var v = Math.Abs(d);
        var e = (int)Math.Log10(v);
        if (e < 4) {
            o.Length = e + 1;
            var n = (int)v;
            while (n >= 10) {
                var a = n / 10;
                var b = n - a * 10;
                o[e--] = (char)(b + 48);
                n = a;
            }
            o[0] = (char)(n + 48);
        } else {
            var idx = e / 4;
            v /= Math.Pow(10, idx * 4);
            e = e - idx * 4;
            o.Length = e + 1;
            var n = (int)v;
            var bak = n;
            while (n >= 10) {
                var a = n / 10;
                var b = n - a * 10;
                o[e--] = (char)(b + 48);
                n = a;
            }
            o[0] = (char)(n + 48);
            if (v > bak) {
                var first = (int)((v - bak) * 10);
                if (first > 0) {
                    o.Append('.');
                    o.Append((char)(first + 48));
                }
            }
            if (idx < NumNames.Length) {
                o.Append(NumNames[idx]);
            } else {
                o.Append("e+");
                o.Append(idx * 4);
            }
        }
    }

    public static void ToDrawStringEN(double d, ref StringBuilder o) {
        o.Clear();
        var v = Math.Abs(d);
        var e = (int)Math.Log10(v);
        if (e < 3) {
            o.Length = e + 1;
            var n = (int)v;
            while (n >= 10) {
                var a = n / 10;
                var b = n - a * 10;
                o[e--] = (char)(b + 48);
                n = a;
            }
            o[0] = (char)(n + 48);
        } else {
            var idx = e / 3;
            v /= Math.Pow(10, idx * 3);
            e = e - idx * 3;
            o.Length = e + 1;
            var n = (int)v;
            var bak = n;
            while (n >= 10) {
                var a = n / 10;
                var b = n - a * 10;
                o[e--] = (char)(b + 48);
                n = a;
            }
            o[0] = (char)(n + 48);
            if (v > bak) {
                var first = (int)((v - bak) * 10);
                if (first > 0) {
                    o.Append('.');
                    o.Append((char)(first + 48));
                }
            }
            if (idx < 10) {
                o.Append(" KMGTPEZYB"[idx]);
            } else {
                o.Append("e+");
                o.Append(idx * 3);
            }
        }
    }

}

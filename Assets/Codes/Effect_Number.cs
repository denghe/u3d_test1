using UnityEngine;
using UnityEngine.UIElements;

public class Effect_Number {
    public Scene scene;
    public Stage stage;

    public GO[] gos = new GO[12];
    public int size;

    public const float incY = -0.5f / 60 * Scene.fps;
    public const int life = (int)(Scene.fps * 0.5);
    public float x, y, scale;
    public int endLifeTime;

    // todo: color ?
    public Effect_Number(Stage stage_, float x_, float y_, float scale_, int v) {
        stage = stage_;
        scene = stage_.scene;
        x = x_;
        y = y_;
        scale = scale_;
        endLifeTime = scene.time + life;
        stage.effectNumbers.Add(this);

        while (v >= 10) {
            int a = v / 10;
            int b = v - a * 10;
            GOInit(size++, b);
            v = a;
        }
        GOInit(size++, v);
    }

    public void GOInit(int idx, int b) {
        var o = new GO();
        GO.Pop(ref o, 0, "FG2");
        o.r.sprite = scene.sprites_numbers[b];
        o.t.localScale = new Vector3(scale, scale, scale);
        gos[idx] = o;
    }

    public bool Update() {
        y += incY;
        return endLifeTime < scene.time;
    }

    public virtual void Draw(float cx, float cy) {
        if (x < cx - Scene.designWidth_2
        || x > cx + Scene.designWidth_2
        || y < cy - Scene.designHeight_2
        || y > cy + Scene.designHeight_2) {
            for (int i = 0; i < size; ++i) {
                gos[i].Disable();
            }
        } else {
            for (int i = 0; i < size; ++i) {
                gos[i].Enable();
                gos[i].t.position = new Vector3(
                    (x - i * 8 * scale) * Scene.designWidthToCameraRatio
                    , -y * Scene.designWidthToCameraRatio
                    , 0);
            }
        }
    }

    public void Destroy() {
        for (int i = 0; i < size; ++i) {
#if UNITY_EDITOR
            if (gos[i].g != null)
#endif
            {
                GO.Push(ref gos[i]);
            }
        }
    }
}


/*
 
// 下面代码 仅供参考. 应该内联直接算出 sprite 下标

    static string ToDrawString1(double d) {
        var v = Math.Abs(d);
        // 手写麻了 算了
        //if (v >= 1_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000d) return string.Format("{0}载", Math.Round(v / 1_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000d, 1));
        //else if (v >= 1_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000d) return string.Format("{0}正", Math.Round(v / 1_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000d, 1));
        //else if (v >= 1_0000_0000_0000_0000_0000_0000_0000_0000_0000d) return string.Format("{0}涧", Math.Round(v / 1_0000_0000_0000_0000_0000_0000_0000_0000_0000d, 1));
        //else if (v >= 1_0000_0000_0000_0000_0000_0000_0000_0000d) return string.Format("{0}沟", Math.Round(v / 1_0000_0000_0000_0000_0000_0000_0000_0000d, 1));
        //else if (v >= 1_0000_0000_0000_0000_0000_0000_0000d) return string.Format("{0}穰", Math.Round(v / 1_0000_0000_0000_0000_0000_0000_0000d, 1));
        //else if (v >= 1_0000_0000_0000_0000_0000_0000d) return string.Format("{0}秭", Math.Round(v / 1_0000_0000_0000_0000_0000_0000d, 1));
        //else if (v >= 1_0000_0000_0000_0000_0000d) return string.Format("{0}垓", Math.Round(v / 1_0000_0000_0000_0000_0000d, 1));
        //else
        if (v >= 1_0000_0000_0000_0000) return string.Format("{0}京", Math.Round(v / 1_0000_0000_0000_0000, 1));
        else if (v >= 1_0000_0000_0000) return string.Format("{0}兆", Math.Round(v / 1_0000_0000_0000, 1));
        else if (v >= 1_0000_0000) return string.Format("{0}亿", Math.Round(v / 1_0000_0000, 1));
        else if (v >= 1_0000) return string.Format("{0}万", Math.Round(v / 1_0000, 1));
        else return v.ToString();
    }


    static string[] NumNames = new string[] {
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

    static void ToDrawString2(double d, ref StringBuilder o) {
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

    static void ToDrawString3(double d, ref StringBuilder o) {
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


*/

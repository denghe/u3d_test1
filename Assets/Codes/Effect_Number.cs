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
        Debug.Log(v);

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
        GO.Pop(ref o, "FG2");
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

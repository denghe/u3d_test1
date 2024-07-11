using UnityEngine;

public class PlayerBullet1 {
    public Scene scene;                                 // 指向场景
    public Stage stage;                                 // 指向关卡
    public Sprite sprite;                               // 指向动画帧

    public GO go;                                       // 保存底层 u3d 资源

    public const float displayScale = 1f;               // 显示放大修正
    public const float defaultRadius = 5f;              // 原始半径

    public float moveSpeed = 2;                         // 当前每帧移动距离
    public float radius = defaultRadius;                // 半径
    public float x, y;                                  // grid中的坐标
    public float radians;                               // 弧度
    public float incX, incY;                            // 每帧的移动增量
    public int lifeEndTime;                             // 

    public PlayerBullet1(Stage stage_, Sprite sprite_, float x_, float y_, float radians_, int life) {
        // 各种基础初始化
        stage = stage_;
        scene = stage_.scene;
        stage.playerBullets.Add(this);
        sprite = sprite_;

        // 从对象池分配 u3d 底层对象
        GO.Pop(ref go);

        // 初始化数据
        x = x_;
        y = y_;
        radians = radians_;
        lifeEndTime = life + scene.time;

        // 根据角度计算移动增量
        incX = Mathf.Cos(radians_) * moveSpeed;
        incY = Mathf.Sin(radians_) * moveSpeed;
    }

    public virtual bool Update() {

        x += incX;
        y += incY;
        // todo: 防范挪动到超出 grid地图 范围

        // 碰撞检测
        var crIdx = scene.spaceContainer.PosToCrIdx(x, y);
        int limit = 100;
        scene.spaceContainer.Foreach9NeighborCells(crIdx, ref limit, o => {
            var m = o as Monster;
            var dx = x - m.spaceX;
            var dy = y - m.spaceY;
            var dd = dx * dx + dy * dy;
            var r = radius + m.radius;
            Debug.Log($"b xy = {x} {y}  m xy = {m.spaceX} {m.spaceY}");
            if (dd < r * r) {
                m.Destroy();
                limit = -1;
            }
        });
        if (limit <= -1) {
            return true;
        }

        return lifeEndTime < scene.time;
    }

    public virtual void Draw(float cx, float cy) {
        if (x < cx - Scene.designWidth_2
            || x > cx + Scene.designWidth_2
            || y < cy - Scene.designHeight_2
            || y > cy + Scene.designHeight_2) {
            go.g.SetActive(false);
        } else {
            go.g.SetActive(true);

            // 同步帧
            go.r.sprite = sprite;

            // 同步 & 坐标系转换( y 坐标需要反转 )
            go.t.position = new Vector3(x * Scene.designWidthToCameraRatio, -y * Scene.designWidthToCameraRatio, 0);
            go.t.localScale = new Vector3(displayScale, displayScale, displayScale);
            go.t.rotation = Quaternion.Euler(0, 0, -radians * (180f / Mathf.PI));
        }
    }

    public virtual void DrawGizmos() {
        Gizmos.DrawWireSphere(new Vector3(x * Scene.designWidthToCameraRatio, -y * Scene.designWidthToCameraRatio, 0), radius * Scene.designWidthToCameraRatio);
    }

    public virtual void Destroy() {
#if UNITY_EDITOR
        if (go.g != null)           // unity 点击停止按钮后，这些变量似乎有可能提前变成 null
#endif
        {
            // 将 u3d 底层对象返回池
            GO.Push(ref go);
        }
    }
}

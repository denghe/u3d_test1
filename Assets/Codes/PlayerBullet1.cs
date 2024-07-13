using UnityEngine;

public class PlayerBullet1 {
    public Scene scene;                                 // 指向场景
    public Stage stage;                                 // 指向关卡

    public GO go;                                       // 保存底层 u3d 资源

    public const float displayBaseScale = 1f;           // 显示放大修正
    public const float defaultRadius = 5f;              // 原始半径
    public const float _1_defaultRadius = 1f / defaultRadius;

    public float moveSpeed = 2;                         // 当前每帧移动距离
    public float radius = defaultRadius;                // 半径
    public float x, y;                                  // grid中的坐标
    public float incX, incY;                            // 每帧的移动增量
    public int lifeEndTime;                             // 自杀时间点

    // todo: damage 穿刺 支持

    public PlayerBullet1(Stage stage_, Sprite sprite_, float x_, float y_, float radians_, int life_) {
        // 各种基础初始化
        stage = stage_;
        scene = stage_.scene;
        stage.playerBullets.Add(this);

        // 从对象池分配 u3d 底层对象
        GO.Pop(ref go);
        go.r.sprite = sprite_;
        go.t.rotation = Quaternion.Euler(0, 0, -radians_ * (180f / Mathf.PI));
        x = x_;
        y = y_;
        lifeEndTime = life_ + scene.time;
        // 根据角度计算移动增量
        incX = Mathf.Cos(radians_) * moveSpeed;
        incY = Mathf.Sin(radians_) * moveSpeed;
    }

    public virtual bool Update() {

        // 让子弹直线移动
        x += incX;
        y += incY;

        // 坐标超出 grid地图 范围: 自杀
        if (x < 0 || x >= Stage.gridWidth || y < 0 || y >= Stage.gridHeight) return true;

        // 在 9 宫范围内查询 首个相交
        var o = stage.monstersSpaceContainer.Foreach9FirstHitCheck(x, y, radius);
        if (o != null) {

            // todo: 令怪减血, 一段时间变白? 可能需要修改 shader 增加一个颜色乘法

            new Effect_Explosion(stage, o.x, o.y, radius * _1_defaultRadius);
            ((Monster)o).Destroy();
            return true;
        }

        return lifeEndTime < scene.time;
    }

    public virtual void Draw(float cx, float cy) {
        if (x < cx - Scene.designWidth_2
            || x > cx + Scene.designWidth_2
            || y < cy - Scene.designHeight_2
            || y > cy + Scene.designHeight_2) {
            go.Disable();
        } else {
            go.Enable();

            // 同步 & 坐标系转换( y 坐标需要反转 )
            go.t.position = new Vector3(x * Scene.designWidthToCameraRatio, -y * Scene.designWidthToCameraRatio, 0);

            // 根据半径同步缩放
            var s = displayBaseScale * radius * _1_defaultRadius;
            go.t.localScale = new Vector3(s, s, s);
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

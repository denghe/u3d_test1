using UnityEngine;

public class Player {
    public Scene scene;                                 // 指向场景
    public IStage stage;                                // 指向关卡
    public Sprite[] sprites;                            // 指向动画帧集合

    public GO go;                                       // 保存底层 u3d 资源

    public const float defaultMoveSpeed = 20;           // 原始移动速度
    public const float _1_defaultMoveSpeed = 1f / defaultMoveSpeed;
    public const float frameAnimIncrease = 1f / 5;      // 帧动画前进速度( 针对 defaultMoveSpeed )
    public float frameIndex = 0;                        // 当前动画帧下标
    public bool flipX;                                  // 根据移动方向判断要不要反转 x 显示
    public float lastMoveValueX;                        // 备份，用来判断移动方向，要不要反转 x 显示

    public float moveSpeed = 20;                        // 当前每帧移动距离
    public float x, y;

    public Player(IStage stage_, Sprite[] sprites_, float x_, float y_) {
        // 各种基础初始化
        stage = stage_;
        scene = stage_.scene;
        sprites = sprites_;
        lastMoveValueX = scene.playerMoveValue.x;

        // 从对象池分配 u3d 底层对象
        GO.Pop(ref go);

        // 设置坐标
        x = x_;
        y = y_;
    }

    public virtual bool Update() {

        // 玩家控制移动
        if (scene.playerMoving) {
            var mv = scene.playerMoveValue;
            x += mv.x * moveSpeed;
            y += mv.y * moveSpeed;

            // 判断动画绘制方向
            if (lastMoveValueX != mv.x) {
                flipX = mv.x < 0;
                lastMoveValueX = mv.x;
            }

            // 根据移动速度步进动画帧下表
            frameIndex += frameAnimIncrease * moveSpeed * _1_defaultMoveSpeed;
            var len = sprites.Length;
            if (frameIndex >= len) {
                frameIndex -= len;
            }
        }

        // todo: 防范挪动到超出 grid地图 范围

        return false;
    }

    public virtual void Draw() {
        // 同步帧下标
        go.r.sprite = sprites[(int)frameIndex];

        // 同步反转状态
        go.r.flipX = flipX;

        // 同步 & 坐标系转换( y 坐标需要反转 )
        go.t.position = new Vector3(x * Scene.designWidthToCameraRatio, -y * Scene.designWidthToCameraRatio, 0);
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

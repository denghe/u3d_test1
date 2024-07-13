using System.Collections.Generic;
using UnityEngine;

public class Player {
    public Scene scene;                                 // 指向场景
    public Stage stage;                                 // 指向关卡
    public Sprite[] sprites;                            // 指向动画帧集合

    public GO go;                                       // 保存底层 u3d 资源

    public const float defaultMoveSpeed = 20;           // 原始移动速度
    public const float _1_defaultMoveSpeed = 1f / defaultMoveSpeed;
    public const float frameAnimIncrease = 1f / 5;      // 帧动画前进速度( 针对 defaultMoveSpeed )
    public const float displayScale = 2f;               // 显示放大修正
    public const float defaultRadius = 13f;             // 原始半径

    public float frameIndex = 0;                        // 当前动画帧下标
    public bool flipX;                                  // 根据移动方向判断要不要反转 x 显示

    public float radius = defaultRadius;                // 该数值和玩家体积同步
    public float x, y;                                  // position
    public List<Vector2> positionHistory = new();       // 历史坐标数组    // todo: 需要自己实现一个 ring buffer 避免 move
    public float radians {                              // 俯视角度下的角色 前进方向 弧度 ( 可理解为 朝向 )
        get {
            return Mathf.Atan2(scene.playerDirection.y, scene.playerDirection.x);
        }
    }
    public int quitInvincibleTime;                      // 退出无敌状态的时间点


    public float moveSpeed = 20;                        // 当前每帧移动距离

    public int hp = 100;                                // 当前血量
    public int maxHp = 100;                             // 血上限
    public int damage = 10;                             // 当前基础伤害倍率( 技能上面为实际伤害值 )
    public int defense = 10;                            // 防御力
    public float criticalRate = 0.05f;                  // 暴击率
    public float criticalDamageRatio = 1.5f;            // 暴击伤害倍率
    public float dodgeRate = 0.05f;                     // 闪避率
    public int getHurtInvincibleTimeSpan = 6;           // 受伤短暂无敌时长( 帧 )
    public List<PlayerSkill> skills = new();            // 玩家技能数组

    public Player(Stage stage_, Sprite[] sprites_, float x_, float y_) {
        // 各种基础初始化
        stage = stage_;
        scene = stage_.scene;
        sprites = sprites_;

        // 从对象池分配 u3d 底层对象
        GO.Pop(ref go);
        go.Enable();

        // 设置坐标
        x = x_;
        y = y_;

        // 先给自己创建一些初始技能
        skills.Add(new PlayerSkill(this).Init());
    }

    public bool Update() {

        // 玩家控制移动
        if (scene.playerMoving) {
            var mv = scene.playerMoveValue;
            x += mv.x * moveSpeed;
            y += mv.y * moveSpeed;

            // 判断绘制 x 坐标要不要翻转
            if (flipX && mv.x > 0) {
                flipX = false;
            } else if (mv.x < 0) {
                flipX = true;
            }

            // 根据移动速度步进动画帧下表
            frameIndex += frameAnimIncrease * moveSpeed * _1_defaultMoveSpeed;
            var len = sprites.Length;
            if (frameIndex >= len) {
                frameIndex -= len;
            }
        }

        // 强行限制移动范围
        if (x < 0) x = 0;
        else if (x >= Stage.gridWidth) x = Stage.gridWidth - float.Epsilon;
        if (y < 0) y = 0;
        else if (y >= Stage.gridHeight) y = Stage.gridHeight - float.Epsilon;

        // 将坐标写入历史记录( 限定长度 )
        positionHistory.Insert(0, new Vector2(x, y));
        if (positionHistory.Count > 60) {
            positionHistory.RemoveAt(positionHistory.Count - 1);
        }

        // 驱动技能
        foreach (var skill in skills) {
            skill.Update();
        }

        return false;
    }

    public void Draw() {
        // 同步帧下标
        go.r.sprite = sprites[(int)frameIndex];

        // 同步反转状态
        go.r.flipX = flipX;

        // 同步 & 坐标系转换( y 坐标需要反转 )
        go.t.position = new Vector3(x * Scene.designWidthToCameraRatio, -y * Scene.designWidthToCameraRatio, 0);
        go.t.localScale = new Vector3(displayScale, displayScale, displayScale);
    }

    public void DrawGizmos() {
        Gizmos.DrawWireSphere(new Vector3(x * Scene.designWidthToCameraRatio, -y * Scene.designWidthToCameraRatio, 0), radius * Scene.designWidthToCameraRatio);
    }

    public void Destroy() {
        foreach (var skill in skills) {
            skill.Destroy();
        }
#if UNITY_EDITOR
        if (go.g != null)           // unity 点击停止按钮后，这些变量似乎有可能提前变成 null
#endif
        {
            // 将 u3d 底层对象返回池
            GO.Push(ref go);
        }
    }
}

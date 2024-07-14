using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet {
    // 快捷指向
    public Scene scene;
    public Stage stage;
    public Player player;
    public PlayerSkill skill;
    public SpaceContainer monstersSpaceContainer;

    public GO go;                                   // 保存底层 u3d 资源

    public const float displayBaseScale = 1f;       // 显示放大修正
    public const float defaultRadius = 5f;          // 原始半径
    public const float _1_defaultRadius = 1f / defaultRadius;
    public const float shootDistance = 15;          // 发射时与身体的距离

    public float x, y, radians;                     // 坐标, 弧度
    public float incX, incY;                        // 每帧的移动增量
    public int lifeEndTime;                         // 自杀时间点
    public List<KeyValuePair<SpaceItem, int>> hitBlackList = new();   // 带超时的穿透黑名单

    // 这些属性从 skill copy
    public float radius;                            // 碰撞检测半径( 和显示放大修正配套 )
    public int damage;                              // 伤害( 倍率 )
    public float moveSpeed;                         // 按照 fps 来算的每一帧的移动距离
    public int life;                                // 子弹存在时长( 帧 ): fps * 3 秒
    public int pierceCount;                         // 最大可穿透次数
    public int pierceDelay;                         // 穿透时间间隔 帧数( 针对相同目标 )
    public int knockbackForce;                      // 击退强度( 退多少帧, 多远 )

    public PlayerBullet(PlayerSkill ps) {
        skill = ps;
        player = ps.player;
        stage = ps.stage;
        scene = ps.scene;
        monstersSpaceContainer = stage.monstersSpaceContainer;
        stage.playerBullets.Add(this);

        // 属性复制
        radius = ps.radius;
        damage = ps.damage;
        moveSpeed = ps.moveSpeed;
        life = ps.life;
        pierceCount = ps.pierceCount;
        pierceDelay = ps.pierceDelay;
        knockbackForce = ps.knockbackForce;
        // ...
    }

    public PlayerBullet Init(float x_, float y_, float radians_) {
        // 从对象池分配 u3d 底层对象
        GO.Pop(ref go);
        go.r.sprite = scene.sprites_bullets[1];
        go.t.rotation = Quaternion.Euler(0, 0, -radians_ * (180f / Mathf.PI));

        lifeEndTime = life + scene.time;
        radians = radians_;
        var cos = Mathf.Cos(radians_);
        var sin = Mathf.Sin(radians_);
        x = x_ + cos * shootDistance;
        y = y_ + sin * shootDistance;
        incX = cos * moveSpeed;
        incY = sin * moveSpeed;

        return this;
    }

    public virtual bool Update() {

        // 维护 超时黑名单. 这步先把超时的删光
        var now = scene.time;
        var newTIme = now + pierceDelay;
        for (var i = hitBlackList.Count - 1; i >= 0; --i) {
            if (hitBlackList[i].Value < now) {
                var lastIndex = hitBlackList.Count - 1;
                hitBlackList[i] = hitBlackList[lastIndex];
                hitBlackList.RemoveAt(lastIndex);
            }
        }

        if (pierceCount <= 1) {
            // 在 9 宫范围内查询 首个相交
            var m = monstersSpaceContainer.Foreach9FirstHitCheck(x, y, radius);
            if (m != null) {
                HurtMonster((Monster)m);
                return true;    // 和怪一起死
            }
        } else {
            // 遍历九宫 挨个处理相交, 消耗 穿刺
            monstersSpaceContainer.Foreach9All(x, y, HitCheck);
            if (pierceCount <= 0) return true;
        }

        // 让子弹直线移动
        x += incX;
        y += incY;

        // 坐标超出 grid地图 范围: 自杀
        if (x < 0 || x >= Stage.gridWidth || y < 0 || y >= Stage.gridHeight) return true;

        // 生命周期完结: 自杀
        return lifeEndTime < scene.time;
    }

    public void HurtMonster(Monster m) {
        new Effect_Explosion(stage, m.x, m.y, radius * _1_defaultRadius);
        ((Monster)m).Destroy();
    }

    public bool HitCheck(SpaceItem m) {
        var vx = m.x - x;
        var vy = m.y - y;
        var r = m.radius + radius;
        if (vx * vx + vy * vy < r * r) {

            // 判断当前怪有没有存在于 超时黑名单
            var listLen = hitBlackList.Count;
            for (var i = 0; i < listLen; ++i) {
                if (hitBlackList[i].Key == m) return false;     // 存在: 不产生伤害, 继续遍历下一只怪
            }

            // 不存在：加入列表
            hitBlackList.Add(new KeyValuePair<SpaceItem, int>(m, scene.time + pierceDelay));

            // 伤害怪
            HurtMonster((Monster)m);

            // 如果穿刺计数 已用光，停止遍历
            if (this.pierceCount-- == 0) {
                // 放点特效?
                return true;
            }
        }
        // 未命中：继续遍历下一只怪
        return false;
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

    public void Destroy() {
#if UNITY_EDITOR
        if (go.g != null)           // unity 点击停止按钮后，这些变量似乎有可能提前变成 null
#endif
        {
            // 将 u3d 底层对象返回池
            GO.Push(ref go);
        }
    }
}

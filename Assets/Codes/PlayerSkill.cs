using System.Text;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class PlayerSkill {
    // 快捷指向
    public Scene scene;
    public Stage stage;
    public Player player;

    public int icon = 123;                                  // 用于 UI 展示
    public float progress = 1;                              // 同步以展示技能就绪进度( 结合 nextCastTime castDelay 来计算 )

    public const float maxShootDistance = 50;               // 子弹发射时与身体的 最大距离
    public float castDelaySeconds = 0.2f;                   // 技能冷却时长
    public int castCount = 3;                               // 一次发射多少颗
    public int nextCastTime = 0;                            // 下一次施展的时间点

    // 创建子弹时，复制到子弹上
    public float radius = 20;                               // 碰撞检测半径( 和显示放大修正配套 )
    public int damage = 1;                                  // 伤害( 倍率 )
    public float moveSpeed = 25;                            // 按照 fps 来算的每一帧的移动距离
    public int life = Scene.fps * 1;                        // 子弹存在时长( 帧 ): fps * 秒
    public int pierceCount = 5;                             // 最大可穿透次数
    public int pierceDelay = 12;                            // 穿透时间间隔 帧数( 针对相同目标 )
    public int knockbackForce = 0;                          // 击退强度( 退多少帧, 多远 )

    public PlayerSkill(Stage stage_) {
        stage = stage_;
        scene = stage_.scene;
        player = scene.player;
    }

    public PlayerSkill Init() {
        return this;
    }

    public virtual void Update() {
        var castDelay = (int)(Scene.fps * castDelaySeconds);

        // 子弹发射逻辑
        var time = scene.time;
        if (nextCastTime < time) {
            nextCastTime = time + castDelay;
            progress = 0;

            var x = player.x;
            var y = player.y;


            // 找射程内 距离最近的 最多 castCount 只 分别朝向其发射子弹
            // 如果不足 castCount 只，轮流扫射，直到用光 castCount 发
            // 0 只 就面对朝向发射
            // 发射时和 player 保持一个距离，同时随着 count 的减少，距离也变短，以解决同一帧内同一角度发射多粒 完全重叠在一起看不出来的问题

            var shootDistanceStep = maxShootDistance / castCount;
            var count = castCount;
            var sc = stage.monstersSpaceContainer;
            var os = sc.result_FindNearestN;
            var n = sc.FindNearestNByRange(Scene.spaceRDD, x, y, moveSpeed * life, count);

            if (n > 0) {
                do {
                    for (int i = 0; i < n; ++i) {
                        var o = os[i].item;
                        var dy = o.y - y;
                        var dx = o.x - x;
                        var r = Mathf.Atan2(dy, dx);
                        var cos = Mathf.Cos(r);
                        var sin = Mathf.Sin(r);
                        var tarX = x + cos * shootDistanceStep * count;
                        var tarY = y + sin * shootDistanceStep * count;
                        new PlayerBullet(this).Init(tarX, tarY, r, cos, sin);
                        --count;
                        if (count == 0) break;
                    }
                } while (count > 0);
            } else {
                var d = scene.playerDirection;
                var r = Mathf.Atan2(d.y, d.x);
                var cos = Mathf.Cos(r);
                var sin = Mathf.Sin(r);
                for (; count > 0; --count) {
                    var tarX = x + cos * shootDistanceStep * count;
                    var tarY = y + sin * shootDistanceStep * count;
                    new PlayerBullet(this).Init(tarX, tarY, r, cos, sin);
                }
            }
        } else {
            progress = 1 - (nextCastTime - time) / castDelay;
        }
    }

    public virtual void Destroy() {
    }
}


/*
    var sb = new StringBuilder();
    sb.Append($"n = {n} [ ");
    for (int i = 0; i < n; ++i) {
        var d = os[i].distance;
        sb.Append($"{d}, ");
    }
    sb.Append("]");
    Debug.Log(sb.ToString());
*/

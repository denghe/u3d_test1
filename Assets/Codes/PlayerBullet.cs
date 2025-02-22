﻿using System.Collections.Generic;
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
    public const float defaultRadius = 7f;          // 原始半径
    public const float _1_defaultRadius = 1f / defaultRadius;

    public float x, y, radians;                     // 坐标, 弧度
    public float incX, incY;                        // 每帧的移动增量
    public int lifeEndTime;                         // 自杀时间点

    // 这些属性从 skill copy
    public float radius;                            // 碰撞检测半径( 和显示放大修正配套 )
    public int damage;                              // 伤害( 倍率 )
    public float moveSpeed;                         // 按照 fps 来算的每一帧的移动距离
    public int life;                                // 子弹存在时长( 帧 ): fps * 秒

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
        // ...
    }

    public PlayerBullet Init(float x_, float y_, float radians_, float cos_, float sin_) {
        // 从对象池分配 u3d 底层对象
        GO.Pop(ref go);
        go.r.sprite = scene.sprites_bullets[1];
        go.t.rotation = Quaternion.Euler(0, 0, -radians_ * (180f / Mathf.PI));

        lifeEndTime = life + scene.time;
        radians = radians_;
        x = x_;
        y = y_;
        incX = cos_ * moveSpeed;
        incY = sin_ * moveSpeed;

        return this;
    }

    public virtual bool Update() {

        // 在 9 宫范围内查询 首个相交
        var m = monstersSpaceContainer.FindFirstCrossBy9(x, y, radius);
        if (m != null) {
            ((Monster)m).Hurt(damage, 0);
            return true;    // 销毁自己
        }

        // 让子弹直线移动
        x += incX;
        y += incY;

        // 坐标超出 grid地图 范围: 自杀
        if (x < 0 || x >= Scene.gridWidth || y < 0 || y >= Scene.gridHeight) return true;

        // 生命周期完结: 自杀
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

    public void Destroy() {
#if UNITY_EDITOR
        if (go.g != null)           // unity 点击停止按钮后，这些变量似乎有可能提前变成 null
#endif
        {
            // 将 u3d 底层对象返回池
            GO.Push(ref go);
        }
        //hitBlackList = null;
    }
}

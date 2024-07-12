using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

public class Stage {

    // 大地图格子数量
    internal const int numRows = 1024, numCols = 1024;

    // 大地图总宽度
    internal const float gridWidth = numCols * Scene.cellSize;

    // 大地图总高度
    internal const float gridHeight = numRows * Scene.cellSize;

    // 大地图中心点坐标
    internal const float gridWidth_2 = gridWidth / 2, gridHeight_2 = gridHeight / 2;
    internal const float gridCenterX = gridWidth_2, gridCenterY = gridHeight_2;

    public Scene scene { get; }
    public List<Sprite[]> spritess = new();
    public int state = 0;

    public Transform camTrans;  // cache
    public Player player;
    public List<PlayerBullet1> playerBullets = new();
    public List<Monster> monsters = new();
    internal SpaceContainer monstersSpaceContainer;


    public Stage(Scene scene) {
        this.scene = scene;
        monstersSpaceContainer = new(numRows, numCols, Scene.cellSize);
        camTrans = Camera.main.transform;
    }

    public bool MonstersUpdate() {
        var os = monsters;
        for (int i = os.Count - 1; i >= 0; i--) {
            var o = os[i];
            if (o.Update()) {
                o.Destroy();    // 会从 monsters 自动移除自己
            }
        }
        return os.Count > 0;
    }

    public bool PlayerBulletsUpdate() {
        var os = playerBullets;
        for (int i = os.Count - 1; i >= 0; i--) {
            var o = os[i];
            if (o.Update()) {
                os.RemoveAtSwapBack(i); // 从数组移除
                o.Destroy();    // 并不会自动从数组移除
            }
        }
        return os.Count > 0;
    }

    public void Update() {
        switch (state) {
            case 0:
                State0();
                return;
            case 1:
                State1();
                return;
            default:
                throw new Exception("???");
        }
    }

    public void State0() {

        // 利用反射来读取 Scene 里面的怪物配置
        var st = typeof(Scene);
        var fs = st.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var f in fs) {
            if (f.FieldType.Name == "Sprite[]") {
                if (f.Name.StartsWith("sprites_monster")) {
                    var ss = f.GetValue(scene) as Sprite[];
                    if (ss.Length > 0) {
                        spritess.Add(ss);
                    }
                }
            }
        }

        // 每一种创建 ?? 只
        foreach (var ss in spritess) {
            for (int i = 0; i < 5000; i++) {
                var x = gridCenterX + UnityEngine.Random.Range(-Scene.designWidth_2, Scene.designWidth_2);
                var y = gridCenterY + UnityEngine.Random.Range(-Scene.designHeight_2, Scene.designHeight_2);
                new Monster(this, ss, x, y);
            }
        }

        // 创建 Player
        player = new(this, scene.sprites_player, gridCenterX, gridCenterY);

        // 初始化 camera 位置, 令其指向 大地图 中心点( y 坐标需要反转 )
        camTrans.position = new Vector3(gridCenterX * Scene.designWidthToCameraRatio, -gridCenterY * Scene.designWidthToCameraRatio, camTrans.position.z);

        state = 1;
    }

    public void State1() {
        MonstersUpdate();
        PlayerBulletsUpdate();
        player.Update();
    }

    public void Draw() {
        // 同步 camera 的位置
        camTrans.position = new Vector3(player.x * Scene.designWidthToCameraRatio, -player.y * Scene.designWidthToCameraRatio, camTrans.position.z);

        // 剔除 & 同步 GO
        var cx = player.x;
        var cy = player.y;
        foreach (var o in monsters) {
            o.Draw(cx, cy);
        }
        foreach (var o in playerBullets) {
            o.Draw(cx, cy);
        }
        player.Draw();
    }

    public void DrawGizmos() {
        foreach (var o in monsters) {
            o.DrawGizmos();
        }
        foreach (var o in playerBullets) {
            o.DrawGizmos();
        }
        player.DrawGizmos();
    }

    public void Destroy() {
        foreach (var o in monsters) {
            o.Destroy(false);             // 纯 destroy，不从 monsters 移除自己
        }
        monsters.Clear();
        Debug.Assert(monstersSpaceContainer.numItems == 0);
        foreach (var o in playerBullets) {
            o.Destroy();
        }
        monsters.Clear();
        player.Destroy();
        player = null;
    }
}

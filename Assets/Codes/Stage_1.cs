using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Stage_1 : IStage {
    public Scene scene { get; }
    public int state = 0;

    public Player player;
    public List<Monster> monsters = new();
    public List<Sprite[]> spritess = new();
    public Transform camTrans;  // cache

    public Stage_1(Scene scene) {
        this.scene = scene;
        camTrans = Camera.main.transform;
    }

    public bool MonstersUpdate() {
        for (int i = monsters.Count - 1; i >= 0; i--) {
            var coin = monsters[i];
            if (coin.Update()) {
                var lastIndex = monsters.Count - 1;
                monsters[i] = monsters[lastIndex];
                monsters.RemoveAt(lastIndex);
            }
        }
        return monsters.Count > 0;
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
                if (f.Name != "sprites_player") {
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
                var x = Scene.gridCenterX + UnityEngine.Random.Range(-Scene.designWidth_2, Scene.designWidth_2);
                var y = Scene.gridCenterY + UnityEngine.Random.Range(-Scene.designHeight_2, Scene.designHeight_2);
                monsters.Add(new Monster(this, ss, x, y));
            }
        }

        // 创建 Player
        player = new(this, scene.sprites_player, Scene.gridCenterX, Scene.gridCenterY);

        // 初始化 camera 位置, 令其指向 大地图 中心点( y 坐标需要反转 )
        camTrans.position = new Vector3(Scene.gridCenterX * Scene.designWidthToCameraRatio, -Scene.gridCenterY * Scene.designWidthToCameraRatio, camTrans.position.z);

        state = 1;
    }

    public void State1() {
        MonstersUpdate();
        player.Update();
    }

    public void Draw() {
        // 同步 camera 的位置
        camTrans.position = new Vector3(player.x * Scene.designWidthToCameraRatio, -player.y * Scene.designWidthToCameraRatio, camTrans.position.z);

        // todo:

        foreach (var monster in monsters) {
            monster.Draw();
        }
        player.Draw();
    }

    public void Destroy() {
        foreach (var monster in monsters) {
            monster.Destroy();
        }
        monsters.Clear();
        player.Destroy();
        player = null;
    }
}

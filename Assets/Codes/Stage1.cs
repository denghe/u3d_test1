using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Stage1 : Stage {
    public int timeout;

    public Stage1(Scene scene) : base(scene) {
        // 这里可判断是不是 切关, 然后对 player 或啥的做相应处理
    }

    public override void Update() {
        switch (state) {
            case 0:
                P0();
                return;
            case 1:
                P1();
                return;
            case 2:
                P2();
                return;
            default:
                throw new System.Exception("can't be here");
        }
    }

    public void P0() {

        // 配置怪生成器
        var time = scene.time;
        monsterGenerators.Add(new MonsterGenerator1("stage0_1", this, time + 0, time + 1000, 20));
        monsterGenerators.Add(new MonsterGenerator2("stage0_2", this, time + 1000, time + 2000, 20));

        // 重置 Player 坐标
        player.Init(this, gridCenterX, gridCenterY);

        // 初始化 camera 位置, 令其指向 大地图 中心点( y 坐标需要反转 )
        camTrans.position = new Vector3(gridCenterX * Scene.designWidthToCameraRatio, -gridCenterY * Scene.designWidthToCameraRatio, camTrans.position.z);

        state = 1;
    }

    public void P1() {
        ExplosionsUpdate();
        MonstersUpdate();
        if (MonstersGeneratorsUpdate() == 0) {      // 怪生成器 已经没了
            timeout = scene.time + Scene.fps * 5;   // 设置 5 秒超时
            state = 2;
        }
        PlayerBulletsUpdate();
        player.Update();
    }

    public void P2() {
        ExplosionsUpdate();
        MonstersUpdate();
        PlayerBulletsUpdate();
        player.Update();
        if (timeout < scene.time) {
            scene.SetStage(new Stage1(scene));      // 已超时：切到新关卡
        }
    }

}

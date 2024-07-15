using System.Collections.Generic;
using Unity.VisualScripting;
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
            default:
                throw new System.Exception("can't be here");
        }
    }

    public void P0() {
        var cx = Scene.gridCenterX;
        var cy = Scene.gridCenterY;

        // 重置 Player 坐标
        player.Init(this, cx, cy);

        //// 验证一下这个表的数据是否正确
        //var d = Scene.spaceRDD;
        //foreach (var i in d.idxs) {
        //    // 根据 格子 offset 计算 x, y 并设置怪的坐标
        //    new Monster1(this).Init(cx + i.x * 16, cy + i.y * 16).radius = 5;
        //}

        state = 1;
    }

    public void P1() {
        Update_Effect_Numbers();
        Update_Player();

        // 测试一下伤害数字的效果
        for (int i = 0; i < 50; i++) {
            var x = Random.Range(-Scene.designWidth_2, Scene.designWidth_2);
            var y = Random.Range(-Scene.designHeight_2, Scene.designHeight_2);
            var v = Random.Range(0, 9999);
            new Effect_Number(this, player.x + x, player.y + y, 2, v);
        }
    }

}

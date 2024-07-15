using UnityEngine;

public class Stage2 : Stage {
    public int timeout;

    public Stage2(Scene scene) : base(scene) {
        // 这里可判断是不是 切关, 然后对 player 或啥的做相应处理

        // 先给自己创建一些初始技能
        // todo: 通过配置来创建. 纯技能并没有什么意义，只有进了关卡之后，才能实例化，开始工作. 也就是说，技能依附于关卡存在。
        // 玩家在游戏过程中，技能可能会 增加，成长，都应该写进 配置。 这样切换关卡后，可以根据配置 再次创建技能
        player.skills.Add(new PlayerSkill(this).Init());
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
        monsterGenerators.Add(new MonsterGenerator1("stage0_1", this, time + Scene.fps * 0, time + Scene.fps * 10, 1));
        monsterGenerators.Add(new MonsterGenerator2("stage0_2", this, time + Scene.fps * 10, time + Scene.fps * 20, 0));

        // 重置 Player 坐标
        player.Init(this, Scene.gridCenterX, Scene.gridCenterY);

        state = 1;
    }

    public void P1() {
        Update_Effect_Explosions();
        Update_Effect_Numbers();
        Update_Monsters();
        if (Update_MonstersGenerators() == 0) {      // 怪生成器 已经没了
            timeout = scene.time + Scene.fps * 5;   // 设置 5 秒超时
            state = 2;
        }
        Update_PlayerBullets();
        player.Update();
    }

    public void P2() {
        Update_Effect_Explosions();
        Update_Effect_Numbers();
        Update_Monsters();
        Update_PlayerBullets();
        player.Update();
        if (timeout < scene.time) {
            scene.SetStage(new Stage1(scene));      // 已超时：切到新关卡
        }
    }

}

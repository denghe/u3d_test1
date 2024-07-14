using System.Collections.Generic;
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


    /*************************************************************************************************************************/
    /*************************************************************************************************************************/

    // 各种引用
    public Scene scene;
    public Player player;
    public int state;
    public Transform camTrans;

    public List<PlayerBullet> playerBullets = new();
    public List<Monster> monsters = new();
    public SpaceContainer monstersSpaceContainer;
    public List<Effect_Explosion> effectExplosions = new();
    public List<MonsterGenerator> monsterGenerators = new();


    /*************************************************************************************************************************/
    /*************************************************************************************************************************/

    public Stage(Scene scene_) {
        scene = scene_;
        player = scene_.player;
        monstersSpaceContainer = new(numRows, numCols, Scene.cellSize);
        camTrans = Camera.main.transform;
    }


    // 派生类需要覆盖
    public virtual void Update() {
        throw new System.Exception("need impl");
    }


    public virtual void Draw() {
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
        foreach (var o in effectExplosions) {
            o.Draw(cx, cy);
        }
        player.Draw();
    }


    public virtual void DrawGizmos() {
        foreach (var o in monsters) {
            o.DrawGizmos();
        }
        foreach (var o in playerBullets) {
            o.DrawGizmos();
        }
        player.DrawGizmos();
    }


    public virtual void Destroy() {
        foreach (var o in monsters) {
            o.Destroy(false);             // 纯 destroy，不从 monsters 移除自己
        }
        foreach (var o in playerBullets) {
            o.Destroy();
        }
        foreach (var o in effectExplosions) {
            o.Destroy();
        }
        foreach (var o in monsterGenerators) {
            o.Destroy();
        }
        player.Destroy();
        // ...

        Debug.Assert(monstersSpaceContainer.numItems == 0);
        monsters.Clear();
        playerBullets.Clear();
        effectExplosions.Clear();
        player = null;
        // ...
    }


    /*************************************************************************************************************************/
    /*************************************************************************************************************************/


    // 执行怪生成配置并返回是否已经全部执行完毕
    public int MonstersGeneratorsUpdate() {
        var time = scene.time;
        for (int i = monsterGenerators.Count - 1; i >= 0; i--) {
            var mg = monsterGenerators[i];
            if (mg.activeTime <= time) {
                if (mg.destroyTime >= time) {
                    mg.Update();
                } else {
                    monsterGenerators.RemoveAtSwapBack(i);
                }
            }
        }
        return monsterGenerators.Count;
    }

    // 驱动所有怪
    public int MonstersUpdate() {
        var os = monsters;
        for (int i = os.Count - 1; i >= 0; i--) {
            var o = os[i];
            if (o.Update()) {
                o.Destroy();    // 会从 容器 自动移除自己
            }
        }
        return os.Count;
    }

    // 驱动所有爆炸特效
    public int ExplosionsUpdate() {
        var os = effectExplosions;
        for (int i = os.Count - 1; i >= 0; i--) {
            var o = os[i];
            if (o.Update()) {
                os.RemoveAtSwapBack(i); // 从数组移除
                o.Destroy();    // 资源回收. 并不会自动从数组移除
            }
        }
        return os.Count;
    }

    // 驱动所有玩家子弹
    public int PlayerBulletsUpdate() {
        var os = playerBullets;
        for (int i = os.Count - 1; i >= 0; i--) {
            var o = os[i];
            if (o.Update()) {
                os.RemoveAtSwapBack(i); // 从数组移除
                o.Destroy();    // 资源回收. 并不会自动从数组移除
            }
        }
        return os.Count;
    }

    // 驱动所有玩家
    public int PlayerUpdate() {
        player.Update();
        return 1;
    }

    // 当前玩家所在屏幕区域边缘随机一个点返回
    public Vector2 GetRndPosOutSideTheArea() {
        var e = Random.Range(0, 4);
        switch (e) {
            case 0:
                return new Vector2(player.x + Random.Range(-Scene.designWidth_2, Scene.designWidth_2), player.y - Scene.designHeight_2);
            case 1:
                return new Vector2(player.x + Random.Range(-Scene.designWidth_2, Scene.designWidth_2), player.y + Scene.designHeight_2);
            case 2:
                return new Vector2(player.x - Scene.designWidth_2, player.y + Random.Range(-Scene.designWidth_2, Scene.designWidth_2));
            case 3:
                return new Vector2(player.x + Scene.designWidth_2, player.y + Random.Range(-Scene.designWidth_2, Scene.designWidth_2));
        }
        return Vector2.zero;
    }

    // 获取甜甜圈形状里的随机点
    public Vector2 GetRndPosDoughnut(float maxRadius, float safeRadius) {
        var len = maxRadius - safeRadius;
        var len_radius = len / maxRadius;
        var safeRadius_radius = safeRadius / maxRadius;
        var radius = Mathf.Sqrt(Random.Range(0, len_radius) + safeRadius_radius) * maxRadius;
        var radians = Random.Range(-Mathf.PI, Mathf.PI);
        return new Vector2(Mathf.Cos(radians) * radius, Mathf.Sin(radians) * radius);
    }

}

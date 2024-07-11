﻿using UnityEngine;

public class Monster : ISpaceItem {
    public Scene scene;                                 // 指向场景
    public Stage stage;                                 // 指向关卡
    public int indexOfContainer;                        // 自己位于的 stage.monsters 数组的下标

    public Sprite[] sprites;                            // 指向动画帧集合
    public GO go;                                       // 保存底层 u3d 资源

    public const float defaultMoveSpeed = 20;           // 原始移动速度
    public const float _1_defaultMoveSpeed = 1f / defaultMoveSpeed;
    public const float frameAnimIncrease = 1f / 5;      // 帧动画前进速度( 针对 defaultMoveSpeed )
    public const float displayScale = 1f;               // 显示放大修正
    public const float defaultRadius = 10f;             // 原始半径

    public float frameIndex = 0;                        // 当前动画帧下标
    public bool flipX;                                  // 根据移动方向判断要不要反转 x 显示
    public float lastMoveValueX;                        // 备份，用来判断移动方向，要不要反转 x 显示

    public float moveSpeed = 10;                        // 当前每帧移动距离
    public float radius = defaultRadius;                // 半径

    #region space_x_y
    public SpaceContainer spaceContainer { get; set; }
    public ISpaceItem spacePrev { get; set; }
    public ISpaceItem spaceNext { get; set; }
    public int spaceIndex { get; set; } = -1;
    public float spaceX { get; set; }
    public float spaceY { get; set; }
    #endregion


    public Monster(Stage stage_, Sprite[] sprites_, float x, float y) {
        // 各种基础初始化
        stage = stage_;
        scene = stage_.scene;
        indexOfContainer = stage.monsters.Count;
        stage.monsters.Add(this);
        sprites = sprites_;

        // 从对象池分配 u3d 底层对象
        GO.Pop(ref go);

        // 放入空间索引容器
        spaceContainer = scene.spaceContainer;
        spaceX = x;
        spaceY = y;
        spaceContainer.Add(this);
        //Debug.Log($"spaceIndex = {spaceIndex}");
    }

    public virtual bool Update() {

        // 步进动画帧下表
        frameIndex += frameAnimIncrease;
        var len = sprites.Length;
        if (frameIndex >= len) {
            frameIndex -= len;
        }

        // 随机角度移动
        var r = Random.Range(0f, Mathf.PI * 2);
        var sin = Mathf.Sin(r);
        var cos = Mathf.Cos(r);
        spaceX += cos * moveSpeed;
        spaceY += sin * moveSpeed;

        // todo: 防范怪物随机挪动到超出 grid地图 范围

        // 更新在空间索引容器中的位置
        spaceContainer.Update(this);
        return false;
    }

    public virtual void Draw(float cx, float cy) {
        if (spaceX < cx - Scene.designWidth_2
            || spaceX > cx + Scene.designWidth_2
            || spaceY < cy - Scene.designHeight_2
            || spaceY > cy + Scene.designHeight_2) {
            go.g.SetActive(false);
        } else {
            go.g.SetActive(true);

            // 同步帧下标
            go.r.sprite = sprites[(int)frameIndex];

            // 同步 & 坐标系转换( y 坐标需要反转 )
            go.t.position = new Vector3(spaceX * Scene.designWidthToCameraRatio, -spaceY * Scene.designWidthToCameraRatio, 0);
            go.t.localScale = new Vector3(displayScale, displayScale, displayScale);
        }
    }

    public virtual void DrawGizmos() {
        Gizmos.DrawWireSphere(new Vector3(spaceX * Scene.designWidthToCameraRatio, -spaceY * Scene.designWidthToCameraRatio, 0), radius * Scene.designWidthToCameraRatio);
    }

    public virtual void Destroy(bool needRemoveFromContainer = true) {
#if UNITY_EDITOR
        if (go.g != null)           // unity 点击停止按钮后，这些变量似乎有可能提前变成 null
#endif
        {
            // 将 u3d 底层对象返回池
            GO.Push(ref go);
        }

        // 从空间索引容器移除
        spaceContainer.Remove(this);

        // 从 stage 容器交换删除
        if (needRemoveFromContainer) {
            var ms = stage.monsters;
            var lastIndex = ms.Count - 1;
            var last = ms[lastIndex];
            last.indexOfContainer = indexOfContainer;
            ms[indexOfContainer] = last;
            ms.RemoveAt(lastIndex);
        }
    }
}

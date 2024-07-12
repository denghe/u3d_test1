using System;
using Unity.VisualScripting;
using UnityEngine;

// 显示设置改为 dx12 可在 editor 里观察 hdr 效果
public partial class Scene : MonoBehaviour {

    // 拖拽时先点击 锁 图标锁定

    // 编辑器中拖拽 带法线的材质球到此 ( texture packer 插件 生成的那个, 需要核查法线贴图是否正确 )
    public Material material;

    // 编辑器中 分组, 多选 拖拽 精灵图集到此 ( texture packer 插件 生成的那个, 展开再 shift 多选 )
    public Sprite[] sprites_player;
    public Sprite[] sprites_bullets;
    public Sprite[] sprites_monster01;
    public Sprite[] sprites_monster02;
    public Sprite[] sprites_monster03;
    public Sprite[] sprites_monster04;
    public Sprite[] sprites_monster05;
    public Sprite[] sprites_monster06;
    public Sprite[] sprites_monster07;
    public Sprite[] sprites_monster08;
    public Sprite[] sprites_monster09;
    // ...


    // 逻辑帧率
    internal const float fps = 60;

    // 逻辑帧率间隔时长
    internal const float frameDelay = 1.0f / fps;

    // 设计分辨率
    internal const float designWidth = 1920, designHeight = 1080;

    // 设计分辨率的一半 方便计算和使用
    internal const float designWidth_2 = designWidth / 2, designHeight_2 = designHeight / 2;

    // 设计分辨率到 摄像头坐标 的转换系数 
    internal const float designWidthToCameraRatio = 19 / designWidth;    // todo: 需要进一步找准这个数据

    // 每个格子的直径( 正方形 )
    internal const float cellSize = 32;

    // 一些常数
    internal const float sqrt2 = 1.414213562373095f;
    internal const float sqrt2_1 = 0.7071067811865475f;

    // 当前总的运行帧编号
    internal int time = 0;

    // 用于稳定调用 逻辑 Update 的时间累计变量
    internal float timePool = 0;

    // 当前关卡
    internal Stage stage;

    // 空间索引容器 要用到的找最近所需要的格子偏移数组( all stage 公用 )
    internal static SpaceRingDiffuseData spaceRDD = new(100, cellSize);


    void Start() {

        // 初始化玩家输入系统
        InitInputAction();

        // 初始化 HDR 显示模式
        try {
            HDROutputSettings.main.RequestHDRModeChange(true);
        } catch (Exception e) {
            Debug.Log(e);
        }

        // 初始化对象池
        GO.Init(material, 20000);

        // 初始化起始关卡
        stage = new Stage(this);
    }

    void Update() {
        // 处理输入( 只是填充 playerMoving 等状态值 )
        HandlePlayerInput();

        // 按设计帧率驱动游戏逻辑
        timePool += Time.deltaTime;
        if (timePool > frameDelay) {
            timePool -= frameDelay;
            ++time;
            stage.Update();
        }

        // 同步显示
        stage.Draw();
    }

    void OnDrawGizmos() {
        if (stage == null) return;
        stage.DrawGizmos();
    }

    void OnDestroy() {
        stage.Destroy();
        GO.Destroy();
    }

    internal void SetStage(Stage newStage) {
        stage.Destroy();
        stage = newStage;
    }

}

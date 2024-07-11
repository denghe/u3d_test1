using System;
using UnityEngine;

/*
条件:
    需要安装 TexturePacker Importer
    需要安装 Input System 并添加 Input Action 生成代码
    项目设置 Player 里须勾选 HDR 那两个选项
    Assets\Settings\Render2D 需修改 General \ Transparency Sort Mode 为 Custom Axis, X 0 Y 1 Z 0
    场景中的 灯光 可以拖拽到 Camera 下面，避免 Camera 移动之后照不到
*/

// 空场景新建 GameObject 空类挂上去. 拖拽时先点击 锁 图标锁定
public partial class Scene : MonoBehaviour {

    // 编辑器中拖拽 带法线的材质球到此 ( texture packer 插件 生成的那个, 需要核查法线贴图是否正确 )
    public Material material;

    // 编辑器中 分组, 多选 拖拽 精灵图集到此 ( texture packer 插件 生成的那个, 展开再 shift 多选 )
    public Sprite[] sprites_player;
    public Sprite[] sprites_monster01;
    public Sprite[] sprites_monster02;
    public Sprite[] sprites_monster03;
    public Sprite[] sprites_monster04;
    public Sprite[] sprites_monster05;
    public Sprite[] sprites_monster06;
    public Sprite[] sprites_monster07;
    public Sprite[] sprites_monster08;
    public Sprite[] sprites_monster09;
    public Sprite[] sprites_monster10;
    public Sprite[] sprites_monster11;
    public Sprite[] sprites_monster12;
    public Sprite[] sprites_monster13;
    public Sprite[] sprites_monster14;
    public Sprite[] sprites_monster15;
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

    // 大地图格子数量
    internal const int numRows = 512, numCols = 512;

    // 每个格子的直径( 正方形 )
    internal const float cellSize = 64;

    // 大地图总宽度
    internal const float gridWidth = numCols * cellSize;

    // 大地图总高度
    internal const float gridHeight = numRows * cellSize;

    // 大地图中心点坐标
    internal const float gridWidth_2 = gridWidth / 2, gridHeight_2 = gridHeight / 2;
    internal const float gridCenterX = gridWidth_2, gridCenterY = gridHeight_2;

    // 一些常数
    internal const float sqrt2 = 1.414213562373095f;
    internal const float sqrt2_1 = 0.7071067811865475f;

    // 当前总的运行帧编号
    internal int time = 0;

    // 用于稳定调用 逻辑 Update 的时间累计变量
    internal float timePool = 0;

    // 当前关卡
    internal IStage stage;

    // 暂时用于怪物的 空间索引容器
    internal SpaceContainer spaceContainer;

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

        // 初始化空间索引
        spaceContainer = new(512, 512, 64);

        // 初始化起始关卡
        stage = new Stage_1(this);
    }

    void Update() {
        // 处理输入( 只是填充 playerMoving 等状态值 )
        HandlePlayerInput();

        // 按设计帧率驱动游戏逻辑
        timePool += Time.deltaTime;
        if (timePool > frameDelay) {
            timePool -= frameDelay;
            stage.Update();
        }

        // 同步显示
        stage.Draw();
    }

    void OnDestroy() {
        stage.Destroy();
        GO.Destroy();
        Debug.Assert(spaceContainer.numItems == 0);
    }

    internal void SetStage(IStage newStage) {
        stage.Destroy();
        stage = newStage;
    }

}

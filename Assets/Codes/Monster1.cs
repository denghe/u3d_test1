public class Monster1 : Monster {
    public Monster1(Stage stage_) : base(stage_) {
    }

    // 供 MonsterGenerator 调用
    public Monster1 Init(float x, float y) {
        Init(scene.sprites_monster01, x, y);
        return this;
    }

    // todo: 独特逻辑
}

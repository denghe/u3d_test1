public class Monster2 : Monster {
    public Monster2(Stage stage_) : base(stage_) {
    }

    // 供 MonsterGenerator 调用
    public void Init(float x, float y) {
        Init(scene.sprites_monster02, x, y);
    }

    // todo: 独特逻辑
}

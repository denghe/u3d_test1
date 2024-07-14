public class MonsterGenerator1 : MonsterGenerator {
    public MonsterGenerator1(string name_, Stage stage_, int activeTime_, int destroyTime_, int generateDelay_) : base(name_, stage_, activeTime_, destroyTime_, generateDelay_) {
    }

    public override void Update() {
        var time = scene.time;
        if (time > nextGenerateTime) {
            nextGenerateTime = time + generateDelay;

            var pos = stage.GetRndPosOutSideTheArea();
            new Monster1(stage).Init(pos.x, pos.y);
        }
    }
}

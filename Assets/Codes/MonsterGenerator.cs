public class MonsterGenerator {
    public string name;
    public Scene scene;
    public Stage stage;

    public int activeTime, destroyTime;
    public int generateDelay, nextGenerateTime;

    protected MonsterGenerator(string name_, Stage stage_, int activeTime_, int destroyTime_, int generateDelay_) {
        name = name_;
        stage = stage_;
        scene = stage_.scene;
        activeTime = activeTime_;
        destroyTime = destroyTime_;
        generateDelay = generateDelay_;
    }

    public virtual void Update() {
        //var time = scene.time;
        //if (time > nextGenerateTime) {
        //    nextGenerateTime = time + generateDelay;
        //
        //    //new Monster( rnd pos ? );
        //}
    }

    public virtual void Destroy() {
    }
}

//public void GenRndMonster() {
//    //// 每一种创建 ?? 只
//    //foreach (var ss in spritess) {
//    //    for (int i = 0; i < 5000; i++) {
//    //        var x = gridCenterX + UnityEngine.Random.Range(-Scene.designWidth_2, Scene.designWidth_2);
//    //        var y = gridCenterY + UnityEngine.Random.Range(-Scene.designHeight_2, Scene.designHeight_2);
//    //        new Monster(this, ss, x, y);
//    //    }
//    //}
//    // todo: 补怪逻辑, 阶段性试图凑够多少只同屏
//    var ss = spritess[Random.Range(0, spritess.Count)];
//    var p = GetRndPosOutSideTheArea();
//    new Monster(this, ss, p.x, p.y);
//}

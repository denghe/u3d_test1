public interface IStage {
    public Scene scene { get; }

    /*
// 方便复制. 用多少复制多少
switch (state) {
    case 0: State0(); return;
    case 1: State1(); return;
    case 2: State2(); return;
    case 3: State3(); return;
    case 4: State4(); return;
    case 5: State5(); return;
    case 6: State6(); return;
    case 7: State7(); return;
    case 8: State8(); return;
    case 9: State9(); return;
    case 10: State10(); return;
    case 11: State11(); return;
    default:
        throw new Exception("???");
}
    */
    public void Update();

    public void Destroy();

    public void Draw();
}

namespace IdleGame.Equipment
{
    // 강화/드롭 로직에서 랜덤을 주입 가능하게 만들어 결정론적 유닛 테스트를 가능하게 한다.
    public interface IRandomSource
    {
        double NextDouble();
    }
}

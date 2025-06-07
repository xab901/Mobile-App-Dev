public abstract class BaseState
{
    protected Enemy currentEnemy;
    public abstract void OnEnter(Enemy enemy);
    public abstract void LogicUpdate(); // 逻辑更新
    public abstract void PhysicsUpdate(); // FixedUpdate中执行，实现物理判断
    public abstract void OnExit();
}

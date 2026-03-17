using UnityEngine;

public abstract class TankState
{
    protected TankBossController tank;
    protected TankStateMachine stateMachine;

    public TankState(TankBossController tank, TankStateMachine stateMachine)
    {
        this.tank = tank;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}

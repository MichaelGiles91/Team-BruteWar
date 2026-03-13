using UnityEngine;

public abstract class SoldierState
{
    protected SoldierEnemyController soldier;
    protected SoldierStateMachine stateMachine;

    public SoldierState(SoldierEnemyController soldier, SoldierStateMachine stateMachine)
    {
        this.soldier = soldier;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}

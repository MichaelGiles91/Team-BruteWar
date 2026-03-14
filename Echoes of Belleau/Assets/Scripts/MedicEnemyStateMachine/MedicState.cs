using UnityEngine;

public abstract class MedicState
{
    protected MedicEnemyController medic;
    protected MedicStateMachine stateMachine;

    public MedicState(MedicEnemyController medic, MedicStateMachine stateMachine)
    {
        this.medic = medic;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}


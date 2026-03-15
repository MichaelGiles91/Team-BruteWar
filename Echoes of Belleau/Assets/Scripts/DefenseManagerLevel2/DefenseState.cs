using UnityEngine;

public abstract class DefenseState 
{
    protected DefenseManagerL2 manager;
    protected DefenseStateMachine stateMachine;

    // Constructor to initialize the state with references to the manager and state machine
    public DefenseState(DefenseManagerL2 manager, DefenseStateMachine stateMachine) 
    {
        this.manager = manager;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }

}

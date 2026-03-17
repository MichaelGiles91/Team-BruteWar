using UnityEngine;

public class DefenseCompleteState : DefenseState
{
    public DefenseCompleteState(DefenseManagerL2 manager, DefenseStateMachine stateMachine)
        : base(manager, stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("Defense complete. All waves cleared.");
    }
}

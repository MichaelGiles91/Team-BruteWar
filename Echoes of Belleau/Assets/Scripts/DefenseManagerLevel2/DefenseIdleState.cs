using UnityEngine;

public class DefenseIdleState : DefenseState
{
    public DefenseIdleState(DefenseManagerL2 manager, DefenseStateMachine stateMachine)
        : base(manager, stateMachine)
    {
    }

    public override void Enter()
    {
    }
}

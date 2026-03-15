using UnityEngine;

public class TankDeadState : TankState
{
    public TankDeadState(TankBossController tank, TankStateMachine stateMachine)
        : base(tank, stateMachine)
    {
    }

    public override void Enter()
    {
        tank.StopMoving();
        Debug.Log(tank.name + " tank is dead.");

        if (tank.destroyOnDeath)
        {
            Object.Destroy(tank.gameObject, 3f);
        }
    }
}

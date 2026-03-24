using UnityEngine;

// Chase behaviour for the Tank boss.
// Pursues the target, orients toward it and fires both machine gun and cannon
// at their configured rates while the target is within detection range.
public class TankChaseState : TankState
{
    private float machineGunTimer;
    private float cannonTimer;

    public TankChaseState(TankBossController tank, TankStateMachine stateMachine)
        : base(tank, stateMachine)
    {
    }

    public override void Enter()
    {
        machineGunTimer = 0f;
        cannonTimer = 0f;
    }

    public override void Update()
    {
        if (tank.target == null)
            return;

        tank.MoveTowards(tank.target.position, tank.chaseSpeed);
        tank.FaceTarget();

        if (tank.IsTargetInRange(tank.detectionRange))
        {
            cannonTimer -= Time.deltaTime;

            if (!tank.IsReloadingMachineGun())
            {
                machineGunTimer -= Time.deltaTime;

                if (machineGunTimer <= 0f)
                {
                    tank.FireMachineGun();
                    machineGunTimer = tank.machineGunFireRate;
                }
            }

            if (cannonTimer <= 0f)
            {
                tank.FireCannon();
                cannonTimer = tank.cannonFireRate;
            }
        }
    }

    public override void Exit()
    {
        tank.StopMoving();
    }
}
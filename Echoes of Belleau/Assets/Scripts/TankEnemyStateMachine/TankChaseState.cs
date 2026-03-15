using UnityEngine;

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
            machineGunTimer -= Time.deltaTime;
            cannonTimer -= Time.deltaTime;

            if (machineGunTimer <= 0f)
            {
                tank.FireMachineGun();
                machineGunTimer = tank.machineGunFireRate;
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

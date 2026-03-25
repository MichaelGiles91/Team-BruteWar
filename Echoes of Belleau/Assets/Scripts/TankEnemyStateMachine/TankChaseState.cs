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

        // Always face the player in both phases
        tank.FaceTarget();

        // PHASE 1:
        // Track the player and use machine gun only.
        // Do NOT chase.
        if (tank.currentPhase == TankBossController.TankPhase.Phase1Patrol)
        {
            tank.StopMoving();

            if (tank.IsTargetInRange(tank.detectionRange))
            {
                if (!tank.IsReloadingMachineGun())
                {
                    machineGunTimer -= Time.deltaTime;

                    if (machineGunTimer <= 0f)
                    {
                        tank.FireMachineGun();
                        machineGunTimer = tank.machineGunFireRate;
                    }
                }
            }

            return;
        }

        // PHASE 2:
        // Chase the player and use cannon only.
        if (tank.currentPhase == TankBossController.TankPhase.Phase2BossFight)
        {
            tank.MoveTowards(tank.target.position, tank.chaseSpeed);

            if (tank.IsTargetInRange(tank.detectionRange))
            {
                cannonTimer -= Time.deltaTime;

                if (cannonTimer <= 0f)
                {
                    tank.FireCannon();
                    cannonTimer = tank.cannonFireRate;
                }
            }

            return;
        }
    }

    public override void Exit()
    {
        tank.StopMoving();
    }
}
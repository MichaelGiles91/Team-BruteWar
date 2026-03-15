using UnityEngine;

public class TankPatrolState : TankState
{
    private float machineGunTimer;

    public TankPatrolState(TankBossController tank, TankStateMachine stateMachine)
        : base(tank, stateMachine)
    {
    }

    public override void Enter()
    {
        machineGunTimer = 0f;

        Transform patrolPoint = tank.GetCurrentPatrolPoint();
        if (patrolPoint != null)
        {
            tank.MoveTowards(patrolPoint.position, tank.patrolSpeed);
        }
    }

    public override void Update()
    {
        if (tank.currentPhase == TankBossController.TankPhase.Phase2BossFight)
        {
            stateMachine.ChangeState(tank.ChaseState);
            return;
        }

        Transform patrolPoint = tank.GetCurrentPatrolPoint();
        if (patrolPoint == null)
            return;

        if (tank.HasReachedDestination())
        {
            tank.AdvancePatrolPoint();

            Transform nextPoint = tank.GetCurrentPatrolPoint();
            if (nextPoint != null)
            {
                tank.MoveTowards(nextPoint.position, tank.patrolSpeed);
            }
        }

        if (tank.target != null && tank.IsTargetInRange(tank.detectionRange))
        {
            tank.FaceTarget();

            machineGunTimer -= Time.deltaTime;
            if (machineGunTimer <= 0f)
            {
                tank.FireMachineGun();
                machineGunTimer = tank.machineGunFireRate;
            }
        }
    }

    public override void Exit()
    {
        tank.StopMoving();
    }
}
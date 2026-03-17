using UnityEngine;

// Patrol behaviour for the Tank boss.
// Handles movement between patrol points, detection and intermittent machine-gun fire
// while patrolling. This state transitions to the Chase state when phase 2 is active.
public class TankPatrolState : TankState
{
    #region Fields

    // Timer to control machine gun fire rate while patrolling and targeting.
    private float machineGunTimer;

    #endregion

    #region Construction

    // Construct the patrol state with a reference to the tank controller and the state machine.
    public TankPatrolState(TankBossController tank, TankStateMachine stateMachine)
        : base(tank, stateMachine)
    {
    }

    #endregion

    #region State: Enter / Update / Exit

    // Called once when the state becomes active.
    // Reset firing timer and start moving toward the current patrol point (if any).
    public override void Enter()
    {
        machineGunTimer = 0f;

        Transform patrolPoint = tank.GetCurrentPatrolPoint();
        if (patrolPoint != null)
        {
            // Use the controller helper to command movement at patrol speed.
            tank.MoveTowards(patrolPoint.position, tank.patrolSpeed);
        }
    }

    // Called every frame while this state is active.
    // - Check for phase changes and switch to chase
    // - Advance patrol points when destination reached
    // - If target is within detection range, face target and fire machine gun at intervals
    public override void Update()
    {
        // If boss has entered phase 2, switch to chase behaviour immediately.
        if (tank.currentPhase == TankBossController.TankPhase.Phase2BossFight)
        {
            stateMachine.ChangeState(tank.ChaseState);
            return;
        }

        Transform patrolPoint = tank.GetCurrentPatrolPoint();
        if (patrolPoint == null)
            return;

        // When the agent reaches its current patrol destination, advance to the next point
        // and issue a move command to it.
        if (tank.HasReachedDestination())
        {
            tank.AdvancePatrolPoint();

            Transform nextPoint = tank.GetCurrentPatrolPoint();
            if (nextPoint != null)
            {
                tank.MoveTowards(nextPoint.position, tank.patrolSpeed);
            }
        }

        // If there is a target and it's within detection range, face it and attempt to shoot.
        if (tank.target != null && tank.IsTargetInRange(tank.detectionRange))
        {
            // Orient horizontally toward the target before firing.
            tank.FaceTarget();

            // Decrease the fire timer and fire when it reaches zero.
            machineGunTimer -= Time.deltaTime;
            if (machineGunTimer <= 0f)
            {
                tank.FireMachineGun();
                machineGunTimer = tank.machineGunFireRate;
            }
        }
    }

    // Called when leaving this state: ensure the tank stops moving to avoid unwanted pathing.
    public override void Exit()
    {
        tank.StopMoving();
    }

    #endregion
}
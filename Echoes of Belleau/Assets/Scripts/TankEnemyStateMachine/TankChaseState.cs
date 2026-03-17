using UnityEngine;

// Chase behaviour for the Tank boss.
// Pursues the target, orients toward it and fires both machine gun and cannon
// at their configured rates while the target is within detection range.
public class TankChaseState : TankState
{
    #region Fields

    // Timers to control weapon fire rates while chasing.
    private float machineGunTimer;
    private float cannonTimer;

    #endregion

    #region Construction

    // Construct the chase state with references to the tank controller and the state machine.
    public TankChaseState(TankBossController tank, TankStateMachine stateMachine)
        : base(tank, stateMachine)
    {
    }

    #endregion

    #region State: Enter / Update / Exit

    // Called once when this state becomes active.
    // Initialize weapon timers so the tank can fire immediately if desired.
    public override void Enter()
    {
        machineGunTimer = 0f;
        cannonTimer = 0f;
    }

    // Called every frame while this state is active.
    // - Move toward the target and face it.
    // - When the target is within detection range, decrement weapon timers and fire when they reach zero.
    public override void Update()
    {
        if (tank.target == null)
            return;

        // Move toward the target using the controller helper and chase speed.
        tank.MoveTowards(tank.target.position, tank.chaseSpeed);
        // Ensure the tank faces the target before firing.
        tank.FaceTarget();

        // Only attempt to fire when the target is within configured detection range.
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

    // Called when leaving this state: stop movement to avoid unintended navigation.
    public override void Exit()
    {
        tank.StopMoving();
    }

    #endregion
}

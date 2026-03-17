using UnityEngine;

/// <summary>
/// Handles the dead state for the Tank boss.
/// Stops movement, logs death, and optionally destroys the tank GameObject after a delay.
/// </summary>
public class TankDeadState : TankState
{
    #region Construction

    // Create the dead state with references to the tank controller and the state machine.
    public TankDeadState(TankBossController tank, TankStateMachine stateMachine)
        : base(tank, stateMachine)
    {
    }

    #endregion

    #region State: Enter

    // Called once when the state becomes active.
    // - Stop any navigation/pathing to avoid further movement.
    // - Log the death event for debugging.
    // - Optionally destroy the tank's GameObject after a short delay if configured.
    public override void Enter()
    {
        // Ensure the NavMeshAgent is stopped and the path cleared.
        tank.StopMoving();

        // Informative log for debugging and tooling.
        Debug.Log(tank.name + " tank is dead.");

        // If configured to destroy on death, destroy the GameObject after 3 seconds.
        // This allows death animations/effects to play if present.
        if (tank.destroyOnDeath)
        {
            Object.Destroy(tank.gameObject, 3f);
        }
    }

    #endregion
}

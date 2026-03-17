using UnityEngine;

// Simple state machine for the Tank boss.
// Responsible for holding the active TankState and forwarding lifecycle calls (Enter, Exit, Update).
// Keeps responsibilities minimal so states own behavior and transitions.
public class TankStateMachine
{
    #region Fields

    // The currently active state. Exposed as read-only to allow external queries without modification.
    public TankState CurrentState { get; private set; }

    #endregion

    #region Public

    /// <summary>
    /// Initialize the state machine with the provided starting state.
    /// Calls the state's Enter method immediately so it becomes active.
    /// </summary>
    /// <param name="startingState">The state to start from.</param>
    public void Initialize(TankState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    /// <summary>
    /// Change the active state to a new state.
    /// Calls Exit on the previous state (if any) and Enter on the new state.
    /// </summary>
    /// <param name="newState">The new state to activate.</param>
    public void ChangeState(TankState newState)
    {
        if (CurrentState != null)
        {
            // Let the current state perform any necessary cleanup.
            CurrentState.Exit();
        }

        CurrentState = newState;

        // Immediately enter the new state so it becomes active on this frame.
        CurrentState.Enter();
    }

    /// <summary>
    /// Forward Update to the currently active state.
    /// Safe to call every frame from the owning controller.
    /// </summary>
    public void Update()
    {
        if (CurrentState != null)
        {
            CurrentState.Update();
        }
    }

    #endregion

    #region Notes / Usage Example

    // When triggering the boss fight call tankBossController.SetPhase2()
    // This can be done from a cutscene manager, trigger zone or other game event.

    /* Example trigger 
    using UnityEngine;

    public class TankBossTrigger : MonoBehaviour
    {
        [SerializeField] private TankBossController tankBoss;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            tankBoss.SetPhase2();
            gameObject.SetActive(false);
        }
    }
    */

    #endregion
}

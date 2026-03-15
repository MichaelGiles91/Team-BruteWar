using UnityEngine;

public class TankStateMachine
{
    public TankState CurrentState { get; private set; }

    public void Initialize(TankState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(TankState newState)
    {
        if (CurrentState != null)
        {
            CurrentState.Exit();
        }

        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        if (CurrentState != null)
        {
            CurrentState.Update();
        }
    }
}

// When Triggereing boss fight call tankBossController.SetPhase2()
// Can do this from a cutscene or triger zone

/*using UnityEngine;

public class TankBossTrigger : MonoBehaviour
{
    [SerializeField] private TankBossController tankBoss;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        tankBoss.SetPhase2();
        gameObject.SetActive(false);
    }
}*/

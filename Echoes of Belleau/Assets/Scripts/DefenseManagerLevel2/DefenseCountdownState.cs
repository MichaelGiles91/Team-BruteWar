using UnityEngine;

public class DefenseCountdownState : DefenseState
{
    private float countdownTimer;

    public DefenseCountdownState(DefenseManagerL2 manager, DefenseStateMachine stateMachine)
        : base(manager, stateMachine)
    {

    }
    public override void Enter()
    {
        if (manager.CurrentWave == null)
        {
            stateMachine.ChangeState(manager.CompleteState);
            return;
        }
        countdownTimer = manager.CurrentWave.startDelay;
        Debug.Log("Starting " + manager.CurrentWave.waveName + " in " + countdownTimer + " seconds.");
    }
    public override void Update()
    {
        countdownTimer -= Time.deltaTime;
        if (countdownTimer <= 0f)
        {
            stateMachine.ChangeState(manager.SpawningState);
        }
    }
}

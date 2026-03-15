using UnityEngine;

public class DefenseWaveActiveState : DefenseState
{
    private float waveClearTimer = -1f;

    public DefenseWaveActiveState(DefenseManagerL2 manager, DefenseStateMachine stateMachine)
        : base(manager, stateMachine)
    {
    }

    public override void Enter()
    {
        waveClearTimer = -1f;
        Debug.Log(manager.CurrentWave.waveName + " is active.");
    }

    public override void Update()
    {
        if (!manager.AreAllEnemiesDefeated())
            return;

        if (waveClearTimer < 0f)
        {
            waveClearTimer = manager.delayAfterWaveClear;
            Debug.Log(manager.CurrentWave.waveName + " cleared.");
        }

        waveClearTimer -= Time.deltaTime;

        if (waveClearTimer <= 0f)
        {
            bool hasNextWave = manager.BeginNextWave();

            if (!hasNextWave)
            {
                stateMachine.ChangeState(manager.CompleteState);
            }
            else
            {
                stateMachine.ChangeState(manager.CountdownState);
            }
        }
    }
}

using UnityEngine;

public class MedicIdleState : MedicState
{
    private float idleTimer;

    public MedicIdleState(MedicEnemyController medic, MedicStateMachine stateMachine)
        : base(medic, stateMachine)
    {
    }

    public override void Enter()
    {
        medic.StopMoving();
        idleTimer = Random.Range(medic.minIdleTime, medic.maxIdleTime);
    }

    public override void Update()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            stateMachine.ChangeState(medic.SeekCoverState);
        }
    }
}


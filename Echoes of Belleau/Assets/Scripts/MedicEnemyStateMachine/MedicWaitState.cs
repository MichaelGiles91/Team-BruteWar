using UnityEngine;

public class MedicWaitState : MedicState
{
    private float waitTimer;

    public MedicWaitState(MedicEnemyController medic, MedicStateMachine stateMachine)
        : base(medic, stateMachine)
    {
    }

    public override void Enter()
    {
        medic.StopMoving();
        waitTimer = medic.waitBetweenPatients;
    }

    public override void Update()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            medic.FindClosestInjuredAlly();

            if (medic.CurrentPatient != null)
            {
                stateMachine.ChangeState(medic.MoveToPatientState);
            }
            else
            {
                stateMachine.ChangeState(medic.SeekCoverState);
            }
        }
    }
}


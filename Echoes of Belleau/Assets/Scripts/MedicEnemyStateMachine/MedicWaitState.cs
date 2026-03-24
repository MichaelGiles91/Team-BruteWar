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
        // If the medic was recently shot and is not safe yet, go find cover first.
        if (medic.ShouldRetreatToCover())
        {
            stateMachine.ChangeState(medic.SeekCoverState);
            return;
        }

        // Wait a short time before checking for the next patient.
        waitTimer -= Time.deltaTime;

        if (waitTimer > 0f)
            return;

        // Look for the nearest injured ally.
        medic.FindClosestInjuredAlly();

        // If one exists, leave cover and go help them.
        if (medic.CurrentPatient != null)
        {
            if (medic.CurrentCover != null)
            {
                medic.CurrentCover.isOccupied = false;
                medic.CurrentCover = null;
            }

            stateMachine.ChangeState(medic.MoveToPatientState);
            return;
        }

        // If nobody needs healing and we are not currently safe, go to cover.
        if (!medic.IsAtCover())
        {
            stateMachine.ChangeState(medic.SeekCoverState);
            return;
        }

        // Nobody needs healing, and we are already in cover,
        // so reset the timer and keep waiting safely.
        waitTimer = medic.waitBetweenPatients;
    }
}
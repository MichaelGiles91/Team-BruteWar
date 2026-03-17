using UnityEngine;

public class MedicMoveToPatientState : MedicState
{
    public MedicMoveToPatientState(MedicEnemyController medic, MedicStateMachine stateMachine)
        : base(medic, stateMachine)
    {
    }

    public override void Enter()
    {
        if (medic.CurrentPatient != null)
        {
            medic.MoveTowards(medic.CurrentPatient.transform.position, medic.moveSpeed);
        }
    }

    public override void Update()
    {
        if (medic.CurrentPatient == null)
        {
            stateMachine.ChangeState(medic.SeekCoverState);
            return;
        }

        if (!medic.CurrentPatient.NeedsHealing())
        {
            stateMachine.ChangeState(medic.SeekCoverState);
            return;
        }

        if (medic.IsPatientInRange() || medic.HasReachedDestination())
        {
            stateMachine.ChangeState(medic.HealState);
        }
    }
}


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
        else
        {
            stateMachine.ChangeState(medic.SeekCoverState);
        }
    }

    public override void Update()
    {
        if (medic.ShouldRetreatToCover())
        {
            medic.CurrentPatient = null;
            stateMachine.ChangeState(medic.SeekCoverState);
            return;
        }

        if (medic.CurrentPatient == null)
        {
            stateMachine.ChangeState(medic.SeekCoverState);
            return;
        }

        if (medic.CurrentPatient.IsDead())
        {
            medic.CurrentPatient = null;
            stateMachine.ChangeState(medic.SeekCoverState);
            return;
        }

        if (!medic.CurrentPatient.NeedsHealing())
        {
            medic.CurrentPatient = null;
            stateMachine.ChangeState(medic.SeekCoverState);
            return;
        }

        if (medic.IsPatientInRange() || medic.HasReachedDestination())
        {
            stateMachine.ChangeState(medic.HealState);
        }
    }
}
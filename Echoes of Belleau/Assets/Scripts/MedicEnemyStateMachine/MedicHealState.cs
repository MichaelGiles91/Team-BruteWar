using UnityEngine;

public class MedicHealState : MedicState
{
    private float healTimer;

    public MedicHealState(MedicEnemyController medic, MedicStateMachine stateMachine)
        : base(medic, stateMachine)
    {
    }

    public override void Enter()
    {
        medic.StopMoving();
        healTimer = medic.healDuration;
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

        healTimer -= Time.deltaTime;

        if (healTimer <= 0f)
        {
            medic.HealCurrentPatient();
            medic.CurrentPatient = null;
            stateMachine.ChangeState(medic.SeekCoverState);
        }
    }
}
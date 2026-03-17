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
        if (medic.CurrentPatient == null)
        {
            stateMachine.ChangeState(medic.SeekCoverState);
            return;
        }

        healTimer -= Time.deltaTime;

        if (healTimer <= 0f)
        {
            medic.HealCurrentPatient();
            stateMachine.ChangeState(medic.SeekCoverState);
        }
    }
}


using UnityEngine;

public class MedicSeekCoverState : MedicState
{
    public MedicSeekCoverState(MedicEnemyController medic, MedicStateMachine stateMachine)
        : base(medic, stateMachine)
    {
    }

    public override void Enter()
    {
        medic.FindBestCover();
    }

    public override void Update()
    {
        if (medic.CurrentCover != null)
        {
            stateMachine.ChangeState(medic.MoveToCoverState);
            return;
        }

        stateMachine.ChangeState(medic.WaitState);
    }
}


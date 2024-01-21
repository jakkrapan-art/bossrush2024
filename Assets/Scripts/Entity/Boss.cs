using System;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class Boss : Entity
{
    [SerializeField]
    private float _rage = 50.00f;
    [SerializeField]
    private float _ragePointIncreased = 0.16f;
    private void Awake()
    {
        _stateMachine = new BossStateMachine();
    }
    private void Start()
    {

    }

    private void FixedUpdate()
    {
        _rage += _ragePointIncreased;
        handleStateByRagePoint();
        _stateMachine.FixedUpdate();
    }

    private void handleStateByRagePoint()
    {
        float ragePercentage = getRagePointPercentage();
        BossStateMachine bossStateMachine = (BossStateMachine)this._stateMachine;
        if (ragePercentage >= 0.80f && ragePercentage < 0.99f)
        {
            if (!bossStateMachine.GetCurrentState().Equals(bossStateMachine.RageState))
            {
                bossStateMachine.ChangeState(bossStateMachine.RageState);
            }
        } else if (ragePercentage >= 0.99f)
        {
            if (!bossStateMachine.GetCurrentState().Equals(bossStateMachine.EnrageState))
            {
                bossStateMachine.ChangeState(bossStateMachine.EnrageState);
            }
        } else if (ragePercentage > 0.01f && ragePercentage <= 0.30f)
        {
            if (!bossStateMachine.GetCurrentState().Equals(bossStateMachine.AlmostFullState))
            {
                bossStateMachine.ChangeState(bossStateMachine.AlmostFullState);
            }
        } else if (ragePercentage <= 0.01f)
        {
            if (!bossStateMachine.GetCurrentState().Equals(bossStateMachine.FullState))
            {
                bossStateMachine.ChangeState(bossStateMachine.FullState);
            }
        } else
        {
            if (!bossStateMachine.GetCurrentState().Equals(bossStateMachine.IdleState))
            {
                bossStateMachine.ChangeState(bossStateMachine.IdleState);
            }
        }
    }

    private float getRagePointPercentage()
    {
        return _rage / 100.00f;
    }

    public void DecreasedRage(float rage)
    {
        _rage -= rage;
    }
}
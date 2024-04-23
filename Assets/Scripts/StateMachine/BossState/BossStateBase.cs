using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStateBase : State
{
    protected BossStateMachine _stateMachine = null;
    protected Boss _boss = null;
    protected string _animationName = null;

    public BossStateBase(BossStateMachine stateMachine, Boss boss, string animationName = "", bool savePrevious = false) : base(savePrevious)
    {
        _stateMachine = stateMachine;
        _boss = boss;
        _animationName = animationName;
    }

    public override void OnEnter()
    {
      base.OnEnter();

      SetAnimationBool(true);
    }

    public override void OnExit()
    {
      base.OnExit();
      SetAnimationBool(false);
    }

    private void SetAnimationBool(bool value)
    {
        if(_boss.Animator != null && !string.IsNullOrEmpty(_animationName))
        {
            _boss.Animator.SetBool(_animationName, value);
        }
    }
}

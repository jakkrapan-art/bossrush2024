using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStateBase : State
{
    protected BossStateMachine _stateMachine = null;
    public BossStateBase(BossStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
}

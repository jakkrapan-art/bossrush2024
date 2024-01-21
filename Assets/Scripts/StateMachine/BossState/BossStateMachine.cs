using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStateMachine : StateMachine
{
    private IdleState IdleState { get; }
    private HungryState HungryState { get; }
    private RageState RageState { get; }
    private EnrageState EnrageState { get; }
    private AlmostFullState AlmostFullState { get; }
    private FullState FullState { get; }
    public BossStateMachine()
    {
        IdleState = new IdleState();
        HungryState = new HungryState();
        RageState = new RageState();
        EnrageState = new EnrageState();
        AlmostFullState = new AlmostFullState();
        FullState = new FullState();

        Init();
    }
    protected override State GetInitialState() => IdleState;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStateMachine : StateMachine
{
    public IdleState IdleState { get; private set; }
    public HungryState HungryState { get; private set; }
    public RageState RageState { get; private set; }
    public EnrageState EnrageState { get; private set; }
    public AlmostFullState AlmostFullState { get; private set; }
    public FullState FullState { get; private set; }
    public BossStateMachine()
    {
        this.IdleState = new IdleState(this);
        this.HungryState = new HungryState(this);
        this.RageState = new RageState(this);
        this.EnrageState = new EnrageState(this);
        this.AlmostFullState = new AlmostFullState(this);
        this.FullState = new FullState(this);

        Init();
    }
    protected override State GetInitialState() => IdleState;
}

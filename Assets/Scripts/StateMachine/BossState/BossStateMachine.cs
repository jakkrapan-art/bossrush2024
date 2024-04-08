using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStateMachine : StateMachine
{
  public Boss Boss { get; private set; }
  public IdleState IdleState { get; private set; }
  public HungryState HungryState { get; private set; }
  public RageState RageState { get; private set; }
  public EnrageState EnrageState { get; private set; }
  public AlmostFullState AlmostFullState { get; private set; }
  public FullState FullState { get; private set; }
  public ThinkState ThinkState { get; private set; }

  public BossStateMachine(Boss boss)
  {
    Boss = boss;

    IdleState = new IdleState(this);
    HungryState = new HungryState(this);
    RageState = new RageState(this);
    EnrageState = new EnrageState(this);
    AlmostFullState = new AlmostFullState(this);
    FullState = new FullState(this);
    ThinkState = new ThinkState(this, 5);

    Init();
  }
  protected override State GetInitialState() => IdleState;
}

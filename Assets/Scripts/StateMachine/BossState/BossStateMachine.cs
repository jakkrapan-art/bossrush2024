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
  public BossEatState EatState { get; private set; }

  public BossStateMachine(Boss boss)
  {
    Boss = boss;

    IdleState = new IdleState(this, boss);
    HungryState = new HungryState(this, boss);
    RageState = new RageState(this, boss);
    EnrageState = new EnrageState(this, boss);
    AlmostFullState = new AlmostFullState(this, boss);
    FullState = new FullState(this, boss);
    ThinkState = new ThinkState(this, boss, 5);
    EatState = new BossEatState(this, boss);

    Init();
  }
  protected override State GetInitialState() => IdleState;
}

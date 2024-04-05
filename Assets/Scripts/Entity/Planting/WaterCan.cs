using UnityEngine;

public class WaterCan : Item
{
  private int _maxWater = 10;
  private int _currentWater;

  public bool Use(int consumeCount)
  {
    if(_currentWater == 0 || _currentWater - consumeCount < 0) return false;

    _currentWater -= consumeCount;
    return true;
  }

  public void Refill()
  {
    _currentWater = _maxWater;
  }
}

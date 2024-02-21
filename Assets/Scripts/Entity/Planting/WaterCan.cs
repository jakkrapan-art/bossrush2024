using UnityEngine;

public class WaterCan : Items
{
  private int _maxWater = 10;
  private int _currentWater;

  public static WaterCan Create(ObjectPool pool, Vector2 position)
  {
    var obj = pool.Get<WaterCan>("WaterCan");

    obj.Refill();
    obj.transform.position = position;
    return obj;
  }

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

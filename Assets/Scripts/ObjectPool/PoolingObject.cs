using UnityEngine;

public class PoolingObject : MonoBehaviour
{
  private ObjectPool _pool;
  private string _key;

  public void Setup(ObjectPool pool, string key)
  {
    _pool = pool;
    _key = key;
  }

  public void ReturnToPool()
  {
    _pool.Return(_key, gameObject);
  }
}

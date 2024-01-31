using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
  public static ObjectPool Create()
  {
    var obj = new GameObject();
    var pool = obj.AddComponent<ObjectPool>();

    return pool;
  }

  private Dictionary<string, Queue<Object>> objectsInPool = new Dictionary<string, Queue<Object>>();

  public T Get<T>(string key) where T : Object
  {
    return GetObjectInPool<T>(key);
  }

  public void Return<T>(string key, T obj) where T : Object
  {
    if (!objectsInPool.ContainsKey(key)) throw new System.Exception("Cannot return object to not exist key. key = " + key);
    SetActiveObject(obj, false);
    objectsInPool[key].Enqueue(obj);
  }

  private void CreateObject<T>(string key) where T : Object
  {
    Queue<Object> list;
    if(objectsInPool.ContainsKey(key))
    {
      list = objectsInPool[key];
    }
    else
    {
      list = new Queue<Object>();
      objectsInPool.Add(key, list);
    }

    var loadedResource = Resources.Load<T>(key);
    if (!loadedResource) return;

    loadedResource.AddComponent<PoolingObject>().Setup(this, key);
    SetActiveObject(loadedResource, false);
    list.Enqueue(loadedResource);
  }

  private T GetObjectInPool<T>(string key) where T : Object
  {
    if (!objectsInPool.ContainsKey(key) || objectsInPool[key].Count == 0) CreateObject<T>(key);

    var list = objectsInPool[key];
    var obj = list.Dequeue();
    SetActiveObject(obj, true);
    return (T)obj;
  }

  private void SetActiveObject<T>(T obj, bool active) where T : Object
  {
    if (obj is GameObject go) go.SetActive(active);
  }
}

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
using System.Collections.Generic;
using UnityEngine;


public class ObjectPool : MonoBehaviour
{
  private readonly string PREFAB_PREFIX = "Prefabs/";
  public static ObjectPool GetInstance()
  {
    var pool = FindObjectOfType<ObjectPool>();
    if (!pool)
    {
      var obj = new GameObject("ObjectPool");
      pool = obj.AddComponent<ObjectPool>();
    }

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
    if (obj is GameObject go) go.transform.SetParent(transform);
    SetActiveObject(obj, false);
    objectsInPool[key].Enqueue(obj);
  }

  private void CreateObject<T>(string key) where T : Object
  {
    Queue<Object> list;
    if (objectsInPool.ContainsKey(key))
    {
      list = objectsInPool[key];
    }
    else
    {
      list = new Queue<Object>();
      objectsInPool.Add(key, list);
    }

    var loadedResource = Resources.Load<T>(PREFAB_PREFIX + key);
    if (!loadedResource) return;

    var instantiated = Instantiate(loadedResource);
    if (instantiated is MonoBehaviour mono)
    {
      mono.name = mono.name.Replace("(Clone)", "");
      PoolingObject poolingObj = mono.GetComponent<PoolingObject>();

      if (poolingObj)
      {
        poolingObj.Setup(this, key);
      }
      else
      {
        mono.gameObject.AddComponent<PoolingObject>().Setup(this, key);
      }
    }
    else
    {
      Debug.LogError("loadedResource is not GameObject. type = " + instantiated.GetType());
    }

    SetActiveObject(instantiated, false);
    list.Enqueue(instantiated);
  }

  private T GetObjectInPool<T>(string key) where T : Object
  {
    if (!objectsInPool.ContainsKey(key) || objectsInPool[key].Count == 0 || objectsInPool[key] == null) CreateObject<T>(key);

    var list = objectsInPool[key];
    if (list.Count == 0) return null;
    var obj = list.Dequeue();
    SetActiveObject(obj, true);
    return obj as T;
  }

  private void SetActiveObject<T>(T obj, bool active) where T : Object
  {
    if (obj is GameObject go) go.SetActive(active);
  }
}
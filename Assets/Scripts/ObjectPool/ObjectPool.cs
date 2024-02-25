using System.Collections.Generic;
using Unity.VisualScripting;
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

  public static void ReturnObjectToPool<T>(T obj) where T : IPoolingObject
  {
    ObjectPool pool = GetInstance();
    if(obj is MonoBehaviour mono == false)
    {
      return;
    }

    string key = mono.name.Replace("(Clone)", "");
    pool.Return(key, obj);
  }

  private Dictionary<string, Queue<object>> objectsInPool = new Dictionary<string, Queue<object>>();

  public T Get<T>(string key, Transform parent = null) where T:Object
  {
    var obj = GetObjectInPool<T>(key);
    if (!obj) return default;
    SetParent(obj, parent);

    return obj;
  }

  public void Return<T>(string key, T obj) where T : IPoolingObject
  {
    if (!objectsInPool.ContainsKey(key)) throw new System.Exception("Cannot return object to not exist key. key = " + key);
    SetActiveObject(obj, false);
    obj.ResetPoolingObject();
    objectsInPool[key].Enqueue(obj);
  }

  private void CreateObject<T>(string key) where T : Object
  {
    Queue<object> list;
    if (objectsInPool.ContainsKey(key))
    {
      list = objectsInPool[key];
    }
    else
    {
      list = new Queue<object>();
      objectsInPool.Add(key, list);
    }

    var loadedResource = Resources.Load<T>(PREFAB_PREFIX + key);
    if (!loadedResource) return;

    var instantiated = Instantiate(loadedResource);
    if(instantiated is GameObject go)
    {
      go.name = go.name.Replace("(Clone)", "");
    }
    else if (instantiated is MonoBehaviour mono)
    {
      mono.name = mono.name.Replace("(Clone)", "");
    }

    SetActiveObject(instantiated, false);
    list.Enqueue(instantiated);
  }

  private T GetObjectInPool<T>(string key) where T : Object
  {
    if (!objectsInPool.ContainsKey(key) || objectsInPool[key].Count == 0 || objectsInPool[key] == null) CreateObject<T>(key);

    var list = objectsInPool[key];
    if (list.Count == 0) return default;
    var obj = list.Dequeue();
    SetActiveObject(obj, true);
    return (T)obj;
  }

  private void SetActiveObject<T>(T obj, bool active)
  {
    GameObject targetToSet = null;
    if (obj is GameObject go)
    {
      targetToSet = go;
    }
    else if (obj is MonoBehaviour mono)
    {
      targetToSet = mono.gameObject;
    }

    targetToSet.SetActive(active);
  }

  private void SetParent<T>(T obj, Transform parent)
  {
    GameObject targetToSet = null;
    if (obj is GameObject go) 
    {
      targetToSet = go;
    }
    else if(obj is MonoBehaviour mono)
    {
      targetToSet = mono.gameObject;
    }

    if (targetToSet) targetToSet.transform.SetParent(parent);
  }
}
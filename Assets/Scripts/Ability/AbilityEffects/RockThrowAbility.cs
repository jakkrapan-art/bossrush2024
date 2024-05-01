using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability/RockThrow", fileName = "RockThrow")]
public class RockThrowAbility : Ability
{
  private List<Transform> _possibleTargets = new List<Transform>();
  private List<int> _targetIndexes = new List<int>();
  private ObjectPool _objectPool;

  [SerializeField]
  private float _rockDuration = 6.0f;
  [SerializeField]
  private bool _unbreakable = false;
  [SerializeField]
  private int _rockCount = 4;

  public override void Setup()
  {
    var bridges = FindObjectsOfType<Bridge>();
    foreach (var bridge in bridges) 
    {
      _possibleTargets.Add(bridge.transform);
      _targetIndexes.Add(_targetIndexes.Count);
    }

    _objectPool = ObjectPool.GetInstance();
    _rockCount = Mathf.Clamp(_rockCount, 0, _possibleTargets.Count);
  }

  public override void Activate()
  {
    base.Activate();
    if (_rockCount == 0) return;

    if(_objectPool != null)
    {
      List<int> randomIndexes = new List<int>(_targetIndexes);
      int removeCount = _targetIndexes.Count - _rockCount;

      for (int i = 0; i < removeCount; i++) 
      {
        randomIndexes.Remove(Random.Range(0, randomIndexes.Count));
      }

      foreach (int index in randomIndexes)
      {
        var rock = _objectPool.Get<Rock>("Rock");
        if (rock != null)
        {
          rock.Setup(_rockDuration, _unbreakable, 1f / _rockDuration);
          var chosenTransform = _possibleTargets[index];

          rock.transform.position = chosenTransform.position + new Vector3(0, -0.65f);
          rock.transform.localScale = chosenTransform.localScale;

          rock.Spawn();
        }
      }
    }
  }
}

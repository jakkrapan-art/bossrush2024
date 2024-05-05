using System.Collections.Generic;
using UnityEngine;

public class AbilityHolder : MonoBehaviour
{
  [SerializeField]
  private List<AbilityTriggerData> _abilities;
  [SerializeField]
  private float _cooldown = 15;
  [SerializeField]
  private float _initialCooldown = 8f;

  private List<Ability> _abilitiesCooldown = new List<Ability>();
  private float _readyTime = 0f;

  private void Start()
  {
    if(_abilities != null)
    {
      foreach (var ability in _abilities)
      {
        if(ability != null) ability.ability.Setup();
      }
    }

    _readyTime += Time.time + _initialCooldown;
  }

  public void ActiveAbility(float rage)
  {
    CheckAbilitiesCooldown();

    //TODO: implement random ability and use it.
    var possibleAbilities = GetPossibleAbilities(rage);
    var randomNum = Random.Range(0, 1f);
    var currPossiblility = 0f;
    Ability usingAbility = null;

    foreach (var data in possibleAbilities)
    {
      if(data != null)
      {
        if(randomNum > currPossiblility && randomNum < currPossiblility + data.triggerChance)
        {
          usingAbility = data.ability;
          break;
        }
        else
        {
          currPossiblility += data.triggerChance;
        }
      }
    }

    if (usingAbility != null)
    {
      usingAbility.Activate();
      _abilitiesCooldown.Add(usingAbility);
    }

    _readyTime = Time.time + _cooldown;
  }

  private List<AbilityTriggerData> GetPossibleAbilities(float rage)
  {
    List<AbilityTriggerData> result = new();

    foreach (var ability in _abilities)
    {
      if ( 
        _abilitiesCooldown.Contains(ability.ability) ||
        !(rage >= ability.rageMin && rage <= ability.rageMax)
      ) continue;

      result.Add(ability);
    }

    return result;
  }

  private void CheckAbilitiesCooldown()
  {
    List<Ability> newList = new List<Ability>();
    for (int i = 0; i < _abilitiesCooldown.Count; i++)
    {
      var ability = _abilitiesCooldown[i];
      if(ability != null && !ability.isReady) 
      {
        newList.Add(ability);
      }
    }

    _abilitiesCooldown = newList;
  }

  public bool IsReady()
  {
    return Time.time >= _readyTime;
  }
}

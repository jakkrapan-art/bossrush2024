using System.Collections.Generic;
using UnityEngine;

public class AbilityHolder : MonoBehaviour
{
  [SerializeField]
  private List<Ability> _abilities;
  private List<Ability> _abilitiesCooldown = new List<Ability>();

  private void Start()
  {
    if(_abilities != null)
    {
      foreach (var ability in _abilities)
      {
        if(ability != null) ability.Setup();
      }
    }
  }

  public void ActiveAbility()
  {
    CheckCooldown();

    //TODO: implement random ability and use it.
  }

  private void CheckCooldown()
  {
    List<Ability> newList = new List<Ability>();
    for (int i = 0; i < _abilitiesCooldown.Count; i++)
    {
      var ability = _abilitiesCooldown[i];
      if(ability != null) 
      {
        if(ability.isReady)
        {
          _abilities.Add(ability);
        }
        else
        {
          newList.Add(ability);
        }
      }
    }

    _abilitiesCooldown = newList;
  }
}

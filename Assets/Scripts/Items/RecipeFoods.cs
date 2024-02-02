using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EntityData", menuName = "Data/RecipeFoods")]
public class RecipeFoods : ScriptableObject
{
  public List<ItemData> ItemsInput;
  public Items ItemOutput;
}

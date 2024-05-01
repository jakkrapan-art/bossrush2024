using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StonedDebuff : MonoBehaviour
{
  InteractableObject _interactableObject;

  private void Start()
  {
    _interactableObject = GetComponent<InteractableObject>();

    if (_interactableObject == null)
    {
      Destroy(this);
    }
    else
    {
      _interactableObject.enabled = false;
    }
  }

  private void OnDestroy()
  {
    if(_interactableObject != null)
    {
      _interactableObject.enabled = true;
    }
  }
}

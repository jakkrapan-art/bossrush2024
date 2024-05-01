using System.Collections;
using UnityEngine;

public static class Utility
{
  public static void Shake(GameObject objToShake)
  {
    objToShake.GetComponent<MonoBehaviour>().StartCoroutine(DoShake(objToShake));
  }

  private static IEnumerator DoShake(GameObject objToShake)
  {
    var objTransform = objToShake.transform;
    float startTime = Time.time;
    float duration = 0.3f;

    Vector3 originPos = objTransform.position;

    while (Time.time < startTime + duration)
    {
      Vector3 randomDirection = Random.insideUnitSphere;
      float randomDistance = Random.Range(0f, 0.18f);
      Vector3 randomPosition = originPos + randomDirection * randomDistance;

      objTransform.position = randomPosition;

      yield return new WaitForFixedUpdate();
    }

    objTransform.position = originPos;
  }
}

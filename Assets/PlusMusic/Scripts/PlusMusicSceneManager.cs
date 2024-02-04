
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlusMusic;


namespace PlusMusic
{
    public class PlusMusicSceneManager : MonoBehaviour
    {
        //-----------------------------------------------
        // Do NOT use RuntimeInitializeLoadType.SubsystemRegistration!
        // It allows for your DontDestroyOnLoad() game object to be destroyed!
        // https://forum.unity.com/threads/game-runs-fine-in-editor-but-not-in-build.1364256/

        //-----------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeInit()
        {
            Debug.Log("PM> Scene.OnRuntimeInit()");

            if (null != PlusMusicCore.Instance)
                Debug.Log("PM> Scene.OnRuntimeInit(): Core found, nothing to do here ...");
            else
            {
                Debug.Log("PM> Scene.OnRuntimeInit(): No Core found, let's make one ...");

                Object pluginObject = Resources.Load("PlusMusicPlugin");
                Instantiate(pluginObject, new Vector3(0, 0, 0), Quaternion.identity);
            }
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlusMusic
{
    public class PMFpsLimiter : MonoBehaviour
    {
        [TextArea(5, 10)]
        public string developerComments =
            "Setting a target FPS for your game can free up some extra CPU cycles that would otherwise " +
            "be wasted on rendering buffers that will never be displayed.\n" +
            "\n"
            ;

        [Tooltip("The target FPS for your game")]
        public int targetFps = 60;


        //----------------------------------------------------------
        private void Awake()
        {
            // Throttle the CPU to give us some more play-time
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFps;
        }

    }
}

/* ---------------------------------------------------------------------------
Application:    PlusMusic Unity Plugin - Triggers
Copyright:      PlusMusic, (c) 2023
Author:         Andy Schmidt
Description:    Trigger to load a track

TODO:
    Important todo items are marked with a $$$ comment

--------------------------------------------------------------------------- */

using System;
using UnityEngine;
using PlusMusic;
using PlusMusicTypes;


namespace PlusMusic
{
    public class PMTriggerLoadTrack: MonoBehaviour
    {
        [TextArea(5, 10)]
        public string developerComments =
            "Use this script to load tracks from your PlusMusic project.\n" +
            "\n" +
            "- 'Track Id'\nLoad track using the unique Track ID found in your PlusMusic project\n" +
            "- 'Track Array Index'\nLoad track using the tracks's array index in your PlusMusic project\n" +
            "- 'Load On Start'\nLoad track at scene start (disable if you use triggers)\n" +
            "- 'Play After Load'\nPlays track immediately after it is loaded (disable if you are scripting playing behavior)\n" +
            "- 'Player Root Object'\nReference to your player object, used as trigger collider." +
            " NOTE: This does not have to be the player, any other collider will do. If omitted, any collider will trigger.\n" +
            "- 'Trigger On Enter' and 'Trigger On Exit'\nSelect what action(s) you want to trigger this script\n" +
            "- 'Track Transition'\nAllows you to customize the transition from the current audio to this track\n" +
            "\n";

        [Header("Track Load Settings")]
        [Tooltip("Unique Track ID from the PlusMusic Project Manager")]
        public Int64 trackId = 0;
        [Tooltip("Array index in the Project Track Array (Zero based)")]
        public int trackArrayIndex = 0;
        [Tooltip("Load Track at Scene Start")]
        public bool loadOnStart = false;
        [Tooltip("Play Track after loading")]
        public bool playAfterLoad = false;
        [Tooltip("Player Object for Collider Trigger")]
        public GameObject playerRootObject;
        [Tooltip("Load Track at Trigger Enter")]
        public bool triggerOnEnter = false;
        [Tooltip("Load Track at Trigger Exit")]
        public bool triggerOnExit = false;
        [Tooltip("Transition to use if PlayAfterLoad is true")]
        public PMTransitionInfo trackTransition;

        private string playerName = "";
        private bool hasProjectLoaded = false;


        //----------------------------------------------------------
        void Start()
        {
            if (null == PlusMusicCore.Instance)
            {
                Debug.LogError("PM> ERROR:PMTriggerLoadTrack.Start(): There is no PlusMusicCore in the scene!");
                return;
            }

            if (null != playerRootObject)
                playerName = playerRootObject.name;
            else
                if (triggerOnEnter || triggerOnExit)
                    Debug.LogWarning(
                        "PM> PMTriggerLoadTrack.Start(): Without a PlayerRootObject object this script will trigger off any collider!");
        }

        //----------------------------------------------------------
        private void Update()
        {
            if (!hasProjectLoaded)
            {
                if (PlusMusicCore.Instance.GetIsProjectLoaded)
                {
                    hasProjectLoaded = true;
                    if (loadOnStart)
                        LoadTrack();
                }
            }
        }

        //----------------------------------------------------------
        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnEnter && hasProjectLoaded)
            {
                if (!String.IsNullOrWhiteSpace(playerName))
                    if (playerName != other.gameObject.name)
                        return;

                LoadTrack();
            }
        }

        //----------------------------------------------------------
        private void OnTriggerExit(Collider other)
        {
            if (triggerOnExit && hasProjectLoaded)
            {
                if (!String.IsNullOrWhiteSpace(playerName))
                    if (playerName != other.gameObject.name)
                        return;

                LoadTrack();
            }
        }

        //----------------------------------------------------------
        public void LoadTrack()
        {
            if (PlusMusicCore.Instance.GetDebugMode)
                Debug.LogFormat("PM> PMTriggerLoadTrack.LoadTrack(): root = {0}, tag = {1}", 
                    transform.root.gameObject.name, trackTransition.tag);

            PlusMusicCore.Instance.LoadTrack(new PMTrackProgress
            {
                id = trackId,
                index = trackArrayIndex,
                autoPlay = playAfterLoad
            });
        }

    }
}

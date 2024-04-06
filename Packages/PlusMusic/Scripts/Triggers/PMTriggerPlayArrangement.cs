/* ---------------------------------------------------------------------------
Application:    PlusMusic Unity Plugin - Triggers
Copyright:      PlusMusic, (c) 2023
Author:         Andy Schmidt
Description:    Trigger to play an arrangement

TODO:
    Important todo items are marked with a $$$ comment

--------------------------------------------------------------------------- */

using System;
using UnityEngine;
using PlusMusic;
using PlusMusicTypes;


namespace PlusMusic
{
    public class PMTriggerPlayArrangement: MonoBehaviour
    {
        [Tooltip("Play Arrangement at Scene Start")]
        public bool playOnStart = false;
        [Tooltip("Player Object for Collider Trigger")]
        public GameObject playerRootObject;
        [Tooltip("Play Arrangement at Trigger Enter")]
        public bool triggerOnEnter = true;
        [Tooltip("Play Arrangement at Trigger Exit")]
        public bool triggerOnExit = false;
        [Tooltip("Transition to use")]
        public PMTransitionInfo arrangementTransition;

        private string playerName = "";
        private bool hasProjectLoaded = false;


        //----------------------------------------------------------
        void Start()
        {
            if (null == PlusMusicCore.Instance)
            {
                Debug.LogError("PM> ERROR:PMTriggerPlayArrangement.Start(): There is no PlusMusicCore in the scene!");
                return;
            }

            if (null != playerRootObject)
                playerName = playerRootObject.name;
            else
                if (triggerOnEnter || triggerOnExit)
                    Debug.LogWarning(
                        "PM> PMTriggerPlayArrangement.Start(): Without a PlayerRootObject object this script will trigger off any collider!");
        }

        //----------------------------------------------------------
        private void Update()
        {
            if (!hasProjectLoaded)
            {
                if (PlusMusicCore.Instance.GetIsProjectLoaded)
                {
                    hasProjectLoaded = true;
                    if (playOnStart)
                        PlayArrangement();
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

                PlayArrangement();
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

                PlayArrangement();
            }
        }

        //----------------------------------------------------------
        public void PlayArrangement()
        {
            if (PlusMusicCore.Instance.GetDebugMode)
                Debug.LogFormat("PM> PMTriggerPlayArrangement.PlayArrangement(): root = {0}, tag = {1}", 
                    transform.root.gameObject.name, arrangementTransition.tag);

            PlusMusicCore.Instance.PlayArrangement(arrangementTransition);
        }

    }
}

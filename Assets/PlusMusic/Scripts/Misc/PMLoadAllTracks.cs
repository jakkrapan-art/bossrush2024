/* ---------------------------------------------------------------------------
Application:    PlusMusic Unity Plugin - Misc
Copyright:      PlusMusic, (c) 2023
Author:         Andy Schmidt
Description:    Load all tracks

TODO:
    Important todo items are marked with a $$$ comment

--------------------------------------------------------------------------- */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlusMusic;
using PlusMusicTypes;
using static PlasticPipe.Server.MonitorStats;
using System.Net;


namespace PlusMusic
{
    public class PMLoadAllTracks: MonoBehaviour
    {

        [HideInInspector]
        public enum loadChoice
        {
            loadAllTracks,
            loadByTrackId,
            loadByProjectArrayIndex,
        };

        [TextArea(5, 10)]
        public string developerComments =
            "Use this script to preload (cache) tracks that you are using in your game to improve in-game loading times.\n" +
            "It is best to attach this script to an initial splash/loading/menu scene at the beginning of your game.\n" +
            "\n" +
            "- 'Load All Tracks'\nIgnores the two optional lists and preloads all tracks in your project\n" +
            "- 'Load By Track Id'\nAdd the unique Track IDs of each track you want to preload\n" +
            "- 'Load By Project Array Index'\nAdd the array indecies of your project tracks you want to preload\n" +
            "\n";

        [Header("Track Load Settings")]
        [Tooltip("Select type of loading/caching")]
        public loadChoice selectLoadType;
        [Tooltip("Print track loading progress to the console log")]
        public bool logLoadProgress = false;

        [Header("Project Track Filters")]
        [Tooltip("Optional list of Track IDs")]
        public List<Int64> loadTracksById;
        [Tooltip("Optional list of Project Array Indecies")]
        public List<int> loadTracksByArrayIndex;


        private bool hasProjectLoaded = false;
        private bool isTrackLoaded = true;
        private float loadTimeoutDefaultValue = 60.0f;



        //----------------------------------------------------------
        void Start()
        {
            if (null == PlusMusicCore.Instance)
            {
                Debug.LogError("PM> ERROR:PMLoadAllTracks.Start(): There is no PlusMusicCore in the scene!");
                return;
            }

            if (!logLoadProgress)
                logLoadProgress = PlusMusicCore.Instance.GetDebugMode;

            PlusMusicCore.Instance.OnTrackLoadingProgress += TrackLoadingProgress;
        }

        //----------------------------------------------------------
        private void Update()
        {
            if (!hasProjectLoaded)
            { 
                if (PlusMusicCore.Instance.GetIsProjectLoaded)
                { 
                    hasProjectLoaded = true;
                    StartCoroutine(LoadTracks());
                }
            }
        }

        //----------------------------------------------------------
        private void OnDestroy()
        {
            if (null != PlusMusicCore.Instance)
            {
                PlusMusicCore.Instance.OnTrackLoadingProgress -= TrackLoadingProgress;
            }
        }

        //----------------------------------------------------------
        public void TrackLoadingProgress(PMTrackProgress progress)
        {
            if (progress.progress < 1.0f)
                isTrackLoaded = false;
            else
                isTrackLoaded = true;
        }

        //----------------------------------------------------------
        // TODO: The WaitUntil() functions have the possibility to deadlock!
        // AS: For now, we're simply using a timeout ...
        protected IEnumerator LoadTracks()
        {
            string func_name = "PMLoadAllTracks.LoadTracks()";
            if (logLoadProgress)
                Debug.Log($"PM> {func_name}");

            // If there is a current track load, we wait until it is loaded
            float loadTimeout = loadTimeoutDefaultValue;
            if (logLoadProgress)
                Debug.Log($"PM> {func_name}: Waiting for pending track load ...");
            yield return new WaitUntil(() => 
                !PlusMusicCore.Instance.GetIsEventManagerProcessing || (loadTimeout -= Time.deltaTime) <= 0.0f);
            if (logLoadProgress)
                if (loadTimeout > 0.0f)
                    Debug.Log(
                        $"PM> {func_name}: OK, moving on after {(loadTimeoutDefaultValue - loadTimeout):F3} seconds");
                else
                    Debug.LogWarning($"PM> {func_name}: Timeout waiting for pending track load!");

            // Now we loop over the track array and load any tracks that arent in memory yet
            int tracksLoaded = 0;
            int tracksAlreadyLoaded = 0;
            int tracksInProject = 0;
            float time_start = Time.realtimeSinceStartup;

            PMMessageProjectInfo project_info = PlusMusicCore.Instance.GetProjectInfo();
            if (null != project_info.tracks)
            {
                tracksInProject = project_info.tracks.Length;
                for (int t = 0; t<tracksInProject; t++)
                {
                    if (!project_info.tracks[t].isLoaded)
                    {
                        bool loadTrack = false;

                        switch (selectLoadType)
                        {
                            case loadChoice.loadAllTracks:
                                loadTrack = true;
                                break;
                            case loadChoice.loadByTrackId:
                                if (loadTracksById.Contains(project_info.tracks[t].id))
                                    loadTrack = true;
                                break;
                            case loadChoice.loadByProjectArrayIndex:
                                if (loadTracksByArrayIndex.Contains(t))
                                    loadTrack = true;
                                break;
                        }

                        if (loadTrack)
                        {
                            if (logLoadProgress)
                                Debug.Log(
                                    $"PM> {func_name}: Loading Track[{t}] {project_info.tracks[t].id}" +
                                    $" - {project_info.tracks[t].name} ...");

                            isTrackLoaded = false;
                            loadTimeout = loadTimeoutDefaultValue;

                            bool didQueue = PlusMusicCore.Instance.LoadTrack(new PMTrackProgress
                            {
                                id = project_info.tracks[t].id,
                                index = t,
                                autoPlay = false
                            });

                            if (didQueue)
                            { 
                                yield return new WaitUntil(() => isTrackLoaded || (loadTimeout -= Time.deltaTime) <= 0.0f);
                                if (loadTimeout > 0.0f)
                                {
                                    if (logLoadProgress)
                                        Debug.Log(
                                            $"PM> {func_name}: Track[{t}] loaded OK in {(loadTimeoutDefaultValue - loadTimeout):F3} seconds");
                                    tracksLoaded++;
                                }
                                else
                                    Debug.LogWarning($"PM> {func_name}: Timeout loading track!");
                            }
                            else
                                Debug.LogWarning($"PM> {func_name}: Track load aborted!");
                        }
                    }
                    else
                    { 
                        tracksAlreadyLoaded++;
                        Debug.Log(
                            $"PM> {func_name}: Track[{t}] {project_info.tracks[t].id}" +
                            $" - {project_info.tracks[t].name} already loaded, skipping ...");
                    }
                }
            }
            else
                Debug.LogWarning($"PM> {func_name}: Project has no tracks!");

            float time_elapsed = Time.realtimeSinceStartup - time_start;

            if ((tracksAlreadyLoaded + tracksLoaded) > 0)
            { 
                if (logLoadProgress)
                { 
                    Debug.Log(
                        $"PM> {func_name}: alreadyLoaded/loaded/inProject = " +
                        $"{tracksAlreadyLoaded}/{tracksLoaded}/{tracksInProject}");
                    Debug.Log($"PM> {func_name}: Time eplased = {time_elapsed:F6} seconds");
                }
            }
            else
                Debug.LogWarning($"PM> {func_name}: No tracks were loaded/cached!");
        }


    }
}

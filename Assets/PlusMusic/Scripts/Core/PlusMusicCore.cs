/* ---------------------------------------------------------------------------
Application:    PlusMusic Unity Plugin - Core Module
Copyright:      PlusMusic, (c) 2023
Author:         Andy Schmidt
Description:    Core plugin utility functions

TODO:
    Important todo items are marked with a $$$ comment

NOTES:
    - Yes, i'm using GOTO in places. Deal with it.

TODO:
    - https://forum.unity.com/threads/the-ins-and-outs-of-audio-memory-in-unity.97690/

--------------------------------------------------------------------------- */

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using PlusMusicTypes;
using static PlasticPipe.Server.MonitorStats;
#if UNITY_EDITOR
    using UnityEditor;
#endif


namespace PlusMusic
{

    //----------------------------------------------------------
    public class PlusMusicCore : MonoBehaviour
    {

        //----------------------------------------------------------
        // Private vars
        //----------------------------------------------------------
        #region private_vars

        private string pluginVersion        = "0.0.0";
        private string cachePath            = "";
        private string cachePathHidden      = "PlusMusic/Resources/Cache~";
        private string cachePathVisible     = "PlusMusic/Resources/Cache";
        private string cachePathProjects    = "Projects";
        private string cachePathTracks      = "Tracks";

        private PlusMusicEventManager EventManager;

        private PMProject currentProject = new PMProject();
        private Dictionary<string, AudioClip> soundFxClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, Coroutine> coroutinePool = new Dictionary<string, Coroutine>();

        private PlusMusicSettingsSo plusMusicAccount;
        private PMSettingsSo pmso;
        private PMSettings settings;
        private int arrangementTypeMax = 9;
        private bool debugMode = false;

        // Audio Mixer and Groups
        private AudioMixer audioMixerPM;
        private AudioMixerGroup[] audioGroupLayers;
        private AudioMixerGroup audioGroupSoundFX;

        // We need two array here so we can smoothly blend audio in/out when transitioning
        private AudioSource[] layerAudioSources1;
        private AudioSource[] layerAudioSources2;

        private int LayerFullMix = (int)PMAudioLayers.LayerFullMix;
        private int LayerTopMix = (int)PMAudioLayers.LayerTopMix;

        private bool audioSource1Playing = true;
        private AudioSource audioSourceSoundFx;
        private AudioSource currentAudioSource;
        private AudioSource nextAudioSource;
        private AudioSource[] currentLayers;
        private AudioSource[] nextLayers;
        private PMAudioState audioState = PMAudioState.StateStopped;
        private bool isAudioPlaying = false;
        private float crossfadeOverlap = 0.005f;
        private float pluginVolume = 1.0f;
        private float[] layerVolumes = new float[5] { 0.0f, 1.0f, 1.0f, 1.0f, 1.0f };

        private float[] filterBuffer = null;
        private int filterLength = 0;
        private float filterVolume = 0.05f;

        private bool didStartupPingback = false;
        private bool sceneUnloadedIsHooked = false;
        private bool killQueue = false;


        #endregion
        //----------------------------------------------------------
        // Public vars
        //----------------------------------------------------------
        #region public_vars


        // Static singleton instance of this Class
        public static PlusMusicCore Instance;

        // VSA delegate
        public delegate bool HookUnityVSA(string actionName, string partnerName, string customerUid);
        public static HookUnityVSA VSASendAttributionEvent;

        [TextArea(5, 10)]
        public string developerComments =
            "Please don't add this script to a scene object as it is automatically loaded by the 'PlusMusicSceneManager'.\n" +
            "\n" +
            "If you have used previous version of the plugin and have scenes with the Core script attached " +
            "(formerly known as the DJ), you will have to remove them all to avoid concurrency conflicts.\n" +
            "\n" +
            "Use PlusMusicSettingsSo in the resources folder to configure your plugin.\n" +
            "\n";

        //----------------------------------------------------------
        // Public Events that allow users to react to the plugin
        //----------------------------------------------------------

        // Event returning true once the Core has finished initializing
        public event Action<bool> OnInit;

        // Event returning information about the loaded project
        public event Action<PMMessageProjectInfo> OnProjectInfoLoaded;

        // Event returning track loading status
        public event Action<PMTrackProgress> OnTrackLoadingProgress;

        // Event returning plugin status info
        public event Action<string> OnRealTimeStatus;

        // Event returning Track playing statistics
        // NOTE: Too costly, consumer should poll instead. See UI/PlusMusicOverlay.cs for an example.
        //public event Action<string> OnTrackPlayProgress;

        // Event returning current arrangement after change
        public event Action<PMTags> OnArrangementChanged;

        // Event for audio state changes Start/Stop/Pause etc.
        public event Action<PMAudioState> OnAudioStateChanged;

        // Event for audio layer volume changes
        // NOTE: For example, you can use this to adjust UI sliders
        public event Action<float[]> OnLayerVolumeChanged;

        //----------------------------------------------------------
        // Public Get/Set functions
        //----------------------------------------------------------
        public int ArrangementTypeMax { get => arrangementTypeMax; }
        public bool GetDebugMode { get => debugMode; }
        public float[] GetLayersVolume { get => layerVolumes; }
        public string GetPluginVersion { get => pluginVersion; }
        public float GetPluginVolume { get => pluginVolume; }
        public PMTransitionInfo GetCurrentTransition { get => currentProject.transition; }
        public PMAudioState GetAudioState  { get => audioState; }
        public bool GetIsAudioPlaying { get => isAudioPlaying; }
        public bool GetIsProjectLoaded { get => currentProject.isLoaded; }
        public PMSettingsSo GetSettings { get => pmso; }
        public PMTrackProgress GetCurrentTrackProgress { get => currentProject.trackProgress; }
        public bool GetIsEventManagerProcessing { get => EventManager.isProcessing; }


        #endregion
        //----------------------------------------------------------
        // Built-in Unity functions
        //----------------------------------------------------------
        #region unity_functions


        //----------------------------------------------------------
        /**
        * @brief Called before start
        */
        private void Awake()
        {
            // Check if we are an imposter
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;    // Nope, we're the real deal

            // Load the account settings
            plusMusicAccount = Resources.Load<PlusMusicSettingsSo>("PlusMusicSettingsSo");
            if (null == plusMusicAccount)
            {
                Debug.LogError("PM> ERROR:Core.Awake(): PlusMusic account is not configured!");
                return;
            }

            // Get the saved settings
            pmso = plusMusicAccount.PlusMusicSettings;
            debugMode = pmso.debugMode;
            pmso.loadAudioFromCache = pmso.useLocalCache;
            pmso.loadMetaFromCache = pmso.useLocalCache;
            pmso.saveAudioToCache = pmso.useLocalCache;
            pmso.saveMetaToCache = pmso.useLocalCache;
            if (pmso.refreshLocalCache)
            {
                pmso.loadAudioFromCache = false;
                pmso.loadMetaFromCache = false;
                pmso.saveAudioToCache = true;
                pmso.saveMetaToCache = true;
            }

            // Say hello
            if (debugMode)
                Debug.Log("PM> Core.Awake()");

            // Only do this if we're playing
            if (Application.isPlaying)
            {
                if (debugMode)
                    Debug.Log($"PM> Core.Awake(): persistAcrossScenes = {pmso.persistAcrossScenes}");

                // Moved DontDestroyOnLoad() to Awake() to make sure we have a valid reference
                // for other classes in their Start() functions
                if (pmso.persistAcrossScenes)
                    DontDestroyOnLoad(Instance);
            }

            OnInit?.Invoke(false);
            if (!Init())
            {
                Destroy(this);
                return;
            }
            OnInit?.Invoke(true);
        }

        //----------------------------------------------------------
        /**
        * @brief Called when the scene is started
        */
        private void Start()
        {
            if (debugMode)
                Debug.Log("PM> Core.Start()");

            if (null == currentAudioSource)
            {
                Debug.LogError("PM> ERROR:Core.Start(): currentAudioSource is null!");
                return;
            }

            if (debugMode)
                Debug.LogFormat("PM> Core.Start(): EventManager.GetStatus() = {0}", EventManager.GetStatus());

            InitCache();

            // Default settings
            settings = new PMSettings();
            settings.target      = GetEnvVariable("PM_TARGET", "app");
            settings.username    = GetEnvVariable("PM_USER", "");
            settings.password    = GetEnvVariable("PM_PASS", "");
            settings.credentials = "";
            settings.auto_play   = pmso.autoPlayProject;
            if ((!string.IsNullOrWhiteSpace(settings.username)) && (!string.IsNullOrWhiteSpace(settings.password)))
                settings.credentials = settings.username + ":" + settings.password + "@";
            settings.base_url = "https://" + settings.credentials + settings.target + ".plusmusic.ai/api/plugin/v1.0/";

            if (debugMode)
            {
                Debug.LogFormat("PM> Core.Start(): Application.dataPath = {0}", Application.dataPath);
                Debug.LogFormat("PM> Core.Start(): Application.persistentDataPath = {0}", Application.persistentDataPath);
                Debug.LogFormat("PM> Core.Start(): base_url = {0}", settings.base_url);
                Debug.LogFormat("PM> Core.Start(): doPingbacks = {0}", pmso.doPingbacks);
            }

            // Hook the scene load/unload callbacks
            // JIRA PP-28: Added sceneLoaded to reset the killQueue flag
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            sceneUnloadedIsHooked = true;

            // Load reverb filter
            filterBuffer = ExtractFilterFromAAI("Filters/reverb.aai");

            // Kick off async project loading
            if (pmso.autoLoadProject)
                LoadProject(pmso.projectId, pmso.authToken, pmso.autoPlayProject);
        }

        //-----------------------------------------------
        private void OnDestroy()
        {
            if (debugMode)
                Debug.Log($"PM> Core.OnDestroy(): sceneUnloadedIsHooked = {sceneUnloadedIsHooked}");

            if (null != plusMusicAccount)
            {
                plusMusicAccount.DeviceId = SystemInfo.deviceUniqueIdentifier;
                plusMusicAccount.PluginVolume = pluginVolume;

                pmso.projectId = currentProject.id;
                pmso.authToken = currentProject.authToken;
                plusMusicAccount.UpdateSettings(pmso);
            }

            if (sceneUnloadedIsHooked)
            { 
                SceneManager.sceneUnloaded -= OnSceneUnloaded;
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        //-----------------------------------------------
        private void OnSceneLoaded(Scene current, LoadSceneMode mode)
        {
            Debug.LogFormat("PM> Core.OnSceneLoaded(): {0}", current.name);

            killQueue = false;
        }

        //-----------------------------------------------
        private void OnSceneUnloaded(Scene current)
        {
            Debug.LogFormat("PM> Core.OnSceneUnloaded(): {0}", current.name);

            if (!pmso.playAcrossScenes)
            {
                killQueue = true;
                StopPlay();
            }
        }


        #endregion
        //----------------------------------------------------------
        // Async Core functions
        //----------------------------------------------------------
        #region async_core_functions


        //----------------------------------------------------------
        /** $$$ TODO:
        * Maybe try to play at a certain time index and eliminate all the preprocessed audio clips???
        * 
        * audioSource.time = 13.21f
        * audioSource.Play();
        * audioSource.SetScheduledEndTime(AudioSettings.dspTime+(14.57f-13.21f));
        */
        private IEnumerator CurveTransitionFade(
            PMTransitionInfo transition, PMArrangement nextArrangement, 
            AudioSource[] nextSource, AudioSource[] currentSource)
        {
            float journey = 0.0f;
            float volumeOut = pluginVolume;
            float volumeIn = (transition.useMainVolume ? transition.mainVolume : pluginVolume);

            // Set the next source volume to zero and start playing
            if (pmso.useAudioLayers)
            {
                for (int l = 1; l<audioGroupLayers.Length; l++)
                {
                    nextSource[l].volume = 0.001f;
                    nextSource[l].clip = nextArrangement.layers[l];
                    nextSource[l].Play();
                }
            }
            else
            {
                nextSource[LayerFullMix].volume = 0.001f;
                nextSource[LayerFullMix].clip = nextArrangement.layers[LayerFullMix];
                nextSource[LayerFullMix].Play();
            }

            isAudioPlaying = true;

            // Blend in next source and blend out current source
            while (journey <= transition.duration)
            {
                journey = journey + Time.deltaTime;
                float percent = Mathf.Clamp01(journey / transition.duration);
                float curvePercent = percent;
                if (transition.curve != null && transition.curve.keys.Length >= 1)
                    curvePercent = transition.curve.Evaluate(percent);

                if (pmso.useAudioLayers)
                {
                    for (int l = 1; l<audioGroupLayers.Length; l++)
                    {
                        // NOTE: We need to use -1 for GetByIndex() since the main FullMix volume is not part of that Class
                        nextSource[l].volume = curvePercent * (transition.useLayerVolumes ? transition.layerVolumes.GetByIndex(l-1) : layerVolumes[l]);
                        currentSource[l].volume = layerVolumes[l] * (1 - curvePercent);
                    }
                }
                else
                {
                    nextSource[LayerFullMix].volume = curvePercent * volumeIn;
                    currentSource[LayerFullMix].volume = volumeOut * (1 - curvePercent);
                }

                yield return null;
            }

            // Stop current source and set global volume(s)
            // JIRA PP-29: We also update the .clip AudioClip here to make sure that we can
            // switch tracks in realtime without having previous audio linger about
            if (pmso.useAudioLayers)
            {
                for (int l = 1; l<audioGroupLayers.Length; l++)
                { 
                    layerVolumes[l] = nextSource[l].volume;
                    currentSource[l].Stop();
                    currentSource[l].clip = nextSource[l].clip;
                }
            }
            else
            { 
                pluginVolume = volumeIn;
                currentSource[LayerFullMix].Stop();
                currentSource[LayerFullMix].clip = nextSource[LayerFullMix].clip;
            }

            if (!transition.returnToPrevious && 0.0f == transition.timeToLive)
                currentProject.transition = CopyTransition(transition);

            OnArrangementChanged?.Invoke(transition.tag);
        }

        //----------------------------------------------------------
        private IEnumerator PlayArrangementTransition(PMTransitionInfo transition)
        {
            string func_name = "Core.PlayArrangementTransition()";
            if (debugMode)
            { 
                Debug.Log($"PM> {func_name}: {transition.tag}, {transition.mainVolume}");
                if (pmso.useAudioLayers)
                {
                    Debug.Log($"PM> transition volumes: {transition.layerVolumes.bass}, {transition.layerVolumes.drums}, " +
                        $"{transition.layerVolumes.topMix}, {transition.layerVolumes.vocals}");
                }
            }

            // Ignore if it is of the same type and canTransitionToItself is disabled
            if ((transition.tag == currentProject.transition.tag) && (!transition.canTransitionToItself))
                yield break;

            // Find the first arrangement that matches our type
            // $$$ TODO: Find a good way to handle multiple of same type
            PMArrangement arrangement = GetArrangementFromType(currentProject.trackIndex, (int)transition.tag);
            if (null == arrangement)
                Debug.LogWarning($"PM> Core.PlayArrangementTransition(): Missing '{transition.tag}' Arrangement in Track!");
            else
            {
                string msg = String.Format("Playing arrangement '{0}' from '{1}' ...",
                    arrangement.name, currentProject.tracks[currentProject.trackIndex].name);

                if (debugMode)
                    Debug.LogFormat("PM> {0}", msg);

                OnRealTimeStatus?.Invoke(msg);
            }

            float delay = GetTimeForTransition(transition.timing);
            if (delay < 0.0f)
            {
                Debug.LogWarning("PM> Core.PlayArrangementTransition(): Could not get timing value, aborting arrangement play ...");
                yield break;
            }

            if (pmso.doPingbacks)
                SendPingBackInfo(
                    new PMPingBackInfo
                    {
                        eventText = "Transition", pingProjectId = currentProject.id,
                        pingArrangementId = arrangement.id, pingTag = transition.tag.ToString(), 
                        pingTransitionType = "CurveTransition", pingTransitionTiming = transition.timing.ToString(),
                        pingTransitionDelay = delay, isUsingStinger = ((int)transition.soundFX > 0)
                    }
                );

            if (debugMode)
                Debug.LogFormat(
                    "PM> Core.PlayArrangementTransition(): delay = {0}, duration = {1}", 
                    delay, transition.duration);

            yield return new WaitForSeconds(delay);

            if (PMSoundFX.None != transition.soundFX)
                PlaySoundFX(transition.soundFX);

            if (transition.returnToPrevious || transition.timeToLive > 0.0f)
            {
                if (null != coroutinePool["QueueOnShotTransition"])
                    StopCoroutine(coroutinePool["QueueOnShotTransition"]);

                coroutinePool["QueueOnShotTransition"] = StartCoroutine(
                    QueueOnShotTransition(currentProject.transition, transition));
            }

            if (null != arrangement)
            {
                if (null != coroutinePool["CurveTransitionFade"])
                    StopCoroutine(coroutinePool["CurveTransitionFade"]);

                currentLayers = layerAudioSources1;
                nextLayers = layerAudioSources2;

                if (audioSource1Playing)
                { 
                    nextAudioSource = layerAudioSources2[LayerFullMix];
                }
                else
                { 
                    nextAudioSource = layerAudioSources1[LayerFullMix];
                    currentLayers = layerAudioSources2;
                    nextLayers = layerAudioSources1;
                }

                if ((int)transition.soundFX > 0)
                    PlaySoundFX(transition.soundFX);

                coroutinePool["CurveTransitionFade"] = StartCoroutine(
                    CurveTransitionFade(
                        transition, arrangement, nextLayers, currentLayers));

                currentLayers = nextLayers;
                currentAudioSource = nextAudioSource;
                currentProject.arrangementId = arrangement.id;
                currentProject.arrangementIndex = GetArrangementIndexFromId(currentProject.trackId, arrangement.id);
                currentProject.audio = currentAudioSource;
                currentProject.transition = transition;
                audioSource1Playing = !audioSource1Playing;
            }
        }

        //----------------------------------------------------------
        private IEnumerator QueueOnShotTransition(PMTransitionInfo previousTransition, PMTransitionInfo currentTransition)
        {
            if (PMTags.none == previousTransition.tag)
                yield break;

            float clipLength = currentProject.audio.clip.length;
            float timeToWait = clipLength - Mathf.Min(currentTransition.duration, clipLength);

            if (currentTransition.timeToLive > 0.0f)
                timeToWait = Mathf.Min(currentTransition.timeToLive, clipLength);

            if (timeToWait < 0.0f)
                timeToWait = 0.0f;

            yield return new WaitForSeconds(timeToWait);

            PMTransitionInfo transition;
            if (currentTransition.returnToPrevious)
                transition = CopyTransition(previousTransition);
            else
                transition = CopyTransition(currentTransition);

            if (!killQueue)
            {
                if (debugMode)
                    Debug.Log("ReturnToPreviousArrangement(): " + transition.tag);
                PlayArrangement(transition);
            }
            else
            {
                if (debugMode)
                    Debug.Log("QueueOnShotTransition(): Abort requested ...");
                killQueue = false;
            }
        }

        //----------------------------------------------------------
        private IEnumerator UploadPingBackAsync(string dataToSend)
        {
            string finalUrl = settings.base_url + "ping-backs";

            if (pmso.logRequestUrls)
                Debug.LogFormat("PM> UploadPingBack(): finalURL = {0}", finalUrl);
            if (debugMode)
                Debug.LogFormat("PM> dataToSend = {0}", dataToSend);

            var webRequest = new UnityWebRequest(finalUrl, "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(dataToSend);
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.SetRequestHeader("x-api-key", currentProject.authToken);

            yield return webRequest.SendWebRequest();
            string jsonResponse = webRequest.downloadHandler.text;

            if (debugMode)
                Debug.LogFormat("PM> responseCode = {0}, result = {1}", webRequest.responseCode, webRequest.result);

            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                OnRealTimeStatus?.Invoke("PingBack failed");
                Debug.LogError("PM> ERROR:PingBack failed: " + webRequest.error);
                if (pmso.logServerResponses)
                    Debug.LogFormat("PM> response = {0}", Regex.Replace(jsonResponse, @"\r\n?|\n", ""));

                // Cleanup ...
                webRequest.Dispose();
                jsonToSend = null;
                jsonResponse = null;
                yield break;
            }
            else
            {
                if (webRequest.responseCode == 200 || webRequest.responseCode == 201)
                {
                    if (debugMode)
                        Debug.Log("PM> PingBackInfo successfully sent");
                    if (pmso.logServerResponses)
                        Debug.LogFormat("PM> response = {0}", Regex.Replace(jsonResponse, @"\r\n?|\n", ""));
                }
                else if (webRequest.responseCode == 403)
                {
                    // $$$ ???
                    //licenseLimited = true;
                    Debug.LogWarning("PM> responseCode = 403");
                }
                else
                {
                    Debug.LogError("PM> ERROR:PingBack failed!");
                    if (pmso.logServerResponses)
                        Debug.LogFormat("PM> response = {0}", Regex.Replace(jsonResponse, @"\r\n?|\n", ""));
                }

                // Cleanup ...
                webRequest.Dispose();
                jsonToSend = null;
                jsonResponse = null;
                yield break;
            }
        }


        #endregion
        //----------------------------------------------------------
        // Callback Event functions
        //----------------------------------------------------------
        #region callback_event_functions


        //----------------------------------------------------------
        private void SendOnProjectInfoLoaded()
        {
            if (debugMode)
                Debug.Log($"PM> Core.SendOnProjectInfoLoaded()");

            PMMessageProjectInfo projectData = GetProjectInfo();

            OnProjectInfoLoaded?.Invoke(projectData);
        }


        #endregion
        //----------------------------------------------------------
        // Misc helper functions, Init and Cleanup
        //----------------------------------------------------------
        #region misc_functions


        //----------------------------------------------------------
        /**
        * @brief Initialize the core plugin
        * 
        * This is called before Start() and should contain any initial heavy lifting to
        * allow for a smooth start of the game
        */
        private bool Init()
        {
            Debug.Log("PM> ------------------ PlusMusicCore (Core) ----------------------");
            if (debugMode)
                Debug.Log("PM> Core.Init()");

            try
            {
                // Set the package data
                plusMusicAccount.UpdatePackageData();
                if (null != plusMusicAccount.PackageData && null != plusMusicAccount.PackageData.version)
                    pluginVersion = plusMusicAccount.PackageData.version;
                else
                    Debug.LogError("PM> Core.Init(): package.json is missing or corrupt!");

                // Damn, this is ugly ...
                int[] vals = (int[])Enum.GetValues(typeof(PMTags));
                arrangementTypeMax = vals.Max();

                // Attach the PlusMusicEventManager at runtime
                gameObject.AddComponent<PlusMusicEventManager>();
                EventManager = GetComponent<PlusMusicEventManager>();

                // Configure our audio sources
                pluginVolume = plusMusicAccount.PluginVolume;

                audioMixerPM = Resources.Load<AudioMixer>("Audio/PMAudioMixer");
                audioGroupLayers  = audioMixerPM.FindMatchingGroups("FullMix");
                audioGroupSoundFX = audioMixerPM.FindMatchingGroups("SoundFX")[0];

                layerAudioSources1 = new AudioSource[audioGroupLayers.Length];
                layerAudioSources2 = new AudioSource[audioGroupLayers.Length];
                for (int s = 0; s<audioGroupLayers.Length; s++)
                {
                    layerAudioSources1[s] = gameObject.AddComponent<AudioSource>();
                    layerAudioSources1[s].outputAudioMixerGroup = audioGroupLayers[s];
                    layerAudioSources1[s].playOnAwake = false;
                    layerAudioSources1[s].loop = true;
                    layerAudioSources1[s].volume = 1.0f;

                    layerAudioSources2[s] = gameObject.AddComponent<AudioSource>();
                    layerAudioSources2[s].outputAudioMixerGroup = audioGroupLayers[s];
                    layerAudioSources2[s].playOnAwake = false;
                    layerAudioSources2[s].loop = true;
                    layerAudioSources2[s].volume = 1.0f;
                }

                audioSourceSoundFx = gameObject.AddComponent<AudioSource>();
                audioSourceSoundFx.playOnAwake = true;
                audioSourceSoundFx.loop = false;
                audioSourceSoundFx.volume = 1.0f;
                audioSourceSoundFx.outputAudioMixerGroup = audioGroupSoundFX;

                currentLayers = layerAudioSources1;
                nextLayers = layerAudioSources2;
                currentAudioSource = layerAudioSources1[LayerFullMix];
                nextAudioSource = layerAudioSources2[LayerFullMix];
                audioSource1Playing = true;

                // Load all sound effects
                int numDuplicates = 0;
                AudioClip[] sfx = Resources.LoadAll<AudioClip>("SoundFX");
                for (int i = 0; i < sfx.Length; i++)
                {
                    if (!soundFxClips.ContainsKey(sfx[i].name))
                        soundFxClips.Add(sfx[i].name, sfx[i]);
                    else
                        numDuplicates++;
                }
                if (numDuplicates > 0)
                    Debug.LogWarning("PM> Core.Init(): Resources/SoundFX contains duplicate files!");

                coroutinePool.Add("CurveTransitionFade", null);
                coroutinePool.Add("QueueOnShotTransition", null);

                return true;

            } catch (Exception e)
            {
                Debug.LogErrorFormat("PM> ERROR:Core.Init(): {0}", e.ToString());
                return false;
            }
        }

        //----------------------------------------------------------
        /**
        * @brief Send usage data to PlusMusic
        * @param pingBackInfo
        */
        private void SendPingBackInfo(PMPingBackInfo pingBackInfo)
        {
            if (!pmso.doPingbacks) return;
            if (pmso.offlineMode) return;

            if (debugMode)
                Debug.Log("PM> Core.SendPingBackInfo()");

            PMPingBackData pingBackData = new PMPingBackData()
            {
                os = SystemInfo.operatingSystem,
                event_text = pingBackInfo.eventText,
                device_id = SystemInfo.deviceUniqueIdentifier,
                in_editor = Application.isEditor,
                platform = "Unity",
                title = Application.productName,
                connected = Application.internetReachability.ToString(),
                is_using_stinger = pingBackInfo.isUsingStinger,
                project_id = pingBackInfo.pingProjectId,
                arrangement_id = ((0 == pingBackInfo.pingArrangementId) ? -1 : pingBackInfo.pingArrangementId),
                arrangement_type = pingBackInfo.pingTag,
                transition_type = pingBackInfo.pingTransitionType,
                transition_timing = pingBackInfo.pingTransitionTiming,
                transition_delay = pingBackInfo.pingTransitionDelay,
                time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                web_url = "",
                plugin_version = pluginVersion,
                play_id = ""
            };

            string sendingData = JsonUtility.ToJson(pingBackData);

            // NOTE: 'event' is a reserved keyword and can't be used in the PingBack data class
            // We use 'event_text' instead and replace it with 'event' in the data string
            // We also replace "" and ":-1" with null to indicate missing string and number values
            sendingData = "{\"ping_backs\":[" +
                sendingData.Replace("event_text", "event")
                    .Replace("\"\"", "null")
                    .Replace(": -1", ": null")
                    .Replace(":-1", ":null") +
                "]}";

            StartCoroutine(UploadPingBackAsync(sendingData));
        }

        //----------------------------------------------------------
        /**
        * @brief Clean up current project data
        */
        private void PMProject_Cleanup()
        {
            Debug.Log("PM> PMProject_Cleanup()");

            // Lets hope the GC gets the hints
            if (null != currentProject)
            {
                if (null != currentProject.tracks)
                {
                    for (int s=0; s<currentProject.tracks.Count; s++)
                    {
                        if (null != currentProject.tracks[s].arrangements)
                        {
                            for (int a = 0; a<currentProject.tracks[s].arrangements.Count; a++)
                            {
                                if (null != currentProject.tracks[s].arrangements[a].segment_clips)
                                {
                                    currentProject.tracks[s].arrangements[a].segment_clips.Clear();
                                    currentProject.tracks[s].arrangements[a].segment_clips = null;
                                }
                                if (null != currentProject.tracks[s].arrangements[a].layers[LayerFullMix])
                                {
                                    AudioClip.Destroy(currentProject.tracks[s].arrangements[a].layers[LayerFullMix]);
                                    currentProject.tracks[s].arrangements[a].layers[LayerFullMix] = null;
                                }
                            }
                            currentProject.tracks[s].arrangements.Clear();
                            currentProject.tracks[s].arrangements = null;
                        }
                    }
                    currentProject.tracks.Clear();
                    currentProject.tracks = null;
                }
                currentProject = null;
            }
        }

        //----------------------------------------------------------
        /**
        * @brief Clean up track data
        * @param trackIndex - Index into the projectInfo.tracks array
        */
        private void PMTrack_Cleanup(int trackIndex)
        {
            Debug.LogFormat("PM> PMTrack_Cleanup({0})", trackIndex);

            // Lets hope the GC gets the hints
            if (null != currentProject)
            {
                if (null != currentProject.tracks)
                {
                    if ((trackIndex > -1) && (trackIndex < currentProject.tracks.Count))
                    {
                        if (null != currentProject.tracks[trackIndex].arrangements)
                        {
                            for (int a = 0; a<currentProject.tracks[trackIndex].arrangements.Count; a++)
                            {
                                if (null != currentProject.tracks[trackIndex].arrangements[a].segment_clips)
                                {
                                    currentProject.tracks[trackIndex].arrangements[a].segment_clips.Clear();
                                    currentProject.tracks[trackIndex].arrangements[a].segment_clips = null;
                                }
                                if (null != currentProject.tracks[trackIndex].arrangements[a].layers[LayerFullMix])
                                {
                                    AudioClip.Destroy(currentProject.tracks[trackIndex].arrangements[a].layers[LayerFullMix]);
                                    currentProject.tracks[trackIndex].arrangements[a].layers[LayerFullMix] = null;
                                }
                            }
                            currentProject.tracks[trackIndex].arrangements.Clear();
                            currentProject.tracks[trackIndex].arrangements = null;
                        }
                    }
                    currentProject.tracks.RemoveAt(trackIndex);
                }
            }
        }

        //----------------------------------------------------------
        /**
        * @brief Return an absolute path for the supplied source path
        * @param source_path
        * @param absolute - If false (default) the source_path is prepended with Application.dataPath
        */
        //----------------------------------------------------------
        private string GetAbsoluteFilePath(string source_path, bool absolute)
        {
            if (!absolute)
                // We force Unix separators for all platforms
                // And yes, "/" separators have worked on Windows since the '90s ...
                return Path.Combine(Application.dataPath + "/", source_path);
            else
                return source_path;
        }

        //----------------------------------------------------------
        private bool DoesFileExist(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            return fi.Exists;
        }

        //----------------------------------------------------------
        private bool CheckFileExtension(string filename, string extension)
        {
            FileInfo fi = new FileInfo(filename);
            return (fi.Extension.ToLower() == extension);
        }

        //----------------------------------------------------------
        private Int64 GetFileSize(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            return fi.Length;
        }

        //----------------------------------------------------------
        // This function copies the structure data into a byte[]
        //
        // https://www.codeproject.com/Articles/11271/Read-and-Write-Structures-to-Files-with-NET
        //----------------------------------------------------------
        private byte[] StructToByteArray(object src_struct)
        {
            try
            {
                // Set the buffer to the correct size 
                byte[] buffer = new byte[Marshal.SizeOf(src_struct)];

                // Allocate the buffer to memory and pin it so that GC cannot use the space (Disable GC) 
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                // Copy the struct into the byte[] 
                Marshal.StructureToPtr(src_struct, handle.AddrOfPinnedObject(), false);

                handle.Free(); // Allow GC to do its job 

                return buffer;

            } catch (Exception ex)
            {
                throw ex;
            }
        }

        //----------------------------------------------------------
        // TODO: $$$ We need to test if this leaks memory
        // The destination object reference is not overwritten here,
        // the data is copied into a new object
        //----------------------------------------------------------
        private object ByteArrayToStruct(byte[] src_buffer, object dst_struct, System.Type obj_type)
        {
            try
            {
                // Allocate the buffer to memory and pin it so that GC cannot use the space (Disable GC) 
                GCHandle handle = GCHandle.Alloc(src_buffer, GCHandleType.Pinned);

                // Copy the byte[] into the struct
                dst_struct = (object)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), obj_type);

                handle.Free(); // Allow GC to do its job 

            } catch (Exception ex)
            {
                throw ex;
            }

            return dst_struct;
        }

        //----------------------------------------------------------
        /**
        * @brief Return the next closest time to the current audio playback time from an array of floats
        * @param timeValues
        * @return The next closest time
        */
        private float GetNextClosestTime(float[] timeValues)
        {
            float nextClosestTime = 0.0f;

            if (null != timeValues)
            {
                foreach (float timeVal in timeValues)
                {
                    if (currentAudioSource.time < timeVal)
                    {
                        nextClosestTime = timeVal - currentAudioSource.time;
                        break;
                    }
                }
            }
            else
                Debug.LogError("PM> Core.GetNextClosestTime(): timeValues is null!");

            return nextClosestTime;
        }

        //----------------------------------------------------------
        /**
        * @brief Return the next closest time for a transition
        * @param transitionTiming
        * @return The next closest time or 0.0f
        */
        private float GetTimeForTransition(PMTimings transitionTiming)
        {
            return transitionTiming switch
            {
                PMTimings.nextBeat => TimeNextBeat(),
                PMTimings.nextBar => TimeNextBar(),
                PMTimings.now => 0.0f,
                _ => 0.0f,
            };
        }

        //----------------------------------------------------------
        private void EventBroker(PMEventTypes type, object args = null)
        {
            if (debugMode)
            {
                Debug.Log($"PM> Core.EventBroker({type})");
                if (null != args)
                    Debug.Log($"PM> Core.EventBroker(): args = {args}");
            }

            EventManager.SetStatus(PMEventStatus.EventAborted);

            switch (type)
            {
                case PMEventTypes.EventGetProjectInfo:
                    LoadProjectInfo((PMProject)args);
                    break;
                //case PMEventTypes.EventGetDefaultProjectInfo:
                //    GetDefaultProjectInfo();
                //    break;
                case PMEventTypes.EventGetTrackInfo:
                    LoadTrackInfo((PMProject)args);
                    break;
                case PMEventTypes.EventGetTrackAudio:
                    LoadTrackAudio((PMProject)args);
                    break;
                case PMEventTypes.EventGetTrackImage:
                    LoadTrackImage((PMProject)args);
                    break;
                case PMEventTypes.EventPlayCurrent:
                    // JIRA PP-33:
                    // Replaced PlayCurrent() with PlayTrack() since anything other than the default
                    // track isn't actually the current one yet
                    PMProject prj = (PMProject)args;
                    PlayTrack(prj.trackProgress.id);
                    EventManager.SetStatus(PMEventStatus.EventWasSuccessful);
                    EventManager.ContinueProcessing();
                    break;
                default:
                    Debug.LogErrorFormat("PM> ERROR:Core.EventBroker(): Invalid event type! {0}", type);
                    break;
            }
        }

        //----------------------------------------------------------
        // 
        // https://signalsmith-audio.co.uk/writing/2021/cheap-energy-crossfade/#cross-fading-curves-amplitude-preserving-cross-fade
        // 
        /**
        * @brief Apply a crossfade (in place) between the end of Segment1 and the beginnig of Segment2
        * @param source - The full source audio that includes both segments out of sequence
        * @param destination - The arrangement audio that includes both segments in sequence
        * @param offset_dst - Offset (in samples) to the transition point in the arrangement between the two segments
        * @param offset_seg1_end - Offset (in samples) of the end of the first segment in source
        * @param offset_seg2_start - Offset (in samples) of the start of the second segment in source
        * @param timeOverlap - Overlaptime for the crossfade, in seconds (default: 0.005 = 5ms)
        * @param frequency - Audio frequency
        * @param channels - Number of channels
        */
        private void CrossFadeSegments(
            float[] source, float[] destination,
            int offset_dst, int offset_seg1_end, int offset_seg2_start,
            float timeOverlap, int frequency, int channels)
        {
            if (debugMode)
                Debug.Log("PM> offset_dst, offset_seg1_end, offset_seg2_start, timeOverlap = " +
                    $"{offset_dst}, {offset_seg1_end}, {offset_seg2_start}, {timeOverlap}");

            int overlap_samples = (int)(timeOverlap * (float)frequency);
            int overlap_samples_2 = overlap_samples / 2;
            int overlap_with_channels = overlap_samples_2 * channels;

            if (debugMode)
                Debug.Log("PM> overlap_samples, overlap_samples_2, overlap_with_channels = " +
                    $"{overlap_samples}, {overlap_samples_2}, {overlap_with_channels}");

            // Time domain step values
            float time_val = 0.0f;
            float step_val = 1.0f / ((float)overlap_samples_2 * 2.0f);

            // Needed buffer offsets
            int ofsr = offset_dst - overlap_with_channels;
            int ofs1 = offset_seg1_end - overlap_with_channels;
            int ofs2 = offset_seg2_start - overlap_with_channels;

            // NOTE: We need to adjust with -1 here because we're using these as backwards indices in the forward loop below
            int endr = offset_dst + overlap_with_channels - 1;
            int end1 = offset_seg1_end + overlap_with_channels - 1;
            int end2 = offset_seg2_start + overlap_with_channels - 1;

            if (debugMode)
                Debug.Log("PM> ofs1, end1 | ofs2, end2 | ofsr, endr = " +
                    $"{ofs1}, {end1} | {ofs2}, {end2} | {ofsr}, {endr}");

            // We loop over half the window size adding values at both the start and end of the overlap.
            // We can do this since both parts of the blend curve are the same, just inverted. This saves us some computation time.
            for (int s = 0; s < overlap_samples_2; s++)
            {
                float gain2 = (time_val * time_val) * (3.0f - (2.0f * time_val));
                float gain1 = 1.0f - gain2;

                for (int c = 0; c<channels; c++)
                {
                    destination[ofsr++] = (source[ofs1++] * gain1) + (source[ofs2++] * gain2);
                    destination[endr--] = (source[end1--] * gain2) + (source[end2--] * gain1);
                }
                time_val += step_val;
            }
        }

        //----------------------------------------------------------
        private AudioClip DestroyClip(AudioClip the_clip)
        {
            if (null != the_clip)
                AudioClip.Destroy(the_clip);

            return null;
        }


        #endregion
        //----------------------------------------------------------
        // AAI functions
        //----------------------------------------------------------
        #region aai_functions


        //----------------------------------------------------------
        private float[] ExtractFilterFromAAI(string resourceName)
        {
            string func_name = "Core.ExtractFilterFromAAI()";
            if (debugMode)
                Debug.Log($"PM> {func_name}: resourceName = {resourceName}");

            float[] filter_data = null;

            // Try to read the AAI file
            try
            {
                // Read in the filter file as a resource
                TextAsset bin_data  = Resources.Load<TextAsset>(resourceName);
                Stream bin_stream   = new MemoryStream(bin_data.bytes);
                BinaryReader reader = new BinaryReader(bin_stream);

                // This is actually ignored by the constructor but we need to pass it in anyways
                // in order for the initialization do be done properly
                bool use_defaults = true;

                // Create default header and read it in
                PMFileHeaders.AAI header = new PMFileHeaders.AAI(use_defaults);
                byte[] fileHeader = new byte[header.size];
                reader.Read(fileHeader, 0, header.size);
                header = (PMFileHeaders.AAI)ByteArrayToStruct(fileHeader, header, typeof(PMFileHeaders.AAI));

                if (debugMode)
                    Debug.LogFormat(
                        "PM> aai signature/size/chunks = '{0}' / {1} / {2}",
                        header.GetSigString(), header.size, header.chunks);

                UInt32 raw = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("RAW "), 0);

                PMFileHeaders.AAI_CHUNK_HEADER temp_header = new PMFileHeaders.AAI_CHUNK_HEADER(use_defaults);
                int header_size = (UInt16)Marshal.SizeOf<PMFileHeaders.AAI_CHUNK_HEADER>();
                byte[] temp_header_bytes = new byte[header_size];

                PMFileHeaders.AAI_RAW chunk_header = new PMFileHeaders.AAI_RAW(use_defaults);
                byte[] chunk_header_bytes = new byte[chunk_header.size];

                // Loop over the chunks and load all matching filters
                while (true)
                {
                    // Read each chunk and skip any that aren't raw
                    int bytes_read = reader.Read(temp_header_bytes, 0, header_size);
                    if (bytes_read < header_size)
                        break;

                    temp_header = (PMFileHeaders.AAI_CHUNK_HEADER)ByteArrayToStruct(
                        temp_header_bytes, temp_header, typeof(PMFileHeaders.AAI_CHUNK_HEADER));

                    // Skip if this is not a raw chunk
                    if (temp_header.signature != raw)
                    {
                        if (debugMode)
                            Debug.LogFormat("PM> Skipping chunk '{0}' ...", temp_header.GetSigString());

                        // Skip the rest of the header up to the last element, the data size
                        int header_skip_bytes = temp_header.size - sizeof(UInt32) - sizeof(UInt16) - sizeof(UInt64);
                        byte[] header_bytes_to_skip = new byte[header_skip_bytes];
                        reader.Read(header_bytes_to_skip, 0, header_skip_bytes);

                        // Read the data size
                        byte[] header_data_size = new byte[sizeof(UInt64)];
                        reader.Read(header_data_size, 0, sizeof(UInt64));
                        UInt64 chunk_data_size = BitConverter.ToUInt64(header_data_size, 0);
                        reader.BaseStream.Seek((long)chunk_data_size, SeekOrigin.Current);

                        // Skip ahead to the next chunk
                        continue;
                    }

                    // We got a raw chunk, lets read it
                    reader.BaseStream.Seek((long)-header_size, SeekOrigin.Current);
                    reader.Read(chunk_header_bytes, 0, chunk_header.size);
                    chunk_header = (PMFileHeaders.AAI_RAW)ByteArrayToStruct(
                    chunk_header_bytes, chunk_header, typeof(PMFileHeaders.AAI_RAW));

                    // Check the type
                    if (chunk_header.type != (UInt16)PMRawTypes.filter)
                    {
                        if (debugMode)
                            Debug.LogFormat(
                                "PM> Wrong Type, skipping '{0}' ... {1} != {2}",
                                chunk_header.GetSigString(), chunk_header.type, PMRawTypes.filter);

                        // Skip ahead to the next chunk
                        reader.BaseStream.Seek((long)chunk_header.data, SeekOrigin.Current);
                        continue;
                    }

                    if (debugMode)
                    {
                        Debug.LogFormat("PM> chunk signature / type = '{0}' / {1}",
                            chunk_header.GetSigString(), (PMTags)chunk_header.type);
                    }

                    // Read the raw data as bytes
                    byte[] data_bytes = new byte[chunk_header.data];
                    reader.Read(data_bytes, 0, (int)chunk_header.data);
                    byte filter_offset = 0xAA;

                    // Deflate the filter data
                    for (int b = 0; b<(int)chunk_header.data; b++)
                        data_bytes[b] ^= filter_offset++;

                    // Save to temp folder
                    string loaclPath = GetAbsoluteFilePath(String.Format(
                        $"{cachePath}/{cachePathTracks}/" +
                        $"filter{PMExtensions.OGG}"
                    ), false);
                    SaveBinToCache(data_bytes, loaclPath);

                    // Load as audio clip
                    string loaclPathFile = "file://" + loaclPath;
                    AudioClip filter_clip = WebApiGetAudioClip(loaclPathFile, AudioType.OGGVORBIS);
                    if (null != filter_clip)
                    {
                        filterLength = filter_clip.samples;

                        // Check if the source is mono. If no, we convert it.
                        if (filter_clip.channels > 1)
                        {
                            float[] filter_temp = new float[filter_clip.samples * filter_clip.channels];
                            filter_clip.GetData(filter_temp, 0);
                            DestroyClip(filter_clip);
                            filter_data = ConvertToMono(filter_temp, filter_clip.samples, filter_clip.channels);
                        }
                        else
                        {
                            filter_data = new float[filterLength];
                            filter_clip.GetData(filter_data, 0);
                        }

                        /*
                        string clipName = GetAbsoluteFilePath(String.Format(
                            $"{cachePath}/{cachePathTracks}/" +
                            $"filter{PMExtensions.WAV}"
                        ), false);
                        SaveClipToCache(filter_data, filter_clip.samples, 1, filter_clip.frequency, clipName);
                        */
                    }

                    // Cleanup
                    File.Delete(loaclPath);
                    if (null != filter_clip)
                        DestroyClip(filter_clip);
                }

                reader.Dispose();
                bin_stream.Dispose();

            } catch (Exception e)
            {
                Debug.LogError($"PM> ERROR:{func_name}: {e.ToString()}");
            }

            return filter_data;
        }

        //----------------------------------------------------------
        private void ExtractLayersFromAAI(int track_index, string filepath)
        {
            if (debugMode)
                Debug.LogFormat("PM> Core.ExtractLayersFromAAI({0})", filepath);

            // Try to read the AAI file
            try
            {
                if (!DoesFileExist(filepath))
                {
                    Debug.LogError("PM> ERROR:Core.ExtractLayersFromAAI(): File does not exists!");
                    return;
                }
                if (!CheckFileExtension(filepath, PMExtensions.AAI))
                {
                    Debug.LogErrorFormat("PM> ERROR:Core.ExtractLayersFromAAI(): File Extension is not {0}!", PMExtensions.AAI);
                    return;
                }

                // This is actually ignored by the constructor but we need to pass it in anyways
                // in order for the initialization do be done properly
                bool use_defaults = true;

                // Create default header
                PMFileHeaders.AAI header = new PMFileHeaders.AAI(use_defaults);
                if (GetFileSize(filepath) < header.size)
                {
                    Debug.LogError("PM> ERROR:Core.ExtractLayersFromAAI(): File appears to be corrupt!");
                    return;
                }

                // Read in the AAI file
                FileStream stream = new FileStream(filepath, FileMode.Open);
                BinaryReader reader = new BinaryReader(stream);

                // Read the header
                byte[] fileHeader = new byte[header.size];
                reader.Read(fileHeader, 0, header.size);
                header = (PMFileHeaders.AAI)ByteArrayToStruct(fileHeader, header, typeof(PMFileHeaders.AAI));

                if (debugMode)
                    Debug.LogFormat(
                        "PM> aai signature/size/chunks = '{0}' / {1} / {2}",
                        header.GetSigString(), header.size, header.chunks);

                UInt32 loud = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("LOUD"), 0);

                PMFileHeaders.AAI_CHUNK_HEADER temp_header = new PMFileHeaders.AAI_CHUNK_HEADER(use_defaults);
                int header_size = (UInt16)Marshal.SizeOf<PMFileHeaders.AAI_CHUNK_HEADER>();
                byte[] temp_header_bytes = new byte[header_size];

                PMFileHeaders.AAI_AUDIO chunk_header = new PMFileHeaders.AAI_AUDIO(use_defaults);
                byte[] chunk_header_bytes = new byte[chunk_header.size];

                // Loop over the chunks and load the audio for each stem
                while (true)
                {
                    // Read each chunk and skip any that aren't audio
                    int bytes_read = reader.Read(temp_header_bytes, 0, header_size);
                    if (bytes_read < header_size)
                    {
                        //Debug.Log("PM> EOF!");
                        break;
                    }
                    temp_header = (PMFileHeaders.AAI_CHUNK_HEADER)ByteArrayToStruct(
                        temp_header_bytes, temp_header, typeof(PMFileHeaders.AAI_CHUNK_HEADER));

                    // Skip if this is not an audio chunk
                    if (temp_header.signature != loud)
                    {
                        if (debugMode)
                            Debug.LogFormat("PM> Skipping chunk '{0}' ...", temp_header.GetSigString());

                        // Skip the rest of the header up to the last element, the data size
                        int header_skip_bytes = temp_header.size - sizeof(UInt32) - sizeof(UInt16) - sizeof(UInt64);
                        byte[] header_bytes_to_skip = new byte[header_skip_bytes];
                        reader.Read(header_bytes_to_skip, 0, header_skip_bytes);

                        // Read the data size
                        byte[] header_data_size = new byte[sizeof(UInt64)];
                        reader.Read(header_data_size, 0, sizeof(UInt64));
                        UInt64 chunk_data_size = BitConverter.ToUInt64(header_data_size, 0);
                        reader.BaseStream.Seek((long)chunk_data_size, SeekOrigin.Current);

                        // Skip ahead to the next chunk
                        continue;
                    }

                    // We got an audio chunk, lets load it
                    reader.BaseStream.Seek((long)-header_size, SeekOrigin.Current);
                    reader.Read(chunk_header_bytes, 0, chunk_header.size);
                    chunk_header = (PMFileHeaders.AAI_AUDIO)ByteArrayToStruct(
                        chunk_header_bytes, chunk_header, typeof(PMFileHeaders.AAI_AUDIO));

                    // Check the track id
                    UInt64 track_id = (UInt64)currentProject.tracks[track_index].track_id;
                    if (chunk_header.id != track_id)
                    {
                        if (debugMode)
                            Debug.LogFormat(
                                "PM> Wrong Track ID, skipping '{0}' ... {1} != {2}",
                                chunk_header.GetSigString(), chunk_header.id, track_id);

                        // Skip ahead to the next chunk
                        reader.BaseStream.Seek((long)chunk_header.data, SeekOrigin.Current);
                        continue;
                    }

                    if (debugMode)
                    {
                        Debug.LogFormat("PM> chunk signature / type = '{0}' / {1}",
                            chunk_header.GetSigString(), (PMTags)chunk_header.type);
                    }

                    // Read the audio data as bytes
                    byte[] data_bytes = new byte[chunk_header.data];
                    reader.Read(data_bytes, 0, (int)chunk_header.data);

                    // Save to temp folder
                    string layer_type = String.Format("{0}", (PMAudioLayerTypes)chunk_header.type);
                    string tempPath = GetAbsoluteFilePath(String.Format(
                        $"{cachePath}/{cachePathTracks}/" +
                        $"{track_id}/" +
                        $"{track_id}_{layer_type}" +
                        $"{PMExtensions.OGG}"
                    ), false);

                    SaveBinToCache(data_bytes, tempPath);
                }
            } catch (Exception e)
            {
                Debug.LogErrorFormat("PM> ERROR:Core.ExtractLayersFromAAI(): {0}", e.ToString());
            }
        }

        //----------------------------------------------------------
        private string GetAAIFormatExtension(ushort format)
        {
            switch (format)
            {
                case PMAudioFormat.raw_float:
                    return PMExtensions.RAW;
                case PMAudioFormat.pcm_16:
                    return PMExtensions.PCM;
                case PMAudioFormat.ogg:
                    return PMExtensions.OGG;
                case PMAudioFormat.wav:
                    return PMExtensions.WAV;
                case PMAudioFormat.mp4:
                    return PMExtensions.MP4;
            }

            return "";
        }


        //----------------------------------------------------------
        /**
        * @brief Create a mono audio buffer from a multi-channel source buffer
        * 
        * @param source - The source audio buffer
        * @param samples - The source audio number of samples
        * @param channels - The source audio number of channels
        * @return float[] - Float array of the new mono audio
        * 
        * @remarks If the source audio is already mono, this function will return a copy of the source array
        */
        private float[] ConvertToMono(float[] source, int samples, int channels)
        {
            float[] mono_buffer = null;
            if (null == source) { return null; }

            try
            {
                mono_buffer = new float[samples];
                float channels_flt = (float)channels;
                int src_index = 0;
                int dst_index = 0;

                for (int s = 0; s<samples; s++)
                {
                    for (int c = 0; c<channels; c++)
                    {
                        mono_buffer[dst_index] += source[src_index++] / channels_flt;
                    }
                    dst_index++;
                }

            } catch (Exception ex)
            {
                Debug.LogError("PM> ERROR:Core.ConvertToMono(): " + ex.ToString());
                mono_buffer = null;
            }

            return mono_buffer;
        }

        //----------------------------------------------------------
        /**
        * @brief Apply a filter to the supplied audio buffer
        * 
        * @param source - The source audio buffer
        * @param samples - The source audio number of samples
        * @param channels - The source audio number of channels
        */
        private void ApplyFilter(float[] source, int samples, int channels)
        {
            if (debugMode)
                Debug.Log("PM> Core.ApplyFilter()");

            if (null == filterBuffer)
            {
                Debug.LogError("PM> ERROR:Core.ApplyFilter(): filterBuffer empty!");
                return;
            }
            if (null == source || samples < 1)
            {
                Debug.LogError("PM> ERROR:Core.ApplyFilter(): Invalid input!");
                return;
            }

            try
            {

                int src_index = 0;
                int filter_index = 0;
                float filter_value = 0.0f;
                bool filter_step = true;
                const float clip_threshold = 0.99f;

                for (int s = 0; s<samples; s++)
                {
                    if (filter_step)
                        filter_value = filterBuffer[filter_index++] * filterVolume;
                    else
                    {
                        filter_value = 0.0f;
                        filter_index++;
                    }
                    if (filter_index >= filterLength)
                    {
                        filter_index = 0;
                        filter_step = !filter_step;
                    }

                    for (int c = 0; c<channels; c++)
                    {
                        source[src_index] += filter_value;

                        // Clamp to avoid clipping
                        if (source[src_index] > clip_threshold)
                            source[src_index] = clip_threshold;
                        else
                            if (source[src_index] < -clip_threshold)
                            source[src_index] = -clip_threshold;

                        src_index++;
                    }
                }

            } catch (Exception ex)
            {
                Debug.LogError("PM> ERROR:Core.ApplyFilter(): " + ex.ToString());
            }
        }


        #endregion
        //----------------------------------------------------------
        // Disk Cache functions
        //----------------------------------------------------------
        #region disk_cache_functions


        //----------------------------------------------------------
        private void InitCache()
        {
            string prevCachePath = "";
            bool deleteMeta = false;

            // Set current cache path
            // $$$ Disabled for now until we can do this properly
            /*
            if (pmso.bundleCacheWithGame)
            {
                cachePath = cachePathVisible;
                prevCachePath = cachePathHidden;
            }
            else
            */
            {
                cachePath = cachePathHidden;
                prevCachePath = cachePathVisible;
                deleteMeta = true;
            }

            string srcPath = GetAbsoluteFilePath(prevCachePath, false);
            string dstPath = GetAbsoluteFilePath(cachePath, false);

            try
            {
                // Check if the destination path exists
                if (!Directory.Exists(dstPath))
                {
                    // Do we have the source folder?
                    if (!Directory.Exists(srcPath))
                    {
                        // Neither exists, create the destination path
                        Debug.LogFormat("PM> Core.InitCache(): Cache Folder does not exist! Creating '{0}'", dstPath);
                        Directory.CreateDirectory(dstPath);
                    }
                    else
                    {
                        // Rename the cache folder
                        Debug.LogFormat("PM> Core.InitCache(): {0} -> {1}", srcPath, dstPath);
                        Directory.Move(srcPath, dstPath);
#if UNITY_EDITOR
                        if (!deleteMeta)
                            if (Application.isEditor)
                                AssetDatabase.Refresh();
#endif
                    }

                    if (deleteMeta)
                    {
                        string metaFile = srcPath + ".meta";

                        Debug.LogFormat("PM> Core.InitCache(): Deleting old .meta '{0}'", metaFile);

                        if (File.Exists(metaFile))
                        {
                            File.Delete(metaFile);
#if UNITY_EDITOR
                            if (Application.isEditor)
                                AssetDatabase.Refresh();
#endif
                        }
                        else
                            Debug.Log("PM> Core.InitCache(): File does not exist!");
                    }
                }

                if (debugMode)
                    Debug.LogFormat("PM> Core.InitCache(): Cache path is '{0}'", dstPath);

            } catch (Exception e)
            {
                Debug.LogError("PM> ERROR:Core.InitCache()");
                Debug.LogException(e, this);
            }
        }

        //----------------------------------------------------------
        /**
        * @brief Load the json data from a cache file into a String
        * @param filepath
        */
        //----------------------------------------------------------
        private string LoadJsonFromCache(string filepath)
        {
            if (debugMode)
                Debug.LogFormat("PM> Core.LoadJsonFromCache({0})", filepath);

            string content = "";

            try
            {
                // Make sure the target folder exists
                string targetFolder = Path.GetDirectoryName(filepath);
                if (Directory.Exists(targetFolder))
                {
                    StreamReader reader = new StreamReader(filepath);
                    content = reader.ReadToEnd();
                    reader.Close();
                }
                else
                {
                    Debug.LogWarningFormat("PM> Core.LoadJsonFromCache(): Target Folder does not exist! '{0}'", targetFolder);
                }
            } catch (Exception e)
            {
                Debug.LogError("PM> ERROR:Core.LoadJsonFromCache()");
                Debug.LogException(e, this);
            }

            return content;
        }

        //----------------------------------------------------------
        /**
        * @brief Save the json String to a chache file
        * @param source
        * @param filepath
        */
        //----------------------------------------------------------
        private void SaveJsonToCache(string source, string filepath)
        {
            if (debugMode)
                Debug.LogFormat("PM> Core.SaveJsonToCache({0})", filepath);

            try
            {
                // Make sure the target folder exists, if not, we create it
                string targetFolder = Path.GetDirectoryName(filepath);
                if (!string.IsNullOrWhiteSpace(targetFolder) && !Directory.Exists(targetFolder))
                {
                    if (debugMode)
                        Debug.Log($"PM> Core.SaveJsonToCache(): Target Folder does not exist! Creating '{targetFolder}'");
                    Directory.CreateDirectory(targetFolder);
                }

                StreamWriter writer = new StreamWriter(filepath, false);
                writer.Write(source);
                writer.Close();

            } catch (Exception e)
            {
                Debug.LogError("PM> ERROR:Core.SaveJsonToCache()");
                Debug.LogException(e, this);
            }
        }

        //----------------------------------------------------------
        private void SaveBinToCache(byte[] source, string filepath)
        {
            if (debugMode)
                Debug.LogFormat("PM> Core.SaveBinToCache({0})", filepath);

            try
            {
                // Make sure the target folder exists, if not, we create it
                string targetFolder = Path.GetDirectoryName(filepath);
                if (!string.IsNullOrWhiteSpace(targetFolder) && !Directory.Exists(targetFolder))
                {
                    if (debugMode)
                        Debug.Log($"PM> Core.SaveBinToCache(): Target Folder does not exist! Creating '{targetFolder}'");
                    Directory.CreateDirectory(targetFolder);
                }

                FileStream file = File.Create(filepath);
                file.Write(source, 0, source.Length);
                file.Close();

            } catch (Exception e)
            {
                Debug.LogError("PM> ERROR:Core.SaveBinToCache()");
                Debug.LogException(e, this);
            }
        }

        //----------------------------------------------------------
        private void SaveTextureToCache(Texture2D source, string filepath)
        {
            if (debugMode)
                Debug.LogFormat("PM> Core.SaveTextureToCache({0})", filepath);

            try
            {
                // Make sure the target folder exists, if not, we create it
                string targetFolder = Path.GetDirectoryName(filepath);
                if (!string.IsNullOrWhiteSpace(targetFolder) && !Directory.Exists(targetFolder))
                {
                    if (debugMode)
                        Debug.Log($"PM> Core.SaveTextureToCache(): Target Folder does not exist! Creating '{targetFolder}'");
                    Directory.CreateDirectory(targetFolder);
                }

                File.WriteAllBytes(filepath, source.EncodeToPNG());

            } catch (Exception e)
            {
                Debug.LogError("PM> ERROR:Core.SaveTextureToCache()");
                Debug.LogException(e, this);
            }
        }

        //----------------------------------------------------------
        /**
        * @brief Save the sample data from an AudioClip to a cache file
        * @param clip_samples - The float[] buffer holding the audio data
        * @param samples - Number of samples
        * @param channels - Number of channels
        * @param frequency - Source audio freqeuncy
        * @param filepath - The path/name of the clip
        * 
        * NOTE: We currently only support WAV files
        */
        //----------------------------------------------------------
        private void SaveClipToCache(float[] clip_samples, int samples, int channels, int frequency, string filepath)
        {
            if (debugMode)
                Debug.LogFormat("PM> Core.SaveClipToCache({0})", filepath);

            try
            {
                FileInfo fi = new FileInfo(filepath);
                if (fi.Extension.ToLower() != ".wav")
                {
                    Debug.LogError("PM> ERROR:Core.SaveClipToCache(): File Extension is not .wav!");
                    return;
                }

                // Make sure the target folder exists, if not, we create it
                string targetFolder = Path.GetDirectoryName(filepath);
                if (!string.IsNullOrWhiteSpace(targetFolder) && !Directory.Exists(targetFolder))
                {
                    if (debugMode)
                        Debug.Log($"PM> Core.SaveClipToCache(): Target Folder does not exist! Creating '{targetFolder}'");
                    Directory.CreateDirectory(targetFolder);
                }

                int num_floats = samples * channels;
                float length = (float)samples / (float)frequency;

                if (debugMode)
                    Debug.LogFormat("PM> channels, frequency, length, samples = {0}, {1}, {2}, {3}",
                        channels, frequency, length, samples);

                int dataLength = num_floats * sizeof(UInt16);   // PCM = 2 bytes
                byte[] buffer = new byte[dataLength];
                int fileSize = 44 + dataLength - 8;             // Header + data - 8
                int blockSize = channels * sizeof(UInt16);
                int bytesPerSecond = frequency * blockSize;

                float rescaleFactor = 32767.0f; // To convert float to Int16
                int p = 0;
                for (int i = 0; i<num_floats; i++)
                {
                    Int16 value = (Int16)(clip_samples[i] * rescaleFactor);
                    buffer[p++] = (byte)(value >> 0);
                    buffer[p++] = (byte)(value >> 8);
                }

                // Write out the WAV file
                FileStream stream = new FileStream(filepath, FileMode.Create);
                BinaryWriter writer = new BinaryWriter(stream);

                // Write header
                PMFileHeaders.WAV header = new PMFileHeaders.WAV();
                header.size      = fileSize;
                header.channels  = (Int16)channels;
                header.frequency = frequency;
                header.bytesPerSecond = bytesPerSecond;
                header.blockSize  = (Int16)blockSize;
                header.dataLength = dataLength;
                foreach (byte[] field in header)
                    writer.Write(field);

                // Write byte[] data
                writer.Write(buffer, 0, header.dataLength);
                writer.Close();

            } catch (Exception e)
            {
                Debug.LogError("PM> ERROR:Core.SaveClipToCache()");
                Debug.LogException(e, this);
            }
        }

        //----------------------------------------------------------
        /**
        * @brief Save the sample data from an AudioClip to a cache file
        * @param clip - The AudioClip object holding the audio data
        * @param filepath - The path/name of the clip
        * 
        * NOTE: We currently only support WAV files
        */
        //----------------------------------------------------------
        private void SaveClipToCache(AudioClip clip, string filepath)
        {

            try
            {
                // Get the float samples from the AudioClip
                int num_floats = clip.samples * clip.channels;
                float[] float_samples = new float[num_floats];
                clip.GetData(float_samples, 0);

                SaveClipToCache(float_samples, clip.samples, clip.channels, clip.frequency, filepath);

            } catch (Exception e)
            {
                Debug.LogError("PM> ERROR:Core.SaveClipToCache()");
                Debug.LogException(e, this);
            }
        }


        #endregion
        //----------------------------------------------------------
        // Public Plugin API functions
        //----------------------------------------------------------
        #region public_plugin_api_functions


        //----------------------------------------------------------
        /**
        * @brief Return a PMTrackInfo object for the supplied track index
        * @param index
        * @return - Returns the PMTrackInfo object or null if not found
        */
        public PMTrackInfo GetTrackInfoByIndex(int index)
        {
            if (null != currentProject.tracks && index > -1 && currentProject.tracks.Count > index)
            {
                return new PMTrackInfo
                {
                    id = currentProject.tracks[index].id,
                    index = index,
                    name = currentProject.tracks[index].name,
                    artist = currentProject.tracks[index].artist,
                    hasLayers = currentProject.tracks[index].canUseLayers,
                    isSelected = (currentProject.tracks[index].id == currentProject.trackId),
                    image = currentProject.tracks[index].AlbumCover,
                    arrangements = currentProject.tracks[index].arrangements
                };
            }

            return null;
        }

        //----------------------------------------------------------
        /**
        * @brief Return track id for the supplied track index
        * @param index
        * @return - Returns the track id or 0 if not found
        */
        public Int64 GetTrackIdByIndex(int index)
        {
            if (null != currentProject.tracks && index > -1 && currentProject.tracks.Count > index)
            {
                return currentProject.tracks[index].id;
            }

            return 0;
        }

        //----------------------------------------------------------
        /**
        * @brief Return the array index for the supplied track id
        * @param trackId
        * @return - Returns the index or -1 if not found
        */
        public int GetTrackIndexFromId(Int64 trackId)
        {
            int retval = -1;

            try
            {
                for (int s = 0; s<currentProject.tracks.Count; s++)
                {
                    if (trackId == currentProject.tracks[s].id)
                    {
                        retval = s;
                        break;
                    }
                }
            } catch (Exception e)
            {
                Debug.LogErrorFormat("PM> ERROR:GetTrackIndexFromId(): {0}", e.ToString());
            }

            return retval;
        }

        //----------------------------------------------------------
        /**
        * @brief Return the array index for the supplied arrangement id
        * @param trackId
        * @param arrangementId
        * @return - Returns the index or -1 if not found
        */
        public int GetArrangementIndexFromId(Int64 trackId, Int64 arrangementId)
        {
            int retval = -1;

            try
            {
                for (int s = 0; s<currentProject.tracks.Count; s++)
                {
                    if (trackId == currentProject.tracks[s].id)
                    {
                        for (int a = 0; a<currentProject.tracks[s].arrangements.Count; a++)
                        {
                            if (arrangementId == currentProject.tracks[s].arrangements[a].id)
                            {
                                retval = a;
                                goto l_end;
                            }
                        }
                    }
                }

            } catch (Exception e)
            {
                Debug.LogErrorFormat("PM> ERROR:GetArrangementIndexFromId(): {0}", e.ToString());
            }

l_end:
            return retval;
        }

        //----------------------------------------------------------
        /**
        * @brief Return the array index for the first arrangement type matching arrangementType
        * @param trackIdx
        * @param arrangementType
        * @return - Returns the index or -1 if not found
        */
        public int GetArrangementIndexFromType(int trackIdx, int arrangementType)
        {
            int retval = -1;

            try
            {
                for (int a = 0; a<currentProject.tracks[trackIdx].arrangements.Count; a++)
                {
                    if (arrangementType == currentProject.tracks[trackIdx].arrangements[a].type_id)
                    {
                        retval = a;
                        goto l_end;
                    }
                }
            } catch (Exception e)
            {
                Debug.LogErrorFormat("PM> ERROR:Core.GetArrangementIndexFromType(): {0}", e.ToString());
            }
l_end:

            return retval;
        }

        //----------------------------------------------------------
        /**
        * @brief Return the first arrangement matching arrangementType
        * @param trackIdx
        * @param arrangementType
        * @return - Returns the PMArrangement or null if not found
        */
        public PMArrangement GetArrangementFromType(int trackIdx, int arrangementType)
        {
            PMArrangement retval = null;

            try
            {
                for (int a = 0; a<currentProject.tracks[trackIdx].arrangements.Count; a++)
                {
                    if (arrangementType == currentProject.tracks[trackIdx].arrangements[a].type_id)
                    {
                        retval = currentProject.tracks[trackIdx].arrangements[a];
                        goto l_end;
                    }
                }
            } catch (Exception e)
            {
                Debug.LogErrorFormat("PM> ERROR:Core.GetArrangementFromType(): {0}", e.ToString());
            }
l_end:
            return retval;
        }

        //----------------------------------------------------------
        /**
        * @brief Play a SoundFX (Stinger) from the Resources/SoundFX folder
        * @param soundFxId
        */
        //----------------------------------------------------------
        public void PlaySoundFX(PMSoundFX soundFxId)
        {
            if (PMSoundFX.None != soundFxId)
            {
                string fxname = soundFxId.ToString();
                if (soundFxClips.ContainsKey(fxname))
                    audioSourceSoundFx.PlayOneShot(soundFxClips[fxname]);
            }
        }

        //----------------------------------------------------------
        /**
        * @brief Return the time of the next beat of the current arrangement
        * @return - Returns the next beat time
        */
        //----------------------------------------------------------
        public float TimeNextBeat()
        {
            float[] beatTimesToUse = null;
            float nextBeat = -1.0f;

            try
            {
                beatTimesToUse = currentProject.tracks[currentProject.trackIndex].arrangements[currentProject.arrangementIndex].beats;
                nextBeat = GetNextClosestTime(beatTimesToUse);

            } catch (Exception e)
            {
                Debug.LogErrorFormat("PM> ERROR:Core.TimeNextBeat(): {0}", e.ToString());
                beatTimesToUse = null;
                nextBeat = -1.0f;
            }

            if (null == beatTimesToUse)
            {
                string errMsg = "PM> Core.TimeNextBeat(): beatTimesToUse is null! " +
                    "Your Track is either missing arrangements or hasn't been fully loaded yet. " +
                    "LoadTrack() is asynchronous, use the 'OnLoadingProgress' message callback to determine if a Track has been fully loaded. " +
                    "See 'PlusMusicSettingsPanel.cs' for an example.";
                Debug.LogError(errMsg);
            }

            return nextBeat;
        }

        //----------------------------------------------------------
        /**
        * @brief Return the time of the next bar of the current arrangement
        * @return - Returns the next bar time
        */
        //----------------------------------------------------------
        public float TimeNextBar()
        {
            float[] barTimesToUse = null;
            float nextBar = -1.0f;

            try
            {
                barTimesToUse = currentProject.tracks[currentProject.trackIndex].arrangements[currentProject.arrangementIndex].bars;
                nextBar = GetNextClosestTime(barTimesToUse);

            } catch (Exception e)
            {
                Debug.LogErrorFormat("PM> ERROR:Core.TimeNextBar(): {0}", e.ToString());
                barTimesToUse = null;
                nextBar = -1.0f;
            }

            if (null == barTimesToUse)
            {
                string errMsg = "PM> Core.TimeNextBar(): barTimesToUse is null! " +
                    "Your Track is either missing arrangements or hasn't been fully loaded yet. " +
                    "LoadTrack() is asynchronous, use the 'OnLoadingProgress' message callback to determine if a Track has been fully loaded. " +
                    "See 'PlusMusicSettingsPanel.cs' for an example.";
                Debug.LogError(errMsg);
            }

            return nextBar;
        }

        //----------------------------------------------------------
        /**
        * @brief Make a deep copy of a transition object
        * @param sourceTransition
        * @return - Returns the new PMTransitionInfo object
        */
        public PMTransitionInfo CopyTransition(PMTransitionInfo sourceTransition)
        {
            return new PMTransitionInfo(
                sourceTransition.tag, sourceTransition.duration, sourceTransition.timing,
                sourceTransition.useMainVolume, sourceTransition.mainVolume,
                sourceTransition.useLayerVolumes, sourceTransition.layerVolumes,
                sourceTransition.soundFX,
                sourceTransition.canTransitionToItself, sourceTransition.returnToPrevious,
                sourceTransition.timeToLive, sourceTransition.curve
            );
        }

        //----------------------------------------------------------
        /**
        * @brief Load an environment variable into a String
        * @param var_name
        * @param def_val
        * @return - Returns the env variable String or def_val("") if not found
        */
        public string GetEnvVariable(string var_name, string def_val)
        {
            string envVariable = Environment.GetEnvironmentVariable(var_name);

            if (!string.IsNullOrWhiteSpace(envVariable))
                return envVariable.Trim();

            return def_val;
        }

        //----------------------------------------------------------
        /**
        * @brief Return a reference to the current AudioSource
        * @return - Returns the current AudioSource
        */
        public AudioSource GetCurrentAudioSource()
        {
            if (currentProject.tracks[currentProject.trackIndex].canUseLayers)
                return currentLayers[1];
            else
                return currentLayers[0];
        }

        //----------------------------------------------------------
        /**
        * @brief Start audio playback
        */
        public void StartPlay()
        {
            if (currentProject.tracks[currentProject.trackIndex].canUseLayers)
            {
                for (int l = 1; l<audioGroupLayers.Length; l++)
                    currentLayers[l].Play();
                isAudioPlaying = currentLayers[1].isPlaying;
            }
            else
            {
                currentAudioSource.Play();
                isAudioPlaying = currentAudioSource.isPlaying;
            }

            audioState = PMAudioState.StatePlaying;
            OnAudioStateChanged?.Invoke(audioState);
        }

        //----------------------------------------------------------
        /**
        * @brief Stop audio playback
        */
        public void StopPlay()
        {
            if (currentProject.tracks[currentProject.trackIndex].canUseLayers)
            {
                for (int l=1; l<audioGroupLayers.Length; l++)
                    currentLayers[l].Stop();
                isAudioPlaying = currentLayers[1].isPlaying;
            }
            else
            {
                currentAudioSource.Stop();
                isAudioPlaying = currentAudioSource.isPlaying;
            }

            audioState = PMAudioState.StateStopped;
            OnAudioStateChanged?.Invoke(audioState);
        }

        //----------------------------------------------------------
        /**
        * @brief Pause audio playback
        */
        public void PausePlay()
        {
            if (currentProject.tracks[currentProject.trackIndex].canUseLayers)
            {
                for (int l = 1; l<audioGroupLayers.Length; l++)
                    currentLayers[l].Pause();
                isAudioPlaying = currentLayers[1].isPlaying;
            }
            else
            {
                currentAudioSource.Pause();
                isAudioPlaying = currentAudioSource.isPlaying;
            }

            audioState = PMAudioState.StatePaused;
            OnAudioStateChanged?.Invoke(audioState);
        }

        //----------------------------------------------------------
        /**
        * @brief Unpause audio playback
        */
        public void UnPausePlay()
        {
            if (currentProject.tracks[currentProject.trackIndex].canUseLayers)
            {
                for (int l = 1; l<audioGroupLayers.Length; l++)
                    currentLayers[l].UnPause();
                isAudioPlaying = currentLayers[1].isPlaying;
            }
            else
            {
                currentAudioSource.UnPause();
                isAudioPlaying = currentAudioSource.isPlaying;
            }

            audioState = PMAudioState.StateUnpaused;
            OnAudioStateChanged?.Invoke(audioState);
        }

        //----------------------------------------------------------
        /**
        * @brief Mute audio playback
        */
        public void MutePlay()
        {
            if (currentProject.tracks[currentProject.trackIndex].canUseLayers)
            {
                for (int l = 1; l<audioGroupLayers.Length; l++)
                    currentLayers[l].mute = true;
                isAudioPlaying = currentLayers[1].isPlaying;
            }
            else
            {
                currentAudioSource.mute = true;
                isAudioPlaying = currentAudioSource.isPlaying;
            }

            audioState = PMAudioState.StateMuted;
            OnAudioStateChanged?.Invoke(audioState);
        }

        //----------------------------------------------------------
        /**
        * @brief Unmute audio playback
        */
        public void UnMutePlay()
        {
            if (currentProject.tracks[currentProject.trackIndex].canUseLayers)
            {
                for (int l = 1; l<audioGroupLayers.Length; l++)
                    currentLayers[l].mute = false;
                isAudioPlaying = currentLayers[1].isPlaying;
            }
            else
            {
                currentAudioSource.mute = false;
                isAudioPlaying = currentAudioSource.isPlaying;
            }

            audioState = PMAudioState.StateUnmuted;
            OnAudioStateChanged?.Invoke(audioState);
        }

        //----------------------------------------------------------
        /**
        * @brief Return the volume of the current track playback
        * @return - Returns the volume (0.0 - 1.0)
        */
        public float GetVolume()
        {
            return pluginVolume;
        }

        //----------------------------------------------------------
        /**
        * @brief Set the volume for the track playback (0.0 - 1.0)
        * @param value
        */
        public void SetVolume(float volume)
        {
            // Clamp volume
            if (volume <= 0.0f)
                volume = 0.001f;
            if (volume > 1.0f)
                volume = 1.0f;

            pluginVolume = volume;

            if (null != currentAudioSource)
                currentAudioSource.volume = pluginVolume;

            if (null != audioSourceSoundFx)
                audioSourceSoundFx.volume = volume;

            currentProject.transition.mainVolume = volume;
        }

        //----------------------------------------------------------
        /**
        * @brief Sets the volume for individual Layers/Stems (0.0 - 1.0)
        * @param layer
        * @param volume
        */
        public void SetLayerVolume(PMAudioLayers layer, float volume)
        {
            if (layer == PMAudioLayers.LayerFullMix)
            { 
                SetVolume(volume);
                return;
            }

            // Clamp volume
            if (volume <= 0.0f)
                volume = 0.001f;
            if (volume > 1.0f)
                volume = 1.0f;

            int layer_index = (int)layer;
            layerVolumes[layer_index] = volume;

            if (audioSource1Playing)
                layerAudioSources1[layer_index].volume = volume;
            else
                layerAudioSources2[layer_index].volume = volume;

            // NOTE: We need to use -1 here since the two arrays are offset by 1
            // $$$: We should probably fix this!
            currentProject.transition.layerVolumes.SetByIndex(layer_index - 1, volume);

            OnLayerVolumeChanged?.Invoke(layerVolumes);
        }

        //----------------------------------------------------------
        /**
        * @brief Make the supplied project the current
        * @param projectId
        * @param authToken
        */
        public void SetCurrentProject(Int64 projectId, string authToken)
        {
            if (debugMode)
                Debug.Log($"PM> Core.SetCurrentProject(): projectId = {projectId}");

            // Set the project settings
            currentProject.id = projectId;
            currentProject.authToken = authToken;
            currentProject.trackIndex = pmso.autoTrackIndex;
            currentProject.transition = CopyTransition(pmso.defaultTransition);
            currentProject.audio = currentAudioSource;
            currentProject.trackProgress = new PMTrackProgress();
            currentProject.isLoaded = false;
            currentProject.isLoading = false;
        }

        //----------------------------------------------------------
        /**
        * @brief [asynchronous] Get the project details json from the PlusMusic Web Api, make it current and auto play if specified
        * @param projectId
        * @param authToken
        * @param autoPlay
        */
        public void LoadProject(Int64 projectId, string authToken, bool autoPlay)
        {
            if (debugMode)
                Debug.Log($"PM> Core.LoadProject({projectId}, {autoPlay})");

            SetCurrentProject(projectId, authToken);

            // Kick off async loading
            EventManager.StopAndReset();

            EventManager.AddEvent(new PMEventObject
            {
                type=PMEventTypes.EventGetProjectInfo,
                func=Instance.EventBroker,
                dependsOnPrevious=false,
                args=currentProject
            });

            if (autoPlay)
            {
                LoadTrack(new PMTrackProgress
                {
                    index = pmso.autoTrackIndex,
                    autoPlay = true
                });
            }

            if (!EventManager.isProcessing)
                EventManager.StartProcessing();
        }

        //----------------------------------------------------------
        /**
        * @brief [asynchronous] Load the project details json from the PlusMusic Web Api
        * @param project_info
        */
        public void LoadProjectInfo(PMProject project_info)
        {
            if (debugMode)
                Debug.Log($"PM> Core.LoadProjectInfo(): {project_info.id}");

            StartCoroutine(LoadProjectInfoAsync(project_info));
        }

        //----------------------------------------------------------
        /**
        * @brief [asynchronous] Load the project details json from the PlusMusic Web Api
        * @param projectId
        * @param projectToken
        * @return - Returns a PMProject object
        */
        public PMProject LoadProjectInfo(Int64 projectId, string projectToken)
        {
            if (debugMode)
                Debug.Log($"PM> Core.LoadProjectInfo(): {projectId}");

            PMProject project_info = new PMProject();
            project_info.id = projectId;
            project_info.authToken = projectToken;
            project_info.isLoading = true;

            StartCoroutine(LoadProjectInfoAsync(project_info));

            return project_info;
        }

        //----------------------------------------------------------
        /**
        * @brief Return the current project details as a PMMessageProjectInfo object
        * @return - Returns a PMMessageProjectInfo object
        */
        public PMMessageProjectInfo GetProjectInfo()
        {
            string func_name = "Core.GetProjectInfo()";
            if (debugMode)
                Debug.Log($"PM> {func_name}");

            PMMessageProjectInfo projectData = null;
            string errMsg = "";

            try
            {

                if (null == currentProject)
                {
                    errMsg = $"PM> ERROR:{func_name}: Project not loaded yet!";
                    goto l_end;
                }

                projectData = new PMMessageProjectInfo();
                projectData.id = currentProject.id;
                projectData.name = currentProject.name;

                if (null != currentProject.tracks)
                {
                    if (currentProject.tracks.Count > 0)
                    {
                        projectData.tracks = new PMMessageTrackList[currentProject.tracks.Count];
                        for (int s = 0; s<currentProject.tracks.Count; s++)
                        {
                            projectData.tracks[s] = new PMMessageTrackList()
                            {
                                id = currentProject.tracks[s].id,
                                name = currentProject.tracks[s].name,
                                hasFilter = currentProject.tracks[s].hasFilter,
                                isSelected = false,
                                canUseLayers = currentProject.tracks[s].canUseLayers,
                                isLoaded = currentProject.tracks[s].isLoaded
                            };

                            if (projectData.tracks[s].id == currentProject.trackId)
                                projectData.tracks[s].isSelected = true;
                        }
                    }
                }
                else
                    errMsg = $"PM> ERROR:{func_name}: tracks is null!";

            } catch (Exception e)
            {
                errMsg = $"PM> ERROR:{func_name}: " + e.ToString();
            }

l_end:
            if ("" != errMsg)
            {
                Debug.LogError(errMsg);
                OnRealTimeStatus?.Invoke(errMsg);
            }

            return projectData;
        }

        //----------------------------------------------------------
        /**
        * @brief [asynchronous] Load the track details json from the PlusMusic Web Api, make it current and auto play if specified
        * @param progress
        * @return - Returns true if the track loading was initiated, false otherwise
        */
        public bool LoadTrack(PMTrackProgress progress)
        {
            if (debugMode)
                Debug.Log("PM> Core.LoadTrack()");

            if (progress.isLoading)
            {
                Debug.LogWarning($"PM> Core.LoadTrack(): Track loading in progress, ignoring request ...");
                return false;
            }

            if (null != currentProject.tracks && currentProject.isLoaded)
            { 
                // Are we loading by id or index?
                if (progress.id > 0)
                    progress.index = GetTrackIndexFromId(progress.id);
                else
                    progress.id = GetTrackIdByIndex(progress.index);

                // Already loaded?
                if (currentProject.tracks[progress.index].isLoaded)
                {
                    Debug.LogWarning(
                        $"PM> Core.LoadTrack(): Track '{currentProject.tracks[progress.index].name}' already loaded, ignoring request ...");
                    return false;
                }
            }

            currentProject.trackProgress = progress;
            currentProject.trackProgress.isLoading = true;
            currentProject.trackProgress.progress = 0.0f;

            OnRealTimeStatus?.Invoke($"Loading track '{currentProject.tracks[progress.index].name}' ...");
            OnTrackLoadingProgress?.Invoke(currentProject.trackProgress);

            EventManager.AddEvent(new PMEventObject
            {
                type=PMEventTypes.EventGetTrackInfo,
                func=Instance.EventBroker,
                args=currentProject
            });

            EventManager.AddEvent(new PMEventObject
            {
                type=PMEventTypes.EventGetTrackImage,
                func=Instance.EventBroker,
                args=currentProject
            });

            EventManager.AddEvent(new PMEventObject
            {
                type=PMEventTypes.EventGetTrackAudio,
                func=Instance.EventBroker,
                args=currentProject
            });

            if (currentProject.trackProgress.autoPlay)
            { 
                EventManager.AddEvent(new PMEventObject
                {
                    type=PMEventTypes.EventPlayCurrent,
                    func=Instance.EventBroker,
                    args=currentProject
                });
            }

            if (!EventManager.isProcessing)
                EventManager.StartProcessing();

            return true;
        }

        //----------------------------------------------------------
        /**
        * @brief Play the supplied track
        * @param trackId
        */
        public void PlayTrack(Int64 trackId)
        {
            if (debugMode)
                Debug.LogFormat("PM> Core.PlayTrack({0})", trackId);

            int index = GetTrackIndexFromId(trackId);
            if (-1 != index)
            {
                // Make this the current track
                currentProject.trackId = currentProject.tracks[index].id;
                currentProject.trackIndex = index;
                currentProject.arrangementIndex = -1;

                // Do we already have the audio?
                if (currentProject.tracks[index].isLoaded)
                {
                    currentProject.arrangementIndex = GetArrangementIndexFromType(index, (int)currentProject.transition.tag);
                    currentProject.trackProgress.progress = 0.0f;
                    OnTrackLoadingProgress?.Invoke(currentProject.trackProgress);
                    PlayCurrent();
                    currentProject.trackProgress.progress = 1.0f;
                    OnTrackLoadingProgress?.Invoke(currentProject.trackProgress);
                }

                // Otherwise, queue loading the track audio
                else
                {
                    LoadTrack(new PMTrackProgress
                    {
                        index = index,
                        autoPlay = true
                    });
                }
            }
            else
            {
                Debug.LogError("PM> ERROR:Core.PlayTrack(): Track Id not found! " + trackId);
            }
        }

        //----------------------------------------------------------
        /**
        * @brief Play the currently selected arrangement
        */
        public void PlayCurrent()
        {
            if (debugMode)
                Debug.Log("PM> Core.PlayCurrent()");

            if (-1 == currentProject.arrangementIndex)
                currentProject.arrangementIndex = GetArrangementIndexFromType(currentProject.trackIndex, (int)currentProject.transition.tag);
            else
                currentProject.transition.tag = 
                    (PMTags)currentProject.tracks[currentProject.trackIndex].arrangements[currentProject.arrangementIndex].type_id;

            StartCoroutine(PlayArrangementTransition(currentProject.transition));
        }

        //----------------------------------------------------------
        /**
        * @brief Play the audio for the specified arrangement
        * @param trackId
        * @param arrangementId
        */
        public void PlayArrangement(Int64 trackId, Int64 arrangementId)
        {
            if (debugMode)
                Debug.LogFormat("PM> Core.PlayArrangement({0}, {1})", trackId, arrangementId);

            string errMsg = "";

            if (null == currentAudioSource)
            {
                errMsg = "PM> ERROR:Core.PlayArrangement(): currentAudioSource is null!";
                goto l_end;
            }

            if (trackId != currentProject.trackId)
            {
                int trackIdx = GetTrackIndexFromId(trackId);
                if (-1 == trackIdx)
                {
                    errMsg = "PM> ERROR:Core.PlayArrangement(): Track Id not found! " + trackId;
                    goto l_end;
                }
                currentProject.trackIndex = trackIdx;
            }

            if (arrangementId != currentProject.arrangementId)
            {
                int arrangementIdx = GetArrangementIndexFromId(trackId, arrangementId);
                if (-1 == arrangementIdx)
                {
                    errMsg = "PM> ERROR:Core.PlayArrangement(): Arrangement Id not found! " + arrangementId;
                    goto l_end;
                }
                currentProject.arrangementIndex = arrangementIdx;
            }

            // $$$ TODO 
            // If the current clip isn't loaded yet, we need to inject to loading events into the Event Manager
            // and also add another EventPlayAudio after that and not finish the rest of this function
            // List.Insert();
            // $$$

            PlayCurrent();

l_end:
            if ("" == errMsg)
            {
                EventManager.SetStatus(PMEventStatus.EventWasSuccessful);
            }
            else
            {
                EventManager.SetStatus(PMEventStatus.EventHasErrors);
                Debug.LogError(errMsg);
                OnRealTimeStatus?.Invoke(errMsg);
            }
        }

        //----------------------------------------------------------
        /**
        * @brief Play the arrangement specified in the PMTransitionInfo object
        * @param transition
        */
        public void PlayArrangement(PMTransitionInfo transition)
        {
            if (debugMode)
                Debug.LogFormat("PM> Core.PlayArrangement({0}, {1}, {2})",
                    transition.tag, transition.mainVolume, transition.useMainVolume);

            StartCoroutine(PlayArrangementTransition(transition));
        }

        //----------------------------------------------------------
        /**
        * @brief Play the arrangement specified by the arrangementType
        * @param arrangementType
        */
        public void PlayArrangement(PMTags arrangementType)
        {
            if (debugMode)
                Debug.LogFormat("PM> PlayArrangement(): arrangementType = {0}", arrangementType);

            // Find the first arrangement that matches our type and play it ...
            int idx = GetArrangementIndexFromType(currentProject.trackIndex, (int)arrangementType);
            if (idx < 0)
            {
                Debug.LogErrorFormat("PM> ERROR:PlayArrangement(): Arrangement index not found for '{0}'!", arrangementType);
                return;
            }

            currentProject.arrangementIndex = idx;
            PlayCurrent();

            return;
        }

        //----------------------------------------------------------
        /**
        * @brief [asynchronous] Load the track details json from the PlusMusic Web Api
        * @param project_info
        */
        public void LoadTrackInfo(PMProject project_info)
        {
            int index = project_info.trackProgress.index;
            if (debugMode)
                Debug.Log($"PM> Core.LoadTrackInfo(): {index}");

            if (index < project_info.numTracks)
            {
                currentProject.trackProgress.id = project_info.tracks[index].id;
                project_info.tracks[index].isLoaded = false;
                project_info.tracks[index].hasAudio = false;
                project_info.tracks[index].isLoading = true;
                StartCoroutine(LoadTrackInfoAsync(project_info.tracks[index]));
            }
            else
                Debug.LogError($"PM> ERROR:Core.LoadTrackInfo(): Index out of range! {index}");
        }

        //----------------------------------------------------------
        /**
        * @brief [asynchronous] Load the track details json from the PlusMusic Web Api
        * @param track_info
        */
        public void LoadTrackInfo(PMTrack track_info)
        {
            if (debugMode)
                Debug.Log($"PM> Core.LoadTrackInfo(): {track_info.id}");

            track_info.isLoaded = false;
            track_info.hasAudio = false;
            track_info.isLoading = true;
            StartCoroutine(LoadTrackInfoAsync(track_info));
        }

        //----------------------------------------------------------
        /**
        * @brief [asynchronous] Load the track audio from the PlusMusic Web Api
        * @param project_info
        */
        public void LoadTrackAudio(PMProject project_info)
        {
            int index = project_info.trackProgress.index;
            if (debugMode)
                Debug.Log($"PM> Core.LoadTrackAudio(): {index}");

            StartCoroutine(LoadTrackAudioAsync(project_info.tracks[index]));
        }

        //----------------------------------------------------------
        /**
        * @brief [asynchronous] Load the track audio from the PlusMusic Web Api
        * @param track_info
        */
        public void LoadTrackAudio(PMTrack track_info)
        {
            if (debugMode)
                Debug.Log($"PM> Core.LoadTrackAudio(): {track_info.id}");

            StartCoroutine(LoadTrackAudioAsync(track_info));
        }

        //----------------------------------------------------------
        /**
        * @brief [asynchronous] Load the track image from the PlusMusic Web Api
        * @param project_info
        */
        public void LoadTrackImage(PMProject project_info)
        {
            int index = project_info.trackProgress.index;
            if (debugMode)
                Debug.Log($"PM> Core.LoadTrackImage(): {index}");

            StartCoroutine(LoadTrackImageAsync(project_info.tracks[index]));
        }


        #endregion
        //----------------------------------------------------------
        // Async Web Api Calls
        //----------------------------------------------------------
        #region async_web_api_calls


        //----------------------------------------------------------
        private IEnumerator LoadLayersFromCacheAsync(PMTrack track)
        {
            if (debugMode)
                Debug.Log("PM> Core.LoadLayersFromCacheAsync()");

            // $$$ TODO - Allow for different types, like WAV and MP4
            AudioType type = AudioType.OGGVORBIS;

            PMAudioLayerTypes[] layerOrder = {
                PMAudioLayerTypes.original, 
                PMAudioLayerTypes.bass, PMAudioLayerTypes.drums, 
                PMAudioLayerTypes.topmix, PMAudioLayerTypes.vocals
            };

            // Skip FullMix here
            for (int s=1; s<track.layerAudioClips.Length; s++)
            {
                string layer_type = String.Format("{0}", layerOrder[s]);
                string loaclPath = GetAbsoluteFilePath(String.Format(
                    $"{cachePath}/{cachePathTracks}/" +
                    $"{track.track_id}/" +
                    $"{track.track_id}_{layer_type}" +
                    $"{PMExtensions.OGG}"
                ), false);
                string loaclPathFile = "file://" + loaclPath;

                yield return RunAsyncWithReturn<AudioClip>(
                    WebApiGetAudioClipAsync2(currentProject.authToken, loaclPathFile, type), (output) => track.layerAudioClips[s] = output);

                if (null != track.layerAudioClips[s])
                {
                    if (debugMode)
                        Debug.Log($"PM> {layer_type}.samples = {track.layerAudioClips[s].samples}");
                }
                else
                    Debug.LogError($"PM> {layer_type} = null!");

                // Cleanup
                if (!pmso.loadAudioFromCache && !pmso.saveAudioToCache)
                {
                    if (File.Exists(loaclPath))
                        File.Delete(loaclPath);
                }
            }

            yield return true;
        }

        //----------------------------------------------------------
        private IEnumerator ArrangementAudioFromLayerAsync(PMTrack track, int layer, int index, int num_floats, float[] full_samples)
        {
            string func_name = "Core.ArrangementAudioFromLayerAsync()";
            if (debugMode)
                Debug.Log($"PM> {func_name}: {track.id}, {(PMAudioLayers)layer}");

            if (null == track.arrangements[index].layers[layer])
            {
                if (debugMode)
                    Debug.LogFormat("PM> Found arrangement '{0}' with no audio",
                        track.arrangements[index].name);

                float length = track.arrangements[index].length;
                int channels = track.layerAudioClips[layer].channels;
                int frequency = track.layerAudioClips[layer].frequency;

                // NOTE: Due to possible rounding errors, we add a few extra samples here
                int num_clip_samples = (int)(length * (float)frequency) + 2;
                int num_clip_floats = num_clip_samples * channels;
                int num_clip_segments = track.arrangements[index].num_segment_clips;

                if (debugMode)
                    Debug.Log("PM> arrangement length, num_floats, num_clip_floats, num_clip_segments = " +
                        $"{length}, {num_floats}, {num_clip_floats}, {num_clip_segments}");

                track.arrangements[index].layers[layer] = AudioClip.Create(
                    track.arrangements[index].name,
                    num_clip_samples, channels, frequency, false);

                if (debugMode)
                    Debug.LogFormat("PM> clip: length, channels, frequency = {0}, {1}, {2}",
                        track.arrangements[index].layers[layer].length,
                        track.arrangements[index].layers[layer].channels, track.arrangements[index].layers[layer].frequency);

                float[] clip_samples = new float[num_clip_floats];
                int clip_offset = 0;
                int prev_segment_end_ofs = 0;

                // Loop over the segements and copy the audio data
                for (int sc = 0; sc<num_clip_segments; sc++)
                {
                    // Convert start/end time to index in the source audio
                    float start_time = track.arrangements[index].segment_clips[sc].start_time;
                    float end_time = track.arrangements[index].segment_clips[sc].end_time;
                    float seg_length = end_time - start_time;
                    int start_idx = (int)(
                        start_time * (float)frequency * (float)channels);
                    int end_idx = (int)(
                        end_time   * (float)frequency * (float)channels);

                    // Make sure the indices always align with the number of channels
                    start_idx = start_idx - (start_idx % channels);
                    end_idx = end_idx - (end_idx % channels);
                    int count = (end_idx - start_idx);

                    // Double check for array overflow
                    int end_offset = clip_offset + count;
                    if (end_offset > num_clip_floats)
                    {
                        int floats_to_trim = end_offset - num_clip_floats;
                        Debug.LogWarning($"PM> {func_name}: Buffer overflow! clip_samples is {floats_to_trim} floats too short");
                        count -= floats_to_trim;
                    }

                    Array.Copy(full_samples, start_idx, clip_samples, clip_offset, count);

                    if ((sc > 0) && (prev_segment_end_ofs != start_idx))
                    {
                        if (debugMode)
                            Debug.Log("PM> Disjointed segment!");

                        CrossFadeSegments(
                            full_samples,           // Source array that holds all segments
                            clip_samples,           // Arrangment array that holds both segments that need to be crossfaded
                            clip_offset,            // Arrangment array offset where both segments touch
                            prev_segment_end_ofs,   // Offset into the source of the end of the first segment
                            start_idx,              // Offset into the source of the start of the second segment
                            crossfadeOverlap, frequency, channels);
                    }

                    clip_offset += count;
                    prev_segment_end_ofs = end_idx;
                }

                // Apply filter if needed
                if (track.hasFilter)
                    ApplyFilter(clip_samples, num_clip_samples, channels);

                track.arrangements[index].layers[layer].SetData(clip_samples, 0);
            }

            if (pmso.saveAudioToCache)
            {
                string clipName = GetAbsoluteFilePath(String.Format(
                    $"{cachePath}/{cachePathProjects}/" +
                    $"{currentProject.id}/" +
                    $"{track.id}_" +
                    $"{(PMAudioLayers)layer}_" +
                    $"{track.arrangements[index].name}" +
                    $"{PMExtensions.WAV}"
                ), false);

                SaveClipToCache(track.arrangements[index].layers[layer], clipName);
            }

            yield return true;
        }

        //----------------------------------------------------------
        private AudioClip WebApiGetAudioClip(string finalUrl, AudioType type)
        {
            if (debugMode)
                Debug.Log("PM> Core.WebApiGetAudioClip()");
            if (pmso.logRequestUrls)
                Debug.LogFormat("PM> finalUrl = {0}", finalUrl);

            AudioClip response = null;

            try
            {
                using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(finalUrl, type))
                {
                    float time_start = Time.realtimeSinceStartup;
                    webRequest.SendWebRequest();
                    while (UnityWebRequest.Result.InProgress == webRequest.result)
                        Thread.Sleep(3);
                    float time_elapsed = Time.realtimeSinceStartup - time_start;

                    if (debugMode)
                        Debug.LogFormat(
                            "PM> Core.WebApiGetAudioClip(): Downloaded {0} bytes in {1} seconds",
                            webRequest.downloadedBytes, time_elapsed.ToString("f6"));

                    if (UnityWebRequest.Result.Success != webRequest.result)
                        Debug.LogError(
                            "PM> ERROR:Core.WebApiGetAudioClip(): Api call failed! " + webRequest.error);
                    else
                    {
                        response = DownloadHandlerAudioClip.GetContent(webRequest);
                        if (null == response)
                            Debug.LogError("PM> ERROR:Core.WebApiGetAudioClip(): Failed to get Api content!");
                    }
                }

            } catch (Exception e)
            {
                Debug.LogErrorFormat("PM> ERROR:Core.WebApiGetAudioClip(): {0}", e.ToString());
            }

            return response;
        }

        //----------------------------------------------------------
        private static IEnumerator RunAsyncWithReturn<T>(IEnumerator target, Action<T> output)
        {
            object result = null;

            while (target.MoveNext())
            {
                result = target.Current;
                yield return result;
            }

            IDisposable disp = target as IDisposable;
            if (null != disp)
                disp.Dispose();

            output((T)result);
        }

        //----------------------------------------------------------
        private IEnumerator LoadArrangementAudioLayerAsync(PMTrack track, int layer)
        {
            string func_name = "Core.LoadArrangementAudioLayerAsync()";
            if (debugMode)
                Debug.Log($"PM> {func_name}: {currentProject.trackIndex}");

            PMAudioLayers layer_type = (PMAudioLayers)layer;

            // $$$ TODO - Allow for different types
            AudioType type = AudioType.WAV;

            for (int a = 0; a<track.arrangements.Count; a++)
            {
                string loaclPath = GetAbsoluteFilePath(String.Format(
                    $"{cachePath}/{cachePathProjects}/" +
                    $"{currentProject.id}/" +
                    $"{track.id}_" +
                    $"{(PMAudioLayers)layer}_" +
                    $"{track.arrangements[a].name}" +
                    $"{PMExtensions.WAV}"
                ), false);
                string loaclPathFile = "file://" + loaclPath;

                yield return RunAsyncWithReturn<AudioClip>(
                    WebApiGetAudioClipAsync2(currentProject.authToken, loaclPathFile, type), (output) => track.arrangements[a].layers[layer] = output);

                if (null != track.arrangements[a].layers[layer])
                {
                    if (debugMode)
                        Debug.Log($"PM> {layer_type}.samples = {track.arrangements[a].layers[layer].samples}");
                }
                else
                    Debug.LogError($"PM> {layer_type} = null!");
            }

            yield return true;
        }

        //----------------------------------------------------------
        private IEnumerator CreateArrangementAudioFromLayerAsync(PMTrack track, int layer)
        {
            string func_name = "Core.CreateArrangementAudioFromLayerAsync()";
            if (debugMode)
                Debug.Log($"PM> {func_name}: {currentProject.trackIndex}");

            string errMsg = "";

            int num_floats = track.layerAudioClips[layer].samples * track.layerAudioClips[layer].channels;
            float[] full_samples = new float[num_floats];
            track.layerAudioClips[layer].GetData(full_samples, 0);

            for (int a = 0; a<track.arrangements.Count; a++)
            {
                bool did_load = false;
                yield return RunAsyncWithReturn<bool>
                    (ArrangementAudioFromLayerAsync(track, layer, a, num_floats, full_samples), (output) => did_load = output);
            }

            track.isLoaded = true;

            if ("" == errMsg)
            {
                EventManager.SetStatus(PMEventStatus.EventWasSuccessful);
            }
            else
            {
                EventManager.SetStatus(PMEventStatus.EventHasErrors);
                Debug.LogError(errMsg);
                OnRealTimeStatus?.Invoke(errMsg);
            }

            yield return true;
        }

        //----------------------------------------------------------
        private IEnumerator CreateFullMixAsync(Int64 project_id, PMTrack track)
        {
            if (debugMode)
                Debug.Log("PM> Core.CreateFullMixAsync()");

            try
            {
                PMStemBuffer[] stems = new PMStemBuffer[4];
                const float clip_threshold = 0.99f;
                int samples = track.layerAudioClips[LayerTopMix].samples;
                int channels = track.layerAudioClips[LayerTopMix].channels;
                int num_floats = samples * channels;
                float[] full_floats = new float[num_floats];

                for (int b = 0; b<4; b++)
                {
                    stems[b].buffer = new float[num_floats];
                    track.layerAudioClips[b+1].GetData(stems[b].buffer, 0);
                }

                for (int f = 0; f<num_floats; f++)
                {
                    full_floats[f] =
                        (stems[2].buffer[f] + stems[0].buffer[f] + stems[1].buffer[f] + stems[3].buffer[f]);

                    // Clamp to avoid clipping
                    if (full_floats[f] > clip_threshold)
                        full_floats[f] = clip_threshold;
                    else
                        if (full_floats[f] < -clip_threshold)
                        full_floats[f] = -clip_threshold;
                }

                if (track.hasFilter)
                    ApplyFilter(full_floats, samples, channels);

                track.layerAudioClips[LayerFullMix] = AudioClip.Create("FullMix",
                    track.layerAudioClips[LayerTopMix].samples,
                    track.layerAudioClips[LayerTopMix].channels,
                    track.layerAudioClips[LayerTopMix].frequency, false);
                track.layerAudioClips[LayerFullMix].SetData(full_floats, 0);

                if (pmso.saveAudioToCache)
                {
                    string clipName = GetAbsoluteFilePath(String.Format(
                        $"{cachePath}/{cachePathTracks}/" +
                        $"{track.track_id}/" +
                        $"{track.track_id}_FullMix" +
                        $"{PMExtensions.WAV}"
                    ), false);

                    SaveClipToCache(track.layerAudioClips[LayerFullMix], clipName);
                }

            } catch (Exception e)
            {
                Debug.LogError("PM> ERROR:Core.CreateFullMixAsync(): " + e.ToString());
            }

            yield return true;
        }

        //----------------------------------------------------------
        private IEnumerator ParseDefaultProjectInfoAsync(PMProject project, string jsonResponse)
        {
            string func_name = "Core.ParseDefaultProjectInfoAsync()";
            if (debugMode)
                Debug.Log($"PM> {func_name}");

            bool didParse = false;
            string errMsg = "";

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                if (pmso.saveMetaToCache)
                {
                    string jsonName = GetAbsoluteFilePath(String.Format(
                        $"{cachePath}/{cachePathProjects}/" +
                        $"default_project" +
                        $"{PMExtensions.JSON}"
                    ), false);

                    if (!File.Exists(jsonName))
                        SaveJsonToCache(jsonResponse, jsonName);
                }

                PMDefaultProjectInfo defaultProject = JsonUtility.FromJson<PMDefaultProjectInfo>(jsonResponse);
                if (null != defaultProject)
                {
                    PMDefaultProjectData projectData = defaultProject.default_project;
                    if (null != defaultProject)
                    {
                        project.id = projectData.id;
                        project.authToken = projectData.plugin_api_key;
                        if (debugMode)
                        {
                            Debug.Log($"PM> {func_name}: project.id = {project.id}");
                            Debug.Log($"PM> {func_name}: project.authToken = {project.authToken}");
                        }
                        didParse = true;
                    }
                    else
                        errMsg = $"PM> ERROR:{func_name} failed: No project data in response!";
                }
                else
                    errMsg = $"PM> ERROR:{func_name} failed: No project info in response!";
            }
            else
                errMsg = $"PM> ERROR:{func_name}: jsonResponse is null!";

            if ("" != errMsg)
            {
                Debug.LogError(errMsg);
                OnRealTimeStatus?.Invoke(errMsg);
            }

            yield return didParse;
        }

        //----------------------------------------------------------
        private IEnumerator ParseProjectInfoAsync(PMProject project, string jsonResponse)
        {
            string func_name = "Core.ParseProjectInfoAsync()";
            if (debugMode)
                Debug.Log($"PM> {func_name}");

            bool didParse = false;
            string errMsg = "";

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                if (pmso.saveMetaToCache)
                {
                    string jsonName = GetAbsoluteFilePath(String.Format(
                        $"{cachePath}/{cachePathProjects}/" +
                        $"{project.id}/" +
                        $"{project.id}_project" +
                        $"{PMExtensions.JSON}"
                    ), false);

                    if (!File.Exists(jsonName))
                        SaveJsonToCache(jsonResponse, jsonName);
                }

                ParentProjectDataV1 tempInfo = JsonUtility.FromJson<ParentProjectDataV1>(jsonResponse);
                if (null != tempInfo)
                {
                    project.name = tempInfo.name;
                    project.isLoaded = false;
                    project.numTracks = 0;
                    project.tracks = new List<PMTrack>();

                    if (debugMode)
                        Debug.Log($"PM> {func_name}: project id, name = {project.id}, {project.name}");

                    if (null != tempInfo.soundtracks)
                    {
                        project.numTracks = tempInfo.soundtracks.Length;

                        for (int s = 0; s<project.numTracks; s++)
                        {
                            project.tracks.Add(
                                new PMTrack
                                {
                                    id         = tempInfo.soundtracks[s].id,
                                    track_id   = tempInfo.soundtracks[s].song_id,
                                    name       = tempInfo.soundtracks[s].name,
                                    updated_at = tempInfo.soundtracks[s].updated_at,
                                    length     = 0.0f, canUseLayers = true,
                                    hasFilter  = !tempInfo.soundtracks[s].is_licensed,
                                    isLoaded   = false, num_arrangements = 0, arrangements = null,
                                    layerAudioClips = new AudioClip[audioGroupLayers.Length]
                                }
                            );

                            if (debugMode)
                                Debug.LogFormat("PM> {0}: Track[{1}] id, name = {2}, {3}",
                                    func_name, s, tempInfo.soundtracks[s].id, tempInfo.soundtracks[s].name);

                            if (s == project.trackIndex)
                                project.trackId = tempInfo.soundtracks[s].id;
                        }
                    }
                    else
                        Debug.LogWarning($"PM> {func_name}: Project has no tracks!");

                    if (debugMode)
                        Debug.Log($"PM> {func_name}: project.numTracks = {project.numTracks}");
                }
            }
            else
                errMsg = $"PM> ERROR:{func_name}: jsonResponse is null!";

            if ("" != errMsg)
            {
                Debug.LogError(errMsg);
                OnRealTimeStatus?.Invoke(errMsg);
            }
            else
            {
                didParse = true;
            }

            SendOnProjectInfoLoaded();

            yield return didParse;
        }

        //----------------------------------------------------------
        private IEnumerator ParseTrackInfoAsync(PMTrack track, string jsonResponse)
        {
            string func_name = "Core.ParseTrackInfoAsync()";
            if (debugMode)
                Debug.Log($"PM> {func_name}");

            bool didParse = false;
            string errMsg = "";

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                int trackIdx = GetTrackIndexFromId(track.id);
                if (-1 == trackIdx)
                {
                    errMsg = $"PM> ERROR:{func_name}: Invalid Track ID! {track.id}";
                    goto l_end;
                }

                if (pmso.saveMetaToCache)
                {
                    string jsonName = GetAbsoluteFilePath(String.Format(
                        $"{cachePath}/{cachePathProjects}/" +
                        $"{currentProject.id}/" +
                        $"{track.id}_track" +
                        $"{PMExtensions.JSON}"
                    ), false);

                    if (!File.Exists(jsonName))
                        SaveJsonToCache(jsonResponse, jsonName);
                }

                ServerArrangementsDataV1 tempInfo = JsonUtility.FromJson<ServerArrangementsDataV1>(jsonResponse);
                if (null != tempInfo)
                {
                    if (debugMode)
                        Debug.LogFormat("PM> track id, name, length = {0}, {1}, {2}",
                            track.id, track.name, tempInfo.song.length);

                    if (null != tempInfo.arrangements)
                    {
                        track.aai_url = tempInfo.aai_url;
                        track.image_url = tempInfo.song.image_url;
                        track.artist = tempInfo.artist.name;
                        track.arrangements = new List<PMArrangement>();
                        track.num_arrangements = tempInfo.arrangements.Length;

                        for (int a = 0; a<track.num_arrangements; a++)
                        {
                            PMArrangement newArrangement = new PMArrangement
                            {
                                id      = tempInfo.arrangements[a].id,
                                name    = tempInfo.arrangements[a].name,
                                type_id = tempInfo.arrangements[a].type_id,
                                length  = 0.0f,
                                beats   = tempInfo.arrangements[a].beats,
                                bars    = tempInfo.arrangements[a].bars,
                                num_segment_clips = 0,
                                segment_clips = new List<PMSegmentClip>(),
                                layers = new AudioClip[5]
                            };

                            if (null != tempInfo.arrangements[a].song_clips)
                            {
                                newArrangement.num_segment_clips = tempInfo.arrangements[a].song_clips.Length;

                                for (int c = 0; c<newArrangement.num_segment_clips; c++)
                                {
                                    Int64 clip_id = tempInfo.arrangements[a].song_clips[c];
                                    int clip_idx = -1;

                                    for (int i = 0; i<tempInfo.song_clips.Length; i++)
                                    {
                                        if (tempInfo.song_clips[i].id == clip_id)
                                        {
                                            clip_idx = i;
                                            break;
                                        }
                                    }

                                    if (clip_idx > -1)
                                    {
                                        PMSegmentClip tempSeg = new PMSegmentClip
                                        {
                                            start_time = tempInfo.song_clips[clip_idx].start_time,
                                            end_time   = tempInfo.song_clips[clip_idx].end_time
                                        };
                                        newArrangement.length += (tempSeg.end_time - tempSeg.start_time);
                                        newArrangement.segment_clips.Add(tempSeg);
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"PM> Missing clip for ID={clip_id}!");
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogWarning("PM> Arrangement has no segment clips!");
                            }

                            if (debugMode)
                                Debug.LogFormat("PM> Arrangement[{0}] type_id, name = {1}, {2}",
                                    a, newArrangement.type_id, newArrangement.name);

                            track.arrangements.Add(newArrangement);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("PM> Track has no arrangements!");
                    }

                    if (debugMode)
                        Debug.LogFormat("PM> track num_arrangements = {0}", track.num_arrangements);
                }
                else
                {
                    errMsg = $"PM> ERROR:{func_name}: Failed to parse arrangement list!";
                }
            }
            else
                errMsg = $"PM> ERROR:{func_name}: jsonResponse is null!";

l_end:
            if ("" != errMsg)
            {
                Debug.LogError(errMsg);
                OnRealTimeStatus?.Invoke(errMsg);
            }
            else
            {
                didParse = true;
            }

            yield return didParse;
        }

        //----------------------------------------------------------
        private IEnumerator ParseTrackAudioAsync(PMTrack track, byte[] byteResponse)
        {
            string func_name = "Core.ParseTrackAudioAsync()";
            if (debugMode)
                Debug.Log($"PM> {func_name}");

            bool did_load = false;
            bool didParse = false;
            string errMsg = "";

            string aaiPath = GetAbsoluteFilePath(String.Format(
                $"{cachePath}/{cachePathTracks}/" +
                $"{track.track_id}/" +
                $"{track.track_id}_track" +
                $"{PMExtensions.AAI}"
            ), false);

            if (null != byteResponse || pmso.loadAudioFromCache)
            {
                if (!pmso.loadAudioFromCache || !File.Exists(aaiPath))
                { 
                    try
                    {
                        SaveBinToCache(byteResponse, aaiPath);
                        ExtractLayersFromAAI(currentProject.trackProgress.index, aaiPath);
                    } catch (Exception e)
                    {
                        errMsg = $"PM> ERROR:{func_name}: " + e.ToString();
                        goto l_end;
                    }

                    did_load = false;
                    yield return RunAsyncWithReturn<bool>
                        (LoadLayersFromCacheAsync(track), (output) => did_load = output);

                    did_load = false;
                    yield return RunAsyncWithReturn<bool>
                        (CreateFullMixAsync(currentProject.id, track), (output) => did_load = output);

                    for (int l=0; l<audioGroupLayers.Length; l++)
                    {
                        float percentage = (float)l / (float)audioGroupLayers.Length;
                        currentProject.trackProgress.progress = percentage;
                        OnTrackLoadingProgress?.Invoke(currentProject.trackProgress);

                        did_load = false;
                        yield return RunAsyncWithReturn<bool>
                            (CreateArrangementAudioFromLayerAsync(track, l), (output) => did_load = output);
                    }
                }
                else
                {
                    for (int l = 0; l<audioGroupLayers.Length; l++)
                    {
                        float percentage = (float)l / (float)audioGroupLayers.Length;
                        currentProject.trackProgress.progress = percentage;
                        OnTrackLoadingProgress?.Invoke(currentProject.trackProgress);

                        did_load = false;
                        yield return RunAsyncWithReturn<bool>
                            (LoadArrangementAudioLayerAsync(track, l), (output) => did_load = output);
                    }
                }
            }
            else
                errMsg = $"PM> ERROR:{func_name}: jsonResponse is null!";

            // Delete temp audio data to free up some memory
            for (int s = 0; s<track.layerAudioClips.Length; s++)
                track.layerAudioClips[s] = DestroyClip(track.layerAudioClips[s]);

l_end:
            if ("" != errMsg)
            {
                Debug.LogError(errMsg);
                OnRealTimeStatus?.Invoke(errMsg);
            }
            else
            {
                didParse = true;
            }

            currentProject.trackProgress.progress = 1.0f;
            OnTrackLoadingProgress?.Invoke(currentProject.trackProgress);

            // Cleanup
            if (!pmso.loadAudioFromCache && !pmso.saveAudioToCache)
            {
                if (File.Exists(aaiPath))
                    File.Delete(aaiPath);
            }

            yield return didParse;
        }

        //----------------------------------------------------------
        private IEnumerator WebApiGetTextureAsync2(string authToken, string finalUrl)
        {
            string func_name = "Core.WebApiGetTextureAsync2()";
            if (debugMode)
                Debug.Log($"PM> {func_name}");
            if (pmso.logRequestUrls)
                Debug.Log($"PM> {func_name}: finalUrl = {finalUrl}");

            // Check for offline mode
            if (pmso.offlineMode && !finalUrl.StartsWith("file"))
            {
                Debug.LogError($"PM> ERROR:{func_name}: Can't download from web in offline mode!");
                yield break;
            }

            // Send Web API request
            float time_start = Time.realtimeSinceStartup;
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(finalUrl);
            webRequest.SetRequestHeader("x-api-key", currentProject.authToken);
            yield return webRequest.SendWebRequest();
            float time_elapsed = Time.realtimeSinceStartup - time_start;

            if (debugMode || pmso.logServerResponses)
                Debug.LogFormat(
                    "PM> {0}: Downloaded {1} bytes in {2} seconds",
                    func_name, webRequest.downloadedBytes, time_elapsed.ToString("f6"));

            Texture2D textureResponse = null;

            // No luck?
            if (UnityWebRequest.Result.Success != webRequest.result)
            {
                // In progress? This should never happen here since we yield
                if (UnityWebRequest.Result.InProgress == webRequest.result)
                    Debug.LogError($"PM> ERROR:{func_name}: InProgress is not a valid result!");

                // ConnectionError, ProtocolError, DataProcessingError
                else
                    Debug.LogError($"PM> ERROR:{func_name}: " + webRequest.error);
            }
            else
                textureResponse = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;

            // Cleanup ...
            webRequest.Dispose();

            yield return textureResponse;
        }

        //----------------------------------------------------------
        private IEnumerator WebApiGetJsonAsync2(string authToken, string finalUrl)
        {
            string func_name = "Core.WebApiGetJsonAsync2()";
            if (debugMode)
                Debug.Log($"PM> {func_name}");
            if (pmso.logRequestUrls)
                Debug.Log($"PM> {func_name}: finalUrl = {finalUrl}");

            // Check for offline mode
            if (pmso.offlineMode && !finalUrl.StartsWith("file"))
            {
                Debug.LogError($"PM> ERROR:{func_name}: Can't download from web in offline mode!");
                yield break;
            }

            // Send Web API request
            float time_start = Time.realtimeSinceStartup;
            UnityWebRequest webRequest = UnityWebRequest.Get(finalUrl);
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.SetRequestHeader("x-api-key", authToken);
            yield return webRequest.SendWebRequest();
            float time_elapsed = Time.realtimeSinceStartup - time_start;

            if (debugMode || pmso.logServerResponses)
                Debug.LogFormat(
                    "PM> {0}: Downloaded {1} bytes in {2} seconds",
                    func_name, webRequest.downloadedBytes, time_elapsed.ToString("f6"));

            string jsonResponse = null;

            // No luck?
            if (UnityWebRequest.Result.Success != webRequest.result)
            {
                // In progress? This should never happen here since we yield
                if (UnityWebRequest.Result.InProgress == webRequest.result)
                    Debug.LogError($"PM> ERROR:{func_name}: InProgress is not a valid result!");

                // ConnectionError, ProtocolError, DataProcessingError
                else
                { 
                    Debug.LogError($"PM> ERROR:{func_name}: " + webRequest.error);
                    Debug.LogFormat("webRequest.downloadHandler.text; = {0}", webRequest.downloadHandler.text);
                }
            }
            else
            { 
                jsonResponse = webRequest.downloadHandler.text;

                if (pmso.logServerResponses)
                    Debug.LogFormat("PM> jsonResponse = {0}", Regex.Replace(jsonResponse, @"\r\n?|\n", ""));
            }

            // Cleanup ...
            webRequest.Dispose();

            yield return jsonResponse;
        }

        //----------------------------------------------------------
        private IEnumerator WebApiGetBinaryAsync2(string authToken, string finalUrl)
        {
            string func_name = "Core.WebApiGetBinaryAsync2()";
            if (debugMode)
                Debug.Log($"PM> {func_name}");
            if (pmso.logRequestUrls)
                Debug.Log($"PM> {func_name}: finalUrl = {finalUrl}");

            // Check for offline mode
            if (pmso.offlineMode && !finalUrl.StartsWith("file"))
            {
                Debug.LogError($"PM> ERROR:{func_name}: Can't download from web in offline mode!");
                yield break;
            }

            // Send Web API request
            float time_start = Time.realtimeSinceStartup;
            UnityWebRequest webRequest = UnityWebRequest.Get(finalUrl);
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.SetRequestHeader("x-api-key", authToken);
            yield return webRequest.SendWebRequest();
            float time_elapsed = Time.realtimeSinceStartup - time_start;

            if (debugMode || pmso.logServerResponses)
                Debug.LogFormat(
                    "PM> {0}: Downloaded {1} bytes in {2} seconds",
                    func_name, webRequest.downloadedBytes, time_elapsed.ToString("f6"));

            byte[] byteResponse = null;

            // No luck?
            if (UnityWebRequest.Result.Success != webRequest.result)
            {
                // In progress? This should never happen here since we yield
                if (UnityWebRequest.Result.InProgress == webRequest.result)
                    Debug.LogError($"PM> ERROR:{func_name}: InProgress is not a valid result!");

                // ConnectionError, ProtocolError, DataProcessingError
                else
                {
                    Debug.LogError($"PM> ERROR:{func_name}: " + webRequest.error);
                    Debug.LogError($"PM> ERROR:{func_name}: " + finalUrl);
                }
            }
            else
                byteResponse = webRequest.downloadHandler.data;

            // Cleanup ...
            webRequest.Dispose();

            yield return byteResponse;
        }

        //----------------------------------------------------------
        private IEnumerator WebApiGetAudioClipAsync2(string authToken, string finalUrl, AudioType type)
        {
            string func_name = "Core.WebApiGetAudioClipAsync2()";
            if (debugMode)
                Debug.Log($"PM> {func_name}");
            if (pmso.logRequestUrls)
                Debug.Log($"PM> {func_name}: finalUrl = {finalUrl}");

            // Check for offline mode
            if (pmso.offlineMode && !finalUrl.StartsWith("file"))
            {
                Debug.LogError($"PM> ERROR:{func_name}: Can't download from web in offline mode!");
                yield break;
            }

            // Send Web API request
            float time_start = Time.realtimeSinceStartup;
            UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(finalUrl, type);
            webRequest.SetRequestHeader("x-api-key", authToken);
            yield return webRequest.SendWebRequest();
            float time_elapsed = Time.realtimeSinceStartup - time_start;

            if (debugMode || pmso.logServerResponses)
                Debug.LogFormat(
                    "PM> {0}: Downloaded {1} bytes in {2} seconds",
                    func_name, webRequest.downloadedBytes, time_elapsed.ToString("f6"));

            AudioClip audioResponse = null;

            // No luck?
            if (UnityWebRequest.Result.Success != webRequest.result)
            {
                // In progress? This should never happen here since we yield
                if (UnityWebRequest.Result.InProgress == webRequest.result)
                    Debug.LogError($"PM> ERROR:{func_name}: InProgress is not a valid result!");

                // ConnectionError, ProtocolError, DataProcessingError
                else
                    Debug.LogError($"PM> ERROR:{func_name}: " + webRequest.error);
            }
            else
            { 
                audioResponse = DownloadHandlerAudioClip.GetContent(webRequest);
            }

            if (debugMode)
            { 
                time_elapsed = Time.realtimeSinceStartup - time_start;
                Debug.LogFormat("PM> Time elapsed {0} seconds", time_elapsed.ToString("f6"));
            }

            // Cleanup ...
            webRequest.Dispose();

            yield return audioResponse;
        }

        //----------------------------------------------------------
        private IEnumerator LoadProjectInfoAsync(PMProject project)
        {
            string func_name = "Core.LoadProjectInfoAsync()";
            if (debugMode)
                Debug.Log($"PM> {func_name}: {project.id}");

            OnRealTimeStatus?.Invoke($"Loading project [{project.id}] ...");

            if (0 == project.id)
            {
                string projectString = null;

                string finalUrl = String.Format("{0}default-project", settings.base_url);
                yield return RunAsyncWithReturn<string>(
                    WebApiGetJsonAsync2(project.authToken, finalUrl), (output) => projectString = output);

                // Parse project info here
                bool did_parse = false;
                yield return RunAsyncWithReturn<bool>
                    (ParseDefaultProjectInfoAsync(project, projectString), (output) => did_parse = output);
            }

            if (0 != project.id)
            {
                string jsonString = null;

                // Try to load from local cache
                if (pmso.loadMetaFromCache)
                {
                    string loaclPath = GetAbsoluteFilePath(String.Format(
                        $"{cachePath}/{cachePathProjects}/" +
                        $"{project.id}/" +
                        $"{project.id}_project" +
                        $"{PMExtensions.JSON}"
                    ), false);

                    if (File.Exists(loaclPath))
                    {
                        loaclPath = "file://" + loaclPath;
                        if (pmso.logRequestUrls)
                            Debug.Log($"PM> {func_name}: loaclPath = {loaclPath}");
                        yield return RunAsyncWithReturn<string>(
                            WebApiGetJsonAsync2(project.authToken, loaclPath), (output) => jsonString = output);
                    }
                }

                // If not yet loaded, try to load from web api
                if (string.IsNullOrEmpty(jsonString))
                {
                    string finalUrl = settings.base_url + "projects/" + project.id + "/hierarchy";
                    if (pmso.logRequestUrls)
                        Debug.Log($"PM> {func_name}: finalUrl = {finalUrl}");

                    yield return RunAsyncWithReturn<string>(
                        WebApiGetJsonAsync2(project.authToken, finalUrl), (output) => jsonString = output);
                }

                if (null != jsonString)
                {
                    bool did_parse = false;
                    yield return RunAsyncWithReturn<bool>(
                        ParseProjectInfoAsync(project, jsonString), (output) => did_parse = output);

                    if (debugMode)
                        Debug.Log($"PM> {func_name}: did_parse = {did_parse}");

                    if (did_parse)
                    { 
                        project.isLoaded = true;
                        if (debugMode)
                            Debug.Log($"PM> {func_name}: projectInfo.isLoaded = {project.isLoaded}");

                        OnRealTimeStatus?.Invoke($"Project [{project.id}] '{project.name}' loaded");
                        EventManager.SetStatus(PMEventStatus.EventWasSuccessful);

                        // Send Unity VSA event
                        if (Application.isEditor && plusMusicAccount.IsFromUnityStore)
                        {
                            // Check if we need to reset the attribution flag
                            if ((plusMusicAccount.DeviceId != SystemInfo.deviceUniqueIdentifier) ||
                                (pmso.authToken != project.authToken))
                            {
                                plusMusicAccount.DidVSAttribution = false;
                            }

                            if (!plusMusicAccount.DidVSAttribution)
                            {
                                if (null != VSASendAttributionEvent)
                                {
                                    string customerUid =
                                        SystemInfo.deviceUniqueIdentifier + "|" +
                                        project.authToken + "|" + pluginVersion;
                                    plusMusicAccount.DidVSAttribution = VSASendAttributionEvent("Initial Project Load", "PlusMusic", customerUid);
                                }
                                else
                                    Debug.LogError("PM> Core.LoadProject(): VSASendAttributionEvent is null!");
                            }
                        }

                        // Call home with some useful info
                        if (pmso.doPingbacks)
                        {
                            string eventStr = "Load Project";
                            if (!didStartupPingback)
                            {
                                didStartupPingback = true;
                                eventStr = "Start of Game";
                            }

                            SendPingBackInfo(
                                new PMPingBackInfo
                                {
                                    eventText = eventStr, pingProjectId = project.id,
                                    pingArrangementId = 0, pingTag = "",
                                    pingTransitionType = "", pingTransitionTiming = "",
                                    pingTransitionDelay = 0.0f, isUsingStinger = false
                                }
                            );
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"PM> ERROR:{func_name}: Project ID is 0!");
            }

            EventManager.ContinueProcessing();
        }

        //----------------------------------------------------------
        private IEnumerator LoadTrackInfoAsync(PMTrack track)
        {
            string func_name = "Core.LoadTrackInfoAsync()";
            if (debugMode)
                Debug.Log($"PM> {func_name}: {track.id}");

            string trackString = null;

            OnRealTimeStatus?.Invoke($"Loading track '{track.name}' ...");

            // Try to load from local cache
            if (pmso.loadMetaFromCache)
            {
                string loaclPath = GetAbsoluteFilePath(String.Format(
                    $"{cachePath}/{cachePathProjects}/" +
                    $"{currentProject.id}/" +
                    $"{track.id}_track" +
                    $"{PMExtensions.JSON}"
                ), false);

                if (File.Exists(loaclPath))
                {
                    loaclPath = "file://" + loaclPath;
                    if (pmso.logRequestUrls)
                        Debug.Log($"PM> {func_name}: loaclPath = {loaclPath}");
                    yield return RunAsyncWithReturn<string>(
                    WebApiGetJsonAsync2(currentProject.authToken, loaclPath), (output) => trackString = output);
                }
            }

            // If not yet loaded, try to load from web api
            if (string.IsNullOrEmpty(trackString))
            {
                string finalUrl = settings.base_url + "projects/" + track.id;
                yield return RunAsyncWithReturn<string>(
                    WebApiGetJsonAsync2(currentProject.authToken, finalUrl), (output) => trackString = output);
            }

            if (null != trackString)
            {
                bool did_parse = false;
                yield return RunAsyncWithReturn<bool>(
                    ParseTrackInfoAsync(track, trackString), (output) => did_parse = output);

                if (did_parse)
                { 
                    EventManager.SetStatus(PMEventStatus.EventWasSuccessful);
                    track.isLoaded = true;
                }
            }

            EventManager.ContinueProcessing();
        }

        //----------------------------------------------------------
        private IEnumerator LoadTrackAudioAsync(PMTrack track)
        {
            string func_name = "Core.LoadTrackAudioAsync()";
            if (debugMode)
                Debug.Log($"PM> {func_name}: {track.id}");

            string errMsg = "";
            byte[] trackAudio = null;

            OnRealTimeStatus?.Invoke($"Loading track '{track.name}' ...");

            // Try to load from local cache
            if (pmso.loadAudioFromCache)
            {
                string loaclPath = GetAbsoluteFilePath(String.Format(
                    $"{cachePath}/{cachePathTracks}/" +
                    $"{track.track_id}/" +
                    $"{track.track_id}_track" +
                    $"{PMExtensions.AAI}"
                ), false);

                if (File.Exists(loaclPath))
                {
                    loaclPath = "file://" + loaclPath;
                    if (pmso.logRequestUrls)
                        Debug.Log($"PM> {func_name}: loaclPath = {loaclPath}");
                    yield return RunAsyncWithReturn<byte[]>(
                        WebApiGetBinaryAsync2(currentProject.authToken, loaclPath), (output) => trackAudio = output);
                }
            }

            // If not yet loaded, try to load from web api
            if (null == trackAudio)
            {
                string finalUrl = track.aai_url;
                if (string.IsNullOrEmpty(finalUrl))
                {
                    errMsg = $"PM> ERROR:{func_name}: AAI download url missing!";
                    goto l_end;
                }
                yield return RunAsyncWithReturn<byte[]>(
                    WebApiGetBinaryAsync2(currentProject.authToken, finalUrl), (output) => trackAudio = output);
            }

            if (null != trackAudio || pmso.loadAudioFromCache)
            {
                bool did_parse = false;
                yield return RunAsyncWithReturn<bool>(
                    ParseTrackAudioAsync(track, trackAudio), (output) => did_parse = output);

                if (did_parse)
                {
                    track.isLoaded = true;
                    track.hasAudio = true;
                    EventManager.SetStatus(PMEventStatus.EventWasSuccessful);
                }
            }
l_end:
            if ("" != errMsg)
            {
                Debug.LogError(errMsg);
                OnRealTimeStatus?.Invoke(errMsg);
            }

            currentProject.trackProgress.isLoading = false;
            EventManager.ContinueProcessing();
        }

        //----------------------------------------------------------
        private IEnumerator LoadTrackImageAsync(PMTrack track)
        {
            string func_name = "Core.LoadTrackImageAsync()";
            if (debugMode)
                Debug.Log($"PM> {func_name}: {track.id}");

            Texture2D trackImage = null;

            OnRealTimeStatus?.Invoke($"Loading track '{track.name}' ...");

            // Try to load from local cache
            if (pmso.loadMetaFromCache)
            {
                string loaclPath = GetAbsoluteFilePath(String.Format(
                    $"{cachePath}/{cachePathTracks}/" +
                    $"{track.track_id}/" +
                    $"{track.track_id}_artwork" +
                    $"{PMExtensions.PNG}"
                ), false);

                if (File.Exists(loaclPath))
                {
                    loaclPath = "file://" + loaclPath;
                    if (pmso.logRequestUrls)
                        Debug.Log($"PM> {func_name}: loaclPath = {loaclPath}");
                    yield return RunAsyncWithReturn<Texture2D>(
                    WebApiGetTextureAsync2(currentProject.authToken, loaclPath), (output) => trackImage = output);
                }
            }

            // If not yet loaded, try to load from web api
            if (null == trackImage)
            { 
                string finalUrl = track.image_url;
                if (string.IsNullOrEmpty(finalUrl))
                {
                    Debug.LogError($"PM> ERROR:{func_name}: Image download url missing!");
                    goto l_end;
                }

                yield return RunAsyncWithReturn<Texture2D>(
                        WebApiGetTextureAsync2(currentProject.authToken, finalUrl), (output) => trackImage = output);
            }

            if (null != trackImage)
            {
                if (pmso.saveAudioToCache)
                {
                    string loaclPath = GetAbsoluteFilePath(String.Format(
                        $"{cachePath}/{cachePathTracks}/" +
                        $"{track.track_id}/" +
                        $"{track.track_id}_artwork" +
                        $"{PMExtensions.PNG}"
                    ), false);

                    if (!File.Exists(loaclPath))
                        SaveTextureToCache(trackImage, loaclPath);
                }

                track.AlbumCover = trackImage;
                EventManager.SetStatus(PMEventStatus.EventWasSuccessful);
            }
l_end:
            EventManager.ContinueProcessing();
        }


        #endregion

    }
}

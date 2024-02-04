/* ---------------------------------------------------------------------------
Application:    PlusMusic Unity Plugin - Settings Panel
Copyright:      PlusMusic, (c) 2023
Author:         Andy Schmidt
Description:    Core plugin utility functions

TODO:
    Important todo items are marked with a $$$ comment

NOTES:

--------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlusMusic;
using PlusMusicTypes;


namespace PlusMusic
{
    public class PlusMusicSettingsPanel: MonoBehaviour
    {
        public static PlusMusicSettingsPanel Instance;

        private InputField projectIDInputField;
        private InputField authTokenInputField;
        //private Button loadProjectButton;
        private Slider volumeSlider;
        private Dropdown trackDropdown;
        private Button stopPlayButton;
        private Button pausePlayButton;
        private Button startPlayButton;
        private Text playTextField;
        private Text statusTextField;
        private bool pauseState = false;
        private PMTrackInfo trackInfo;
        private PMMessageProjectInfo projectInfoData;
        private Color statusColor;
        private Button[] arrangementButtons;
        private Color[] arrangementColors;
        private Color lightGreen = new Color(0.5507298f, 0.9811321f, 0.7152745f, 1.0f);
        private bool showDebug   = false;
        private bool hasStarted  = false;
        private int selectedIdx  = 0;
        private PMSettingsSo pluginSettings;

        [HideInInspector]
        public bool trackIsLoaded = false;
        public PMTransitionInfo transition = new PMTransitionInfo();


        //----------------------------------------------------------
        private void Awake()
        {
            Instance = this;
        }

        //----------------------------------------------------------
        private void Start()
        {
            if (null == PlusMusicCore.Instance) {
                Debug.LogError("PM> ERROR:PlusMusicSettingsPanel.Start(): There is no PlusMusicCore in the scene!"); 
                return; 
            }

            trackIsLoaded = false;

            // Get needed UI components
            projectIDInputField = GameObject.Find("inputProjectId").GetComponent<InputField>();
            authTokenInputField = GameObject.Find("inputAuthToken").GetComponent<InputField>();
            //loadProjectButton   = GameObject.Find("btnLoad").GetComponent<Button>();
            volumeSlider        = GameObject.Find("sliderVolume").GetComponent<Slider>();
            trackDropdown       = GameObject.Find("dropTrackList").GetComponent<Dropdown>();
            stopPlayButton      = GameObject.Find("btnStop").GetComponent<Button>();
            pausePlayButton     = GameObject.Find("btnPause").GetComponent<Button>();
            startPlayButton     = GameObject.Find("btnPlay").GetComponent<Button>();
            playTextField       = GameObject.Find("txtPlayInfo").GetComponent<Text>();
            statusTextField     = GameObject.Find("txtStatus").GetComponent<Text>();

            arrangementButtons = new Button[PlusMusicCore.Instance.ArrangementTypeMax+1];
            arrangementColors  = new Color[PlusMusicCore.Instance.ArrangementTypeMax+1];
            arrangementButtons[(int)PMTags.high_backing]  = GameObject.Find("btn_highbacking").GetComponent<Button>();
            arrangementButtons[(int)PMTags.low_backing]   = GameObject.Find("btn_lowbacking").GetComponent<Button>();
            arrangementButtons[(int)PMTags.backing_track] = GameObject.Find("btn_backing_track").GetComponent<Button>();
            arrangementButtons[(int)PMTags.preview]       = GameObject.Find("btn_preview").GetComponent<Button>();
            arrangementButtons[(int)PMTags.victory]       = GameObject.Find("btn_victory").GetComponent<Button>();
            arrangementButtons[(int)PMTags.failure]       = GameObject.Find("btn_failure").GetComponent<Button>();
            arrangementButtons[(int)PMTags.highlight]     = GameObject.Find("btn_highlight").GetComponent<Button>();
            arrangementButtons[(int)PMTags.lowlight]      = GameObject.Find("btn_lowlight").GetComponent<Button>();
            arrangementButtons[(int)PMTags.full_song]     = GameObject.Find("btn_full_song").GetComponent<Button>();
            GetArrangementColors();

            pluginSettings = PlusMusicCore.Instance.GetSettings;

            // Project ID Input
            if (null != projectIDInputField)
            {
                if (pluginSettings.autoLoadProject)
                    projectIDInputField.interactable = false;
                projectIDInputField.text = pluginSettings.projectId.ToString();
            }
            else
                Debug.LogWarning("PM> PlusMusicSettingsPanel.Start(): 'projectIDTextField' is null!");

            // Auth Token Input
            if (null != authTokenInputField)
            {
                if (pluginSettings.autoLoadProject)
                    authTokenInputField.interactable = false;
                authTokenInputField.text = pluginSettings.authToken;
            }
            else
                Debug.LogWarning("PM> PlusMusicSettingsPanel.Start(): 'authTokenInputField' is null!");

            // Load Project Button
            /*
            if (null != loadProjectButton)
            {
                if (pluginSettings.autoLoadProject)
                    loadProjectButton.interactable = false;
                loadProjectButton.onClick.AddListener(LoadProject);
            }
            else
                Debug.LogWarning("PM> PlusMusicSettingsPanel.Start(): 'loadProjectButton' is null!");
            */

            // Volume Slider
            if (null != volumeSlider)
            {
                volumeSlider.value = PlusMusicCore.Instance.GetPluginVolume;
                volumeSlider.onValueChanged.AddListener(SetTrackVolume);
            }
            else
                Debug.LogWarning("PM> PlusMusicSettingsPanel.Start(): 'volumeSlider' is null!");

            // Stop Play Button
            if (null != stopPlayButton)
            {
                if (pluginSettings.autoLoadProject)
                    stopPlayButton.interactable = false;
                stopPlayButton.onClick.AddListener(StopPlay);
            }
            else
                Debug.LogWarning("PM> PlusMusicSettingsPanel.Start(): 'stopPlayButton' is null!");

            // Pause Play Button
            if (null != pausePlayButton)
            {
                if (pluginSettings.autoLoadProject)
                    pausePlayButton.interactable = false;
                pausePlayButton.onClick.AddListener(PausePlay);
            }
            else
                Debug.LogWarning("PM> PlusMusicSettingsPanel.Start(): 'pausePlayButton' is null!");

            // Start Play Button
            if (null != startPlayButton)
            {
                if (pluginSettings.autoLoadProject)
                    startPlayButton.interactable = false;
                startPlayButton.onClick.AddListener(StartPlay);
            }
            else
                Debug.LogWarning("PM> PlusMusicSettingsPanel.Start(): 'startPlayButton' is null!");

            // Hooking the needed PlusMusicCore events
            PlusMusicCore.Instance.OnTrackLoadingProgress += SetLoadingProgress;

            // Track select dropdown
            if (null != trackDropdown)
            {
                trackDropdown.ClearOptions();
                if (pluginSettings.autoLoadProject)
                    trackDropdown.interactable = false;
                trackDropdown.onValueChanged.AddListener(SetTrackOnChange);
                PlusMusicCore.Instance.OnProjectInfoLoaded += SetProjectInfo;
            }
            else 
                Debug.LogWarning("PM> PlusMusicSettingsPanel.Start(): 'trackDropdown' is null!");

            // Arrangement play info field
            if (null != playTextField)
            {
                playTextField.text = "";
            }
            else
                Debug.LogWarning("PM> PlusMusicSettingsPanel.Start(): 'playTextField' is null!");

            // Debug status field
            if (null != statusTextField)
            {
                statusColor = statusTextField.color;
                statusTextField.text = "asdfasdfasdf";
                PlusMusicCore.Instance.OnRealTimeStatus += SetStatusText;
            }
            else
                Debug.LogWarning("PM> PlusMusicSettingsPanel.Start(): 'statusTextField' is null!");

            Instance.showDebug = pluginSettings.debugMode;
            hasStarted = true;
        }

        //----------------------------------------------------------
        private void Update()
        {
            if (PlusMusicCore.Instance.GetIsAudioPlaying)
            {
                AudioSource source = PlusMusicCore.Instance.GetCurrentAudioSource();
                if (null != source)
                {
                    string arrangementName = "-";
                    PMTransitionInfo currentTransition = PlusMusicCore.Instance.GetCurrentTransition;
                    if (null != currentTransition)
                        arrangementName = String.Format("{0}", currentTransition.tag);

                    string msg = String.Format(
                        "Playing '{0}' for {1:F2}/{2:F2} seconds",
                        arrangementName, source.time, source.clip.length
                    );
                    SetPlayText(msg);
                }
            }
        }

        //----------------------------------------------------------
        // Cleanup 
        //----------------------------------------------------------
        private void OnDestroy()
        {
            if (null == PlusMusicCore.Instance) { return; }

            // Unhooking PlusMusicCore events
            if (null != trackDropdown)
                PlusMusicCore.Instance.OnProjectInfoLoaded -= SetProjectInfo;
            if (null != statusTextField)
                PlusMusicCore.Instance.OnRealTimeStatus -= SetStatusText;

            PlusMusicCore.Instance.OnTrackLoadingProgress -= SetLoadingProgress;

            // Remove all runtime listeners
            //if (null != loadProjectButton)
            //    loadProjectButton.onClick.RemoveAllListeners();
            if (null != volumeSlider)
                volumeSlider.onValueChanged.RemoveAllListeners();
            if (null != stopPlayButton)
                stopPlayButton.onClick.RemoveAllListeners();
            if (null != pausePlayButton)
                pausePlayButton.onClick.RemoveAllListeners();
            if (null != startPlayButton)
                startPlayButton.onClick.RemoveAllListeners();
            if (null != trackDropdown)
                trackDropdown.onValueChanged.RemoveAllListeners();
        }

        //----------------------------------------------------------
        public void SetLoadingProgress(PMTrackProgress progress)
        {
            // Check if we're ready and if this is our Track
            if (!Instance.hasStarted) return;

            if (progress.progress < 1.0f)
            {
                Instance.trackIsLoaded = false;
            }
            else
            {
                trackInfo = PlusMusicCore.Instance.GetTrackInfoByIndex(Instance.selectedIdx);
                Instance.trackIsLoaded = true;

                SetArrangementColors();

                trackDropdown.interactable       = true;
                //projectIDInputField.interactable = true;
                //authTokenInputField.interactable = true;
                //loadProjectButton.interactable   = true;
                stopPlayButton.interactable      = true;
                pausePlayButton.interactable     = true;
                startPlayButton.interactable     = true;
            }
        }

        //----------------------------------------------------------
        public void SetProjectInfo(PMMessageProjectInfo eventProjectInfoData)
        {
            if (showDebug)
                Debug.Log("PM> PlusMusicSettingsPanel.SetProjectInfo");

            try
            {
                Instance.projectInfoData = eventProjectInfoData;
                trackDropdown.ClearOptions();

                if (null == Instance.projectInfoData.tracks || Instance.projectInfoData.tracks.Length < 1)
                {
                    Debug.LogWarning("PM> PlusMusicSettingsPanel.SetProjectInfo(): No tracks!");
                    return;
                }

                int idx = 0;
                Instance.selectedIdx = 0;
                List<string> dropOptions = new List<string>();
                foreach (PMMessageTrackList track in eventProjectInfoData.tracks)
                {
                    dropOptions.Add(track.name);
                    if (track.isSelected)
                        Instance.selectedIdx = idx;
                    idx++;
                }

                trackDropdown.AddOptions(dropOptions);
                trackDropdown.value = Instance.selectedIdx;

            } catch (Exception e)
            {
                Debug.LogError("PM> ERROR:PlusMusicSettingsPanel.SetProjectInfo()");
                Debug.LogException(e, this);
            }
        }

        //----------------------------------------------------------
        private void GetArrangementColors()
        {
            int sel = trackDropdown.value;

            for (int b = 1; b<arrangementButtons.Length; b++)
            {
                arrangementColors[b] = arrangementButtons[b].GetComponent<Image>().color;
            }
        }

        //----------------------------------------------------------
        private void SetArrangementColors()
        {
            if (null == trackInfo) {  return; }

            try
            {
                for (int a = 1; a<arrangementButtons.Length; a++)
                    arrangementButtons[a].GetComponent<Image>().color = arrangementColors[a];
 
                if (null != trackInfo.arrangements)
                {
                    for (int a = 0; a<trackInfo.arrangements.Count; a++)
                    {
                        arrangementButtons[trackInfo.arrangements[a].type_id].GetComponent<Image>().color = lightGreen;
                    }
                }

            } catch (Exception e)
            {
                Debug.LogError("PM> ERROR:PlusMusicSettingsPanel.SetArrangementColors()");
                Debug.LogException(e, this);
            }
        }

        //----------------------------------------------------------
        // Handles changing track selection
        public void SetTrackOnChange(Int32 index)
        {
            if (Instance.showDebug)
                Debug.Log("PM> PlusMusicSettingsPanel.SetTrackOnChange(): " + index);

            if (selectedIdx == index) { return; }
            selectedIdx = index;

            Instance.trackIsLoaded = false;
            SetArrangementColors();
            PlusMusicCore.Instance.PlayTrack(projectInfoData.tracks[index].id);
        }

        //----------------------------------------------------------
        // Sets the Track volume
        public void SetTrackVolume(float volume)
        {
            if (Instance.showDebug)
                Debug.Log("PM> PlusMusicSettingsPanel.SetTrackVolume(): " + volume);

            for (int l=0; l<5; l++)
            { 
                PlusMusicCore.Instance.SetLayerVolume((PMAudioLayers)l, volume);
                Instance.transition.layerVolumes.SetByIndex(l, volume);
            }
        }

        //----------------------------------------------------------
        // Sets the Arrangement play info text
        public void SetPlayText(string playText)
        {
            playTextField.text = playText;
        }

        //----------------------------------------------------------
        // Sets the status text
        public void SetStatusText(string statusText)
        {
            // Check for errors
            if (statusText.StartsWith("PM> ERROR:"))
                statusTextField.color = Color.red;
            else
                statusTextField.color = statusColor;

            statusTextField.text = statusText;
        }

        //----------------------------------------------------------
        // Load the supplied project
        public void LoadProject()
        {
            if (Instance.showDebug)
                Debug.Log("PM> PlusMusicSettingsPanel.LoadProject()");

            trackDropdown.ClearOptions();
            playTextField.text = "";
            statusTextField.text = "Not implemented yet!";
            SetArrangementColors();

            // $$$ 
            //PlusMusicCore.Instance.LoadProject(Int64.Parse(projectIDInputField.text), authTokenInputField.text, 0);
        }

        //----------------------------------------------------------
        public void StartPlay()
        {
            if (Instance.showDebug)
                Debug.Log("PM> PlusMusicSettingsPanel.StartPlay()");

            PlusMusicCore.Instance.StartPlay();
        }

        //----------------------------------------------------------
        public void PausePlay()
        {
            if (Instance.showDebug)
                Debug.Log("PM> PlusMusicSettingsPanel.PausePlay(): " + pauseState);

            if (!pauseState)
            {
                pausePlayButton.GetComponentInChildren<Text>().text = "Resume";
                PlusMusicCore.Instance.PausePlay();
            }
            else
            {
                pausePlayButton.GetComponentInChildren<Text>().text = "Pause";
                PlusMusicCore.Instance.UnPausePlay();
            }

            pauseState = !pauseState;
        }

        //----------------------------------------------------------
        public void StopPlay()
        {
            if (Instance.showDebug)
                Debug.Log("PM> PlusMusicSettingsPanel.StopPlay()");

            PlusMusicCore.Instance.StopPlay();
        }

        //----------------------------------------------------------
        public void PlayHighBacking()
        {
            Instance.transition.tag = PMTags.high_backing;
            PlayTransition();
        }

        //----------------------------------------------------------
        public void PlayLowBacking()
        {
            Instance.transition.tag = PMTags.low_backing;
            PlayTransition();
        }

        //----------------------------------------------------------
        public void PlayBackingTrack()
        {
            Instance.transition.tag = PMTags.backing_track;
            PlayTransition();
        }

        //----------------------------------------------------------
        public void PlayPreview()
        {
            Instance.transition.tag = PMTags.preview;
            PlayTransition();
        }

        //----------------------------------------------------------
        public void PlayVictory()
        {
            Instance.transition.tag = PMTags.victory;
            PlayTransition();
        }

        //----------------------------------------------------------
        public void PlayFailure()
        {
            Instance.transition.tag = PMTags.failure;
            PlayTransition();
        }

        //----------------------------------------------------------
        public void PlayHighlight()
        {
            Instance.transition.tag = PMTags.highlight;
            PlayTransition();
        }

        //----------------------------------------------------------
        public void PlayLowlight()
        {
            Instance.transition.tag = PMTags.lowlight;
            PlayTransition();
        }

        //----------------------------------------------------------
        public void PlayFullTrack()
        {
            Instance.transition.tag = PMTags.full_song;
            PlayTransition();
        }

        //----------------------------------------------------------
        private void PlayTransition()
        {
            if (Instance.showDebug)
                Debug.Log($"PM> PlusMusicSettingsPanel.PlayTransition({Instance.transition.tag})");

            if (Instance.transition.useMainVolume)
                SetTrackVolume(Instance.transition.mainVolume);

            PlusMusicCore.Instance.PlayArrangement(Instance.transition);
        }
    }
}

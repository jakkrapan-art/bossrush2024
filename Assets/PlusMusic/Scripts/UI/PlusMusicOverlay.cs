/* ---------------------------------------------------------------------------
Application:    PlusMusic Unity Plugin - UI Overlay
Copyright:      PlusMusic, (c) 2023
Author:         Andy Schmidt
Description:    UI overlay for the core plugin functions

TODO:
    Important todo items are marked with a $$$ comment

NOTES:

--------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using PlusMusic;
using PlusMusicTypes;


namespace PlusMusic
{
    public class PlusMusicOverlay: MonoBehaviour
    {
        public GameObject uiCanvas;
        public Slider loadingBar;
        public TMP_Text loadingText;
        public Dropdown trackDropdown;
        public Slider sliderMain;
        public Slider sliderBass;
        public Slider sliderDrums;
        public Slider sliderTopMix;
        public Slider sliderVocals;
        public Image albumCover;
        public Sprite noImage;
        public Color debugColor = new Color(0.1603f, 0.1603f, 0.1603f, 1.0f);

        public bool HasLoaded { get => hasLoaded; set => hasLoaded = value; }

        private GraphicRaycaster ui_raycaster;
        private PointerEventData click_data;
        private List<RaycastResult> click_results;
        private TMP_Text infoText;
        private TMP_Text plusMusicVersion;
        private TMP_Text playTextField;
        private PMMessageProjectInfo projectInfoData;
        private bool doVersion    = true;
        private bool hasStarted   = false;
        private bool hasLoaded    = false;
        private string trackName  = "-";
        private string artistName = "-";
        private string arrangementName = "-";
        private TMP_Text trackText;
        private TMP_Text artistText;
        private TMP_Text arrangementText;
        private TMP_Text intensityText;
        private TMP_Text layerTitle;
        private float layer_intensity = 0.0f;
        private int selectedIdx  = 0;
        private bool showDebug   = false;
        private Color errorColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);


        //----------------------------------------------------------
        private void Awake()
        {
            infoText         = GameObject.Find("dbgText").GetComponent<TMP_Text>();
            plusMusicVersion = GameObject.Find("pluginVersion").GetComponent<TMP_Text>();
            trackText        = GameObject.Find("txtTitle").GetComponent<TMP_Text>();
            artistText       = GameObject.Find("txtArtist").GetComponent<TMP_Text>();
            arrangementText  = GameObject.Find("txtArrangement").GetComponent<TMP_Text>();
            layerTitle       = GameObject.Find("LayerMixer").GetComponent<TMP_Text>();
            intensityText    = GameObject.Find("txtIntensity").GetComponent<TMP_Text>();
            playTextField    = GameObject.Find("txtPlayInfo").GetComponent<TMP_Text>();

            // NOTE: This needs to be in Awake() because the Core offline mode is too fast
            // and this Start() function might be called after SetProjectInfo() was already invoked.
            trackDropdown.ClearOptions();
        }

        //----------------------------------------------------------
        // Start is called before the first frame update
        private void Start()
        {
            if (null == PlusMusicCore.Instance)
            {
                Debug.LogError("PM> ERROR:PlusMusicUI.Start(): There is no PlusMusicCore in the scene!");
                return;
            }

            // Hook event listeners
            PlusMusicCore.Instance.OnProjectInfoLoaded += SetProjectInfo;
            PlusMusicCore.Instance.OnArrangementChanged += SetArrangement;
            PlusMusicCore.Instance.OnRealTimeStatus += SetDebugText;
            PlusMusicCore.Instance.OnTrackLoadingProgress += SetLoadingProgress;
            PlusMusicCore.Instance.OnLayerVolumeChanged += SetLayerVolume;

            ui_raycaster  = uiCanvas.GetComponent<GraphicRaycaster>();
            click_data    = new PointerEventData(EventSystem.current);
            click_results = new List<RaycastResult>();

            showDebug = PlusMusicCore.Instance.GetDebugMode;
            trackDropdown.onValueChanged.AddListener(SetSoundtrackOnChange);

            sliderMain.value = PlusMusicCore.Instance.GetVolume();
            SetLayerVolume(PlusMusicCore.Instance.GetLayersVolume);

            UpdateIntensity();
            albumCover.sprite = noImage;
            trackText.text    = trackName;
            artistText.text   = artistName;
            arrangementText.text = arrangementName;

            sliderMain.onValueChanged.AddListener(SetMainVolume);
            sliderBass.onValueChanged.AddListener(SetBassVolume);
            sliderDrums.onValueChanged.AddListener(SetDrumsVolume);
            sliderTopMix.onValueChanged.AddListener(SetTopMixVolume);
            sliderVocals.onValueChanged.AddListener(SetVocalsVolume);

            hasStarted = true;
        }

        //----------------------------------------------------------
        // Update is called once per frame
        private void Update()
        {
            if (PlusMusicCore.Instance.GetIsAudioPlaying)
            {
                AudioSource source = PlusMusicCore.Instance.GetCurrentAudioSource();
                if (null != source)
                {
                    string msg = String.Format(
                        "Playing '{0}' for {1:F2}/{2:F2} seconds",
                        arrangementName, source.time, source.clip.length
                    );
                    SetPlayText(msg);
                }
            }
        
            if (doVersion)
            {
                plusMusicVersion.text = String.Format("PlusMusic Version: {0}", PlusMusicCore.Instance.GetPluginVersion);
                doVersion = false;
            }
        }

        //----------------------------------------------------------
        private void OnDestroy()
        {
            PlusMusicCore.Instance.OnLayerVolumeChanged   -= SetLayerVolume;
            PlusMusicCore.Instance.OnTrackLoadingProgress -= SetLoadingProgress;
            PlusMusicCore.Instance.OnRealTimeStatus       -= SetDebugText;
            PlusMusicCore.Instance.OnArrangementChanged   -= SetArrangement;
            PlusMusicCore.Instance.OnProjectInfoLoaded    -= SetProjectInfo;
        }

        //----------------------------------------------------------
        public void SetDebugText(string text)
        {
            if (infoText != null)
            { 
                if (text.Contains("error", StringComparison.OrdinalIgnoreCase))
                    infoText.color = errorColor;
                else
                    infoText.color = debugColor;
                infoText.text = text;
            }
        }

        //----------------------------------------------------------
        public void SetLayerVolume(float[] layerVolume)
        {
            bool useLayers = false;
            if (null != projectInfoData)
                useLayers = projectInfoData.tracks[selectedIdx].canUseLayers;

            if (useLayers)
            {
                sliderMain.value = 1.0f;
                // Index starts at 1 because we want to skipp FullMix here
                sliderBass.value   = layerVolume[1];
                sliderDrums.value  = layerVolume[2];
                sliderTopMix.value = layerVolume[3];
                sliderVocals.value = layerVolume[4];
                layer_intensity = (layerVolume[1] + layerVolume[2] + layerVolume[3] + layerVolume[4]) / 4.0f;
                UpdateIntensity();
            }
            else
            {
                sliderMain.value = PlusMusicCore.Instance.GetVolume();
                sliderBass.value   = 0.0f;
                sliderDrums.value  = 0.0f;
                sliderTopMix.value = 0.0f;
                sliderVocals.value = 0.0f;
                layer_intensity = 0.0f;
                UpdateIntensity();
            }
        }

        //----------------------------------------------------------
        private void UpdateIntensity()
        {
            intensityText.text = String.Format("{0:F2}", layer_intensity);
        }

        //----------------------------------------------------------
        public void SetArrangement(PMTags tag)
        {
            arrangementName = tag.ToString();
            arrangementText.text = arrangementName;

            SetLayerVolume(PlusMusicCore.Instance.GetLayersVolume);
        }

        //----------------------------------------------------------
        // Sets the Arrangement play info text
        public void SetPlayText(string playText)
        {
            playTextField.text = playText;
        }

        //----------------------------------------------------------
        public void SetLoadingProgress(PMTrackProgress progress)
        {
            // Check if we're ready and if this is our track
            if (!hasStarted) return;

            if (0.0f == progress.progress)
            {
                albumCover.sprite = noImage;
                artistText.text = "-";
            }

            loadingBar.value = progress.progress;
            if (progress.progress < 1.0f)
            {
                int percentage = 0;
                if (progress.progress > 0.0f)
                    percentage = (int)(progress.progress * 100.0f);

                hasLoaded = false;
                loadingText.text = String.Format("Loading {0}% ...", percentage);
            }
            else
            {
                loadingText.text = "Done";

                PMTrackInfo info = PlusMusicCore.Instance.GetTrackInfoByIndex(selectedIdx);
                artistName = info.artist;
                artistText.text = artistName;

                if (null != info.image)
                {
                    albumCover.sprite = Sprite.Create(
                        info.image, 
                        new Rect(0, 0, info.image.width, info.image.height),
                        new Vector2(0.5f, 0.5f)
                    );
                }

                hasLoaded = true;
            }
        }

        //----------------------------------------------------------
        public void SetProjectInfo(PMMessageProjectInfo eventProjectInfoData)
        {
            if (showDebug)
                Debug.Log("PM> PlusMusicUI.SetProjectInfo");

            try {

                projectInfoData = eventProjectInfoData;
                trackDropdown.ClearOptions();

                if (null == projectInfoData.tracks || projectInfoData.tracks.Length < 1)
                {
                    Debug.LogWarning("PM> PlusMusicUI.SetProjectInfo(): No tracks!");
                    return;
                }

                int idx = 0;
                selectedIdx = 0;
                List<string> dropOptions = new List<string>();
                foreach (PMMessageTrackList track in eventProjectInfoData.tracks)
                { 
                    dropOptions.Add(track.name);
                    if (track.isSelected)
                        selectedIdx = idx;
                    idx++;
                }
                trackDropdown.AddOptions(dropOptions);
                trackDropdown.value = selectedIdx;

                // Set the track meta data
                arrangementName = "-";
                SetTrackOptions(selectedIdx);

            } catch (Exception e)
            {
                Debug.LogError("PM> ERROR:PlusMusicUI.SetProjectInfo()");
                Debug.LogException(e, this);
            }
        }

        //----------------------------------------------------------
        private void SetTrackOptions(int index)
        {
            if (showDebug)
                Debug.Log($"PM> PlusMusicUI.SetTrackOptions(): index = {index}");

            string trackOption = projectInfoData.tracks[index].name;
            trackName   = "-";
            artistName = "-";

            int dashLocation = trackOption.IndexOf(" - ", StringComparison.Ordinal);
            if (dashLocation > 0)
            {
                trackName  = trackOption.Substring(0, dashLocation);
                artistName = trackOption.Substring(dashLocation + 3);
            }
            else
            {
                trackName = trackOption;
            }

            trackText.text  = trackName;
            artistText.text = artistName;
            arrangementText.text = arrangementName;

            if (projectInfoData.tracks[index].canUseLayers)
            { 
                layerTitle.text = "Layer Mixer";
                sliderBass.interactable   = true;
                sliderDrums.interactable  = true;
                sliderTopMix.interactable = true;
                sliderVocals.interactable = true;
                sliderMain.interactable   = false;
            }
            else
            { 
                layerTitle.text = "Soundtrack does not support layers!";
                sliderBass.interactable   = false;
                sliderDrums.interactable  = false;
                sliderTopMix.interactable = false;
                sliderVocals.interactable = false;
                sliderMain.interactable   = true;
            }
        }

        //----------------------------------------------------------
        public void SetSoundtrackOnChange(Int32 index)
        {
            if (showDebug)
                Debug.Log("PM> PlusMusicUI.SetSoundtrackOnChange(): " + index);

            if (selectedIdx == index) { return; }
            selectedIdx = index;

            // Set the Album meta data
            SetTrackOptions(index);

            PlusMusicCore.Instance.PlayTrack(projectInfoData.tracks[index].id);
        }

        //----------------------------------------------------------
        public void SetMainVolume(float value)
        {
            if (!hasStarted) return;

            PlusMusicCore.Instance.SetLayerVolume(PMAudioLayers.LayerFullMix, value);
        }

        //----------------------------------------------------------
        public void SetMainMute()
        {
            if (!hasStarted) return;
            PlusMusicCore.Instance.MutePlay();
        }

        //----------------------------------------------------------
        public void SetBassVolume(float bassVolume)
        {
            if (!hasStarted) return;
            PlusMusicCore.Instance.SetLayerVolume(PMAudioLayers.LayerBass, bassVolume);
            UpdateIntensity();
        }

        //----------------------------------------------------------
        public void SetDrumsVolume(float drumsVolume)
        {
            if (!hasStarted) return;
            PlusMusicCore.Instance.SetLayerVolume(PMAudioLayers.LayerDrums, drumsVolume);
            UpdateIntensity();
        }

        //----------------------------------------------------------
        public void SetTopMixVolume(float topmixVolume)
        {
            if (!hasStarted) return;
            PlusMusicCore.Instance.SetLayerVolume(PMAudioLayers.LayerTopMix, topmixVolume);
            UpdateIntensity();
        }

        //----------------------------------------------------------
        public void SetVocalsVolume(float vocalsVolume)
        {
            if (!hasStarted) return;
            PlusMusicCore.Instance.SetLayerVolume(PMAudioLayers.LayerVocals, vocalsVolume);
            UpdateIntensity();
        }

    }
}

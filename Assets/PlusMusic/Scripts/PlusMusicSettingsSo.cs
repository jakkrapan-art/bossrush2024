/* ---------------------------------------------------------------------------
Application:    PlusMusic Unity Plugin - Settings
Copyright:      PlusMusic, (c) 2023
Author:         Andy Schmidt
Description:    ScriptableObject to store the plugin settings

TODO:
    Important todo items are marked with a $$$ comment

--------------------------------------------------------------------------- */

using UnityEngine;
using PlusMusicTypes;


namespace PlusMusic
{

    //----------------------------------------------------------
    public class PlusMusicSettingsSo :ScriptableObject
    {
        [TextArea(5, 10)]
        public string developerComments =
            "- 'Project Id' and 'Auth Token'\n" +
            "Your PlusMusic project credentials from the Project Manager web page. " + 
            "Replace the default values with your own project info.\n" +

            "- 'Persists Across Scenes'\n" +
            "Should always be on to ensure proper song caching\n" +

            "- 'Offline Mode'\n" +
            "In offline mode, the plugin only loads data from the local cache. " +
            "You need to prime the cache first, it needs to contain all audio and meta data " +
            "for all songs used in your game.\n" +

            "- 'Auto Load Project'\n" +
            "In most circumstances you'd want this to be enabled. " +
            "If turned off, you have to load the project data manually via script.\n" +

            "\n";

        [SerializeField] private PMSettingsSo _plusMusicSettings;

        [Header("Unity Settings")]
        [Tooltip("Was this downloaded from the Unity Store?")]
        [SerializeField] private bool _isFromUnityStore = false;
        [Tooltip("Did the plugin complete the Unity VSA callback?")]
        [SerializeField] private bool _didVSAttribution = false;
        public TextAsset jsonPackage;
        [SerializeField] private PackageData packageData;

        private string _deviceId = "";
        private float _pluginVolume = 1.0f;

        public PMSettingsSo PlusMusicSettings => _plusMusicSettings;
        public PackageData PackageData => packageData;
        public bool IsFromUnityStore => _isFromUnityStore;
        public bool DidVSAttribution { get => _didVSAttribution; set => _didVSAttribution = value; }
        public string DeviceId { get => _deviceId; set => _deviceId = value; }
        public float PluginVolume { get => _pluginVolume; set => _pluginVolume = value; }


        //----------------------------------------------------------
        public void UpdateSettings(PMSettingsSo settings)
        {
            if (null != settings)
                _plusMusicSettings = settings;
        }

        //----------------------------------------------------------
        public void UpdatePackageData()
        {
            if (null != jsonPackage)
                packageData = JsonUtility.FromJson<PackageData>(jsonPackage.text);
        }

    }
}

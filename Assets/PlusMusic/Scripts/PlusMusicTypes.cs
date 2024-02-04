/* ---------------------------------------------------------------------------
Application:    PlusMusic Unity Plugin - Type Definitions
Copyright:      PlusMusic, (c) 2023
Author:         Andy Schmidt
Description:    Various type definitions

TODO:
    Important todo items are marked with a $$$ comment

--------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;


namespace PlusMusicTypes
{

    //----------------------------------------------------------
    // Basic types, structs, enums, classes
    //----------------------------------------------------------
    #region api_basic_types


    public enum PMWebRequestTypes
    {
        Json = 0,
        Audio,
        Binary
    }

    public enum PMSoundFX
    {
        None = 0,
        DrumRoll1,
        DrumRoll2,
        Swoosh
    }

    public enum PMAudioState
    {
        StateNone = 0,
        StatePlaying,
        StateStopped,
        StatePaused,
        StateUnpaused,
        StateMuted,
        StateUnmuted
    };

    // Event Types for the Event Manager
    public enum PMEventTypes
    {
        EventNone = 0,
        EventGetProjectInfo,
        EventGetDefaultProjectInfo,
        EventGetTrackInfo,
        EventGetTrackAudio,
        EventGetTrackImage,
        EventPlayCurrent
    };

    // Event Status for the Event Manager
    public enum PMEventStatus
    {
        EventWasSuccessful = 0,
        EventHasWarnings,
        EventHasErrors,
        EventAborted
    };

    // Predefined arrangement types
    public enum PMTags
    {
        none = 0,
        high_backing,
        low_backing,
        backing_track,
        preview,
        victory,
        failure,
        highlight,
        lowlight,
        full_song
    };

    public enum PMAudioLayers
    {
        LayerFullMix = 0,
        LayerBass,
        LayerDrums,
        LayerTopMix,
        LayerVocals
    }

    // Predefined audio layer types
    public enum PMAudioLayerTypes
    {
        original = 5000,
        vocals,
        drums,
        bass,
        topmix
    };

    public enum PMTimings
    {
        now = 0,
        nextBeat,
        nextBar
    };

    [System.Serializable]
    public class PMLayerVolumes
    {
        [Range(0.0f, 1.0f)]
        public float bass;
        [Range(0.0f, 1.0f)]
        public float drums;
        [Range(0.0f, 1.0f)]
        public float topMix;
        [Range(0.0f, 1.0f)]
        public float vocals;

        public PMLayerVolumes()
        {
            bass   = 1.0f;
            drums  = 1.0f;
            topMix = 1.0f;
            vocals = 1.0f;
        }

        public float GetByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return bass;
                case 1:
                    return drums;
                case 2:
                    return topMix;
                case 3:
                    return vocals;
            }

            return 0.001f;
        }

        public void SetByIndex(int index, float volume)
        {
            switch (index)
            {
                case 0:
                    bass = volume;
                    break;
                case 1:
                    drums = volume;
                    break;
                case 2:
                    topMix = volume;
                    break;
                case 3:
                    vocals = volume;
                    break;
            }
        }
    }

    // Stem Conversion Helper
    public struct PMStemBuffer
    {
        public int     samples;
        public float[] buffer;
    }

    public class PMEventObject
    {
        public PMEventTypes type = PMEventTypes.EventNone;
        public Action<PMEventTypes, object> func = null;
        public bool dependsOnPrevious = true;
        public PMEventStatus abortThreshold = PMEventStatus.EventHasErrors;
        public PMEventStatus status = PMEventStatus.EventWasSuccessful;
        public object args;
    }

    public class PMTrackProgress
    {
        public Int64 id;
        public int   index;
        public bool  autoPlay;
        public bool  isLoading;
        public int   numToLoad;
        public int   numLoaded;
        public int   numMissed;
        public float progress;

        public PMTrackProgress()
        {
            Reset();
        }

        public void Reset()
        {
            id        = 0;
            index     = 0;
            autoPlay  = false;
            isLoading = false;
            numToLoad = 0;
            numLoaded = 0;
            numMissed = 0;
            progress  = 0.0f;
        }
    }

    public class PMAudioClip
    {
        public Int64 type_id;
        AudioClip clip;
    }

    public class PMPingBackInfo
    {
        public string eventText;
        public Int64  pingProjectId;
        public Int64  pingArrangementId;
        public string pingTag;
        public string pingTransitionType;
        public string pingTransitionTiming;
        public float  pingTransitionDelay;
        public bool   isUsingStinger;
    }

    [System.Serializable]
    public class PMPingBackData
    {
        public string os;
        public string event_text;
        public string device_id;
        public bool   in_editor;
        public string platform;
        public string title;
        public string connected;
        public bool   is_using_stinger;
        public Int64  project_id;
        public Int64  arrangement_id;
        public string arrangement_type;
        public string transition_type;
        public string transition_timing;
        public float  transition_delay;
        public string time;
        public string web_url;
        public string plugin_version;
        public string play_id;
    }

    [System.Serializable]
    public class PMSettings
    {
        public string target;
        public string username;
        public string password;
        public string credentials;
        public string base_url;
        public bool   auto_play;
    }

    [Serializable]
    public class PMSettingsSo
    {
        [Header("Plugin Settings")]
        [Tooltip("Keep PlusMusic loaded across scenes")]
        public bool persistAcrossScenes = true;
        [Tooltip("Log/Display extra debugging information")]
        public bool debugMode = false;
        [Tooltip("In offline mode, the plugin only loads data from the local cache")]
        public bool offlineMode = false;
        [Tooltip("Log the api server requests")]
        public bool logRequestUrls = false;
        [Tooltip("Log the server api responses")]
        public bool logServerResponses = false;
        [Tooltip("Send usage data to PlusMusic")]
        public bool doPingbacks = true;

        [Header("Cache Settings")]
        [Tooltip("Store meta-data and audio using a local disk cache")]
        public bool useLocalCache = true;
        [Tooltip("Refresh meta-data and audio in the local disk cache")]
        public bool refreshLocalCache = false;

        [HideInInspector] public bool loadAudioFromCache = true;
        [HideInInspector] public bool loadMetaFromCache = true;
        [HideInInspector] public bool saveAudioToCache = true;
        [HideInInspector] public bool saveMetaToCache = true;

        //[Tooltip("Include cache in your Unity project. NOTE: This will significantly increase your game size!")]
        // $$$ Not ready yet
        [HideInInspector] public bool bundleCacheWithGame = false;

        [Header("PlusMusic Project Settings")]
        [Tooltip("Unique Project ID from the PlusMusic Project Manager")]
        public Int64 projectId = 0;
        [Tooltip("Unique Authentication Token from the PlusMusic Project Manager")]
        public string authToken = "";
        [Tooltip("Auto Load Project on Start()")]
        public bool autoLoadProject = true;
        [Tooltip("Auto Play the Track below on Start()")]
        public bool autoPlayProject = true;
        [Tooltip("The index of the Track to play")]
        public int autoTrackIndex = 0;

        [Header("Audio Playback Settings")]
        [Tooltip("If true, any current audio will continue playing even after a scene is unloaded")]
        public bool playAcrossScenes = false;
        [Tooltip("Use individual audio layers (Stems)")]
        public bool useAudioLayers = true;
        [Tooltip("The default transition, used if Auto Play Project is enabled")]
        public PMTransitionInfo defaultTransition = new PMTransitionInfo();
    }

    [Serializable]
    public class PackageData
    {
        public string name;
        public string version;
        public string displayName;
        public string description;
        public string unity;
        public string unityRelease;
        public string documentationUrl;
        public string changelogUrl;
        public string licensesUrl;
        public string[] dependencies;
        public string[] keywords;
        public PackageAuthor author;
    }

    [Serializable]
    public class PackageAuthor
    {
        public string name;
        public string email;
        public string url;
    }

    [System.Serializable]
    public class PMDefaultProjectData
    {
        public Int64 id;
        public string plugin_api_key;
    }

    [System.Serializable]
    public class PMDefaultProjectInfo
    {
        public PMDefaultProjectData default_project;
    }

    public struct PMFlags
    {
        public const bool doAutoPlay = true;
    }

    // Supported file extensions
    public class PMExtensions
    {
        public const string AAI  = ".aai";
        public const string OGG  = ".ogg";
        public const string WAV  = ".wav";
        public const string JSON = ".json";
        public const string RAW  = ".raw";
        public const string BIN  = ".bin";
        public const string MP4  = ".mp4";
        public const string PCM  = ".pcm";
        public const string PNG  = ".png";
        public const string BYTES = ".bytes";
    }

    [System.Serializable]
    public class PMTransitionInfo
    {
        [Tooltip("Which Arrangement type to play")]
        public PMTags tag;
        [Tooltip("Duration of the transition")]
        [Min(0.0f)]
        public float duration;
        [Tooltip("When to start the transition")]
        public PMTimings timing;
        [Tooltip("If checked, the main volume below is used as the traget volume.\nIf unchecked, the main volume is unchanged.")]
        public bool useMainVolume;
        [Tooltip("The main volume to transtition to")]
        [Range(0.0f, 1.0f)]
        public float mainVolume;
        [Tooltip("If checked, the layer volumes below are used as the traget volumes.\nIf unchecked, the layer volumes are unchanged.")]
        public bool useLayerVolumes;
        [Tooltip("The layer volumes to transtition to")]
        public PMLayerVolumes layerVolumes;
        [Tooltip("Optional sound effect to play")]
        public PMSoundFX soundFX;
        [Tooltip("Allow the arrangement to transition to itself (Reset/Restart when played)")]
        public bool canTransitionToItself;
        [Tooltip("If enabled, the arrangement will not loop but instead revert back to the previous arrangement after it has finished playing")]
        public bool returnToPrevious;
        [Tooltip("If > 0, the arrangement will stop playing at the cutoff time instead of the length of the audio")]
        [Min(0.0f)]
        public float timeToLive;
        [Tooltip("Which curve to use to transition")]
        public AnimationCurve curve;

        public PMTransitionInfo()
        {
            tag                     = PMTags.backing_track;
            duration                = 1.0f;
            timing                  = PMTimings.now;
            useMainVolume           = true;
            mainVolume              = 1.0f;
            useLayerVolumes         = true;
            layerVolumes            = new PMLayerVolumes();
            soundFX                 = PMSoundFX.None;
            canTransitionToItself   = true;
            returnToPrevious        = false;
            timeToLive              = 0.0f;
            curve                   = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        }

        public PMTransitionInfo(
            PMTags tag, float durationTransition, PMTimings timing,
            bool useMainVolume, float mainVolume, bool useLayerVolumes, PMLayerVolumes layerVolumes,
            PMSoundFX soundFX, bool canTransitionToItself,
            bool returnToPrevious, float timeToLive, AnimationCurve curve
        ) : this()
        {
            this.tag = tag;
            this.duration = durationTransition;
            this.timing = timing;
            this.useMainVolume = useMainVolume;
            this.mainVolume = mainVolume;
            this.useLayerVolumes = useLayerVolumes;
            this.layerVolumes = layerVolumes;
            this.soundFX = soundFX;
            this.canTransitionToItself = canTransitionToItself;
            this.returnToPrevious = returnToPrevious;
            this.timeToLive = timeToLive;
            this.curve = new AnimationCurve(curve.keys);
        }
    }


    #endregion
    //----------------------------------------------------------
    // Data objects
    //----------------------------------------------------------
    #region api_data_objects


    [System.Serializable]
    public class PMProject
    {
        public Int64  id;
        public string name;
        public string authToken;
        public bool   isLoading;
        public bool   isLoaded;
        public int    trackIndex;
        public Int64  trackId;
        public int    arrangementIndex;
        public Int64  arrangementId;
        public AudioSource audio;
        public PMTransitionInfo transition;
        public PMTrackProgress  trackProgress;
        public int           numTracks;
        public List<PMTrack> tracks;

        public PMProject()
        {
            id               = 0;
            name             = "Default";
            authToken        = "";
            isLoading        = false;
            isLoaded         = false;
            trackIndex       = 0;
            trackId          = 0;
            arrangementIndex = -1;
            arrangementId    = 0;
            audio            = null;
            transition       = null;
            trackProgress    = null;
            numTracks        = 0;
            tracks           = null;
        }
    }

    [System.Serializable]
    public class PMTrack
    {
        public Int64  id;
        public Int64  track_id;
        public string name;
        public string artist;
        public string updated_at;
        public string aai_url;
        public string image_url;
        public float  length;
        public bool   canUseLayers;
        public bool   hasFilter;
        public bool   hasAudio;
        public bool   isLoading;
        public bool   isLoaded;
        public float  loadIncrement;
        public float  loadProgress;
        public int    num_arrangements;
        public List<PMArrangement> arrangements;
        public Texture2D   AlbumCover;
        public AudioClip[] layerAudioClips;
    }

    [System.Serializable]
    public class PMArrangement
    {
        public Int64   id;
        public string  name;
        public Int64   type_id;
        public float   length;
        public string  s3_url;
        public float[] beats;
        public float[] bars;
        public int     num_segment_clips;
        public List<PMSegmentClip> segment_clips;
        public AudioClip[] layers;
    }

    [System.Serializable]
    public class PMSegmentClip
    {
        public float start_time;
        public float end_time;
    }


    #endregion
    //----------------------------------------------------------
    // Event data objects
    //----------------------------------------------------------
    #region message_data_objects


    [System.Serializable]
    public class PMTrackInfo
    {
        public Int64     id;
        public int       index;
        public string    name;
        public string    artist;
        public bool      hasLayers;
        public bool      isSelected;
        public Texture2D image;
        public List<PMArrangement> arrangements;
    }

    [System.Serializable]
    public class PMMessageProjectInfo
    {
        public Int64  id;
        public string name;
        public PMMessageTrackList[] tracks;
    }

    [System.Serializable]
    public class PMMessageTrackList
    {
        public Int64  id;
        public string name;
        public bool   hasFilter;
        public bool   isSelected;
        public bool   canUseLayers;
        public bool   isLoaded;
    }


    #endregion
    //----------------------------------------------------------
    // Needed to read json from the web-api
    //----------------------------------------------------------
    #region api_json_objects_beta


    //----------------------------------------------------------
    // Backend API v1.x
    //----------------------------------------------------------
    [System.Serializable]
    public class ParentProjectDataV1
    {
        public Int64         id;
        public string        name;
        public string        updated_at;
        public TrackDataV1[] soundtracks;
    }

    [System.Serializable]
    public class TrackDataV1
    {
        public Int64  id;
        public Int64  song_id;
        public string name;
        public bool   is_licensed;
        public string updated_at;
    }

    [System.Serializable]
    public class TrackMetaV1
    {
        public string name;
        public Int64  genre_id;
        public string image_url;
        public float  length;
    }

    [System.Serializable]
    public class ArtistMetaV1
    {
        public string name;
    }

    [System.Serializable]
    public class ServerArrangementsDataV1
    {
        public Int64           id;
        public Int64           song_id;
        public TrackMetaV1     song;
        public ArtistMetaV1    artist;
        public string          aai_url;
        public TrackClipV1[]   song_clips;
        public ArrangementV1[] arrangements;
    }

    [System.Serializable]
    public class TrackClipV1
    {
        public Int64  id;
        public string title;
        public float  start_time;
        public float  end_time;
        public float  intensity;
    }

    [System.Serializable]
    public class ArrangementV1
    {
        public Int64   id;
        public string  name;
        public Int64   type_id;
        public float   length;
        public bool    is_instrumental;
        public bool    is_loopable;
        public string  updated_at;
        public float[] beats;
        public float[] bars;
        public Int64[] song_clips;
    }


    #endregion
    //----------------------------------------------------------
    // AAI related types
    //----------------------------------------------------------
    #region aai_types


    // AAI TOC Entry
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PMTocEntry
    {
        public UInt32 signature;       // Signature of the chunk
        public UInt16 type;            // Type of the chunk
        public UInt16 unused;          // Currently not in use, padding so an entry fits into two 64-bit integer
        public UInt64 offset;          // File offset of the data chunk
    }

    // AAI Audio formats
    public class PMAudioFormat
    {
        public const UInt16 none = 0;
        public const UInt16 raw_float = 1;  // Uncompressed 32-bit float, no header
        public const UInt16 pcm_16 = 2;     // Uncompressed 16-bit PCM, no header
        public const UInt16 ogg = 3;        // OGG file, with header
        public const UInt16 wav = 4;        // WAV file, with header
        public const UInt16 mp4 = 5;        // MP4 file, with header
    }

    // AAI Audio types
    public class PMAudioTypes
    {
        public const UInt16 none = 0;
        public const UInt16 high_backing = 1;       // high_backing arrangement
        public const UInt16 low_backing = 2;        // low_backing arrangement
        public const UInt16 backing_track = 3;      // backing_track arrangement
        public const UInt16 preview = 4;            // preview arrangement
        public const UInt16 victory = 5;            // victory arrangement
        public const UInt16 failure = 6;            // failure arrangement
        public const UInt16 highlight = 7;          // highlight arrangement
        public const UInt16 lowlight = 8;           // lowlight arrangement
        public const UInt16 full_song = 9;          // full_song arrangement
        public const UInt16 original = 5000;        // original song audio
        public const UInt16 stem_vocals = 5001;     // vocal layer/stem
        public const UInt16 stem_drums = 5002;      // drum layer/stem
        public const UInt16 stem_bass = 5003;       // bass layer/stem
        public const UInt16 stem_other = 5004;      // other layer/stem
        public const UInt16 instrumental = 5005;    // instrumental version of the full_song
    }

    // AAI Json types
    public class PMJsonTypes
    {
        public const UInt16 none = 0;
        public const UInt16 license = 1;        // License meta data
        public const UInt16 project = 2;        // Project meta data
        public const UInt16 track = 3;          // Track meta data
        public const UInt16 arrangement = 4;    // Arrangement meta data
        public const UInt16 stem = 5;           // Stem meta data
        public const UInt16 url = 6;            // Url meta data
    }

    // AAI Image formats
    public class PMImageTypes
    {
        public const UInt16 none = 0;
        public const UInt16 raw_rgb = 1;    // Raw RGB
        public const UInt16 raw_rgba = 2;   // Raw RGBA
        public const UInt16 jpg = 3;        // JPEG
        public const UInt16 png = 4;        // PNG
    }

    // AAI Raw formats
    public class PMRawTypes
    {
        public const UInt16 none = 0;
        public const UInt16 album = 1;  // Album cover art
        public const UInt16 filter = 2; // Audio filter
    }


    #endregion
    //----------------------------------------------------------
    // Headers for supported audio file types
    //----------------------------------------------------------
    #region file_headers


    public class PMFileHeaders
    {
        // PlusMusic AAI (Adaptive Artificial Intelligence)

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AAI_CHUNK_HEADER
        {
            public UInt32 signature;
            public UInt16 size;

            public AAI_CHUNK_HEADER(bool dummy = true)
            {
                signature = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("CHNK"), 0);
                size      = (UInt16)Marshal.SizeOf<AAI_CHUNK_HEADER>();
            }

            public string GetSigString()
            {
                byte[] sig_bytes = new byte[4];
                sig_bytes[0] = (byte)((signature >>  0) & 0xFF);
                sig_bytes[1] = (byte)((signature >>  8) & 0xFF);
                sig_bytes[2] = (byte)((signature >> 16) & 0xFF);
                sig_bytes[3] = (byte)((signature >> 24) & 0xFF);
                return Encoding.ASCII.GetString(sig_bytes, 0, 4);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AAI
        {
            public UInt32 signature;    // 'AAI '
            public UInt16 size;         // Size (in bytes) of this header, including the signature
            public UInt16 version;      // AAI Version
            public UInt32 chunks;       // Number of chunks
            public UInt64 toc;          // Offset to TOC chunk
            public UInt64 data;         // Size of all chunks
            // DATA chunks

            public AAI(bool dummy = true)
            {
                signature = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("AAI "), 0);
                size      = (UInt16)Marshal.SizeOf<AAI>();
                version   = 1;
                chunks    = 0;
                toc       = 0;
                data      = 0;
            }

            public string GetSigString()
            {
                byte[] sig_bytes = new byte[4];
                sig_bytes[0] = (byte)((signature >>  0) & 0xFF);
                sig_bytes[1] = (byte)((signature >>  8) & 0xFF);
                sig_bytes[2] = (byte)((signature >> 16) & 0xFF);
                sig_bytes[3] = (byte)((signature >> 24) & 0xFF);
                return Encoding.ASCII.GetString(sig_bytes, 0, 4);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AAI_TOC
        {
            public AAI_TOC(bool dummy = true)
            {
                signature = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("TOC "), 0);
                size  = (UInt16)Marshal.SizeOf<AAI_TOC>();
                entries = 0;
                data  = 0;
            }

            public UInt32 signature;    // 'TOC '
            public UInt16 size;         // Size (in bytes) of this header, including the signature
            public UInt32 entries;      // Number of entries in the TOC
            public UInt64 data;         // Size of the data
            // DATA
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AAI_AUDIO
        {
            public UInt32 signature;    // 'LOUD'
            public UInt16 size;         // Size (in bytes) of this header, including the signature
            public UInt16 type;         // Type of audio object
            public UInt16 format;       // Audio format
            public UInt16 blocksize;    // Block size
            public UInt16 channels;     // Number of channels
            public UInt32 frequency;    // Sample frequency
            public UInt64 id;           // Optional (unique) ID
            public UInt64 data;         // Size of the data
            // DATA

            public AAI_AUDIO(bool dummy = true)
            {
                signature = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("LOUD"), 0);
                size      = (UInt16)Marshal.SizeOf<AAI_AUDIO>();
                type      = 0;
                format    = PMAudioFormat.raw_float;
                blocksize = 4;
                channels  = 2;
                frequency = 44100;
                id        = 0;
                data      = 0;
            }

            public string GetSigString()
            {
                byte[] sig_bytes = new byte[4];
                sig_bytes[0] = (byte)((signature >>  0) & 0xFF);
                sig_bytes[1] = (byte)((signature >>  8) & 0xFF);
                sig_bytes[2] = (byte)((signature >> 16) & 0xFF);
                sig_bytes[3] = (byte)((signature >> 24) & 0xFF);
                return Encoding.ASCII.GetString(sig_bytes, 0, 4);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class AAI_JSON
        {
            public AAI_JSON(bool dummy = true)
            {
                signature = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("JSON"), 0);
                size      = (UInt16)Marshal.SizeOf<AAI_JSON>();
                type      = 0;
                id        = 0;
                data      = 0;
            }

            public UInt32 signature;    // 'JSON'
            public UInt16 size;         // Size (in bytes) of this header, including the signature
            public UInt16 type;         // Type of json object
            public UInt64 id;           // Optional (unique) ID
            public UInt64 data;         // Size of the data
            // DATA
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class AAI_IMAGE
        {
            public AAI_IMAGE(bool dummy = true)
            {
                signature = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("IMAG"), 0);
                size      = (UInt16)Marshal.SizeOf<AAI_IMAGE>();
                type      = 0;
                id        = 0;
                data      = 0;
            }

            public UInt32 signature;    // 'IMAG'
            public UInt16 size;         // Size (in bytes) of this header, including the signature
            public UInt16 type;         // Type of image object
            public UInt16 format;       // Image format
            public UInt64 id;           // Optional (unique) ID
            public UInt64 data;         // Size of the data
            // DATA
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class AAI_RAW
        {
            public UInt32 signature;    // 'RAW '
            public UInt16 size;         // Size (in bytes) of this header, including the signature
            public UInt16 type;         // Type of raw object
            public UInt16 format;       // Raw format
            public UInt64 id;           // Optional (unique) ID
            public UInt64 data;         // Size of the data
            // DATA

            public AAI_RAW(bool dummy = true)
            {
                signature = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("RAW "), 0);
                size      = (UInt16)Marshal.SizeOf<AAI_RAW>();
                type      = 0;
                id        = 0;
                data      = 0;
            }

            public string GetSigString()
            {
                byte[] sig_bytes = new byte[4];
                sig_bytes[0] = (byte)((signature >>  0) & 0xFF);
                sig_bytes[1] = (byte)((signature >>  8) & 0xFF);
                sig_bytes[2] = (byte)((signature >> 16) & 0xFF);
                sig_bytes[3] = (byte)((signature >> 24) & 0xFF);
                return Encoding.ASCII.GetString(sig_bytes, 0, 4);
            }
        }

        // WAV (Waveform Audio File Format)
        public class WAV
        {
            public UInt32 signature = 0x46464952;       // 'RIFF'
            public Int32 size = 0;                      // Size of file - 8
            public UInt32 content = 0x45564157;         // 'WAVE'
            public UInt32 formatChunk = 0x20746d66;     // chunk 'fmt '
            public Int32 formatLength = 16;             // chunk length (in bytes) following this field
            public Int16 type = 1;                      // 1 = Uncompressed PCM, 2 Bytes per sample
            public Int16 channels = 2;                  // Number of channels
            public Int32 frequency = 44100;             // Sample frequency
            public Int32 bytesPerSecond = 0;            // Bytes per second
            public Int16 blockSize = 4;                 // Block size
            public Int16 bitsPerSample = 16;            // Bits per Sample
            public UInt32 dataChunk = 0x61746164;       // chunk 'data'
            public Int32 dataLength = 0;                // chunk length (in bytes) following this field
            // DATA
            // Number of data bytes = (channels * samples * type_size)

            // Custom enumerator that can be used with a BinaryWriter
            public IEnumerator<byte[]> GetEnumerator()
            {
                yield return BitConverter.GetBytes(signature);
                yield return BitConverter.GetBytes(size);
                yield return BitConverter.GetBytes(content);
                yield return BitConverter.GetBytes(formatChunk);
                yield return BitConverter.GetBytes(formatLength);
                yield return BitConverter.GetBytes(type);
                yield return BitConverter.GetBytes(channels);
                yield return BitConverter.GetBytes(frequency);
                yield return BitConverter.GetBytes(bytesPerSecond);
                yield return BitConverter.GetBytes(blockSize);
                yield return BitConverter.GetBytes(bitsPerSample);
                yield return BitConverter.GetBytes(dataChunk);
                yield return BitConverter.GetBytes(dataLength);
            }
        }

    }


    #endregion
}

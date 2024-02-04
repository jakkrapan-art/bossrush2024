
using UnityEngine;
using PlusMusic;
using PlusMusicTypes;


namespace PlusMusic
{
    public class PMTriggerLayerVolume: MonoBehaviour
    {

        [Tooltip("The layer volumes to transtition to")]
        public PMLayerVolumes layerVolumes;
        public bool switchMaterial = false;
        public Material matEnterTrigger;
        public Material matExitTrigger;

        private MeshRenderer triggerMeshRenderer = null;


        void Start()
        {
            if (null == PlusMusicCore.Instance)
            {
                Debug.LogError("PM> ERROR:PMTriggerPlayArrangement.Start(): There is no PlusMusicCore in the scene!");
                return;
            }

            if (switchMaterial)
            { 
                triggerMeshRenderer = GetComponent<MeshRenderer>();
                if (null == triggerMeshRenderer)
                    Debug.LogWarning("PM> PMTriggerLayerVolume.Start(): triggerMeshRenderer is null!");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (switchMaterial && null != triggerMeshRenderer && null != matEnterTrigger)
            { 
                Material[] mats = triggerMeshRenderer.materials;
                mats[0] = matEnterTrigger;
                triggerMeshRenderer.materials = mats;
            }

            PlusMusicCore.Instance.SetLayerVolume(PMAudioLayers.LayerBass, layerVolumes.bass);
            PlusMusicCore.Instance.SetLayerVolume(PMAudioLayers.LayerDrums, layerVolumes.drums);
            PlusMusicCore.Instance.SetLayerVolume(PMAudioLayers.LayerTopMix, layerVolumes.topMix);
            PlusMusicCore.Instance.SetLayerVolume(PMAudioLayers.LayerVocals, layerVolumes.vocals);
        }

        private void OnTriggerExit(Collider other)
        {
            if (switchMaterial && null != triggerMeshRenderer && null != matExitTrigger)
            {
                Material[] mats = triggerMeshRenderer.materials;
                mats[0] = matExitTrigger;
                triggerMeshRenderer.materials = mats;
            }
        }

    }
}

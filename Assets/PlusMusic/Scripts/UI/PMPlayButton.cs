
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PlusMusic;
using PlusMusicTypes;


namespace PlusMusic
{
    public class PMPlayButton: MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public Image playButton;

        private bool isPlaying = true;
        private Color32 white = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        private Color32 gray  = new Color(0.5f, 0.5f, 0.5f, 1.0f);


        // Start is called before the first frame update
        void Start()
        {
            if (null == playButton)
            {
                Debug.LogWarning("PlayButton.Start(): playButton is null!");
                return;
            }

            SetState();
            PlusMusicCore.Instance.OnAudioStateChanged += StateChanged;
        }

        private void OnDestroy()
        {
            PlusMusicCore.Instance.OnAudioStateChanged -= StateChanged;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        // Do this when the mouse click on this selectable UI object is released
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPlaying)
            { 
                isPlaying = true;
                PlusMusicCore.Instance.StartPlay();
            }
        }

        private void SetState()
        {
            if (isPlaying)
                playButton.color = gray;
            else
                playButton.color = white;
        }

        public void StateChanged(PMAudioState state)
        {
            //Debug.LogFormat("PlayButton.StateChanged(): state = {0}", state);

            // 1 = Playing, 2 = Stopped, 3 = Paused, 4 = Unpaused, 5 = Muted, 6 = Unmuted
            if ((int)state < 5)
            { 
                if (PMAudioState.StatePlaying == state)
                    isPlaying = true;
                else
                    isPlaying = false;

                SetState();
            }
        }

    }
}

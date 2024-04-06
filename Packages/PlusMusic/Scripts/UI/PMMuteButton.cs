
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PlusMusic;
using PlusMusicTypes;


namespace PlusMusic
{
    public class PMMuteButton: MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public Image muteButton;

        private bool isMuted = false;
        private Color32 white = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        private Color32 gray  = new Color(0.5f, 0.5f, 0.5f, 1.0f);

        // Start is called before the first frame update
        void Start()
        {
            if (null == muteButton)
            {
                Debug.LogWarning("MuteButton.Start(): muteButton is null!");
                return;
            }

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
            if (!isMuted)
            {
                isMuted = true;
                PlusMusicCore.Instance.MutePlay();
            }
        }

        private void SetState()
        {
            if (isMuted)
                muteButton.color = gray;
            else
                muteButton.color = white;
        }

        public void StateChanged(PMAudioState state)
        {
            //Debug.LogFormat("MuteButton.StateChanged(): state = {0}", state);

            // 1 = Playing, 2 = Stopped, 3 = Paused, 4 = Unpaused, 5 = Muted, 6 = Unmuted
            if ((int)state > 4)
            { 
                if (PMAudioState.StateMuted == state)
                    isMuted = true;
                else
                    isMuted = false;

                SetState();
            }
        }

    }
}

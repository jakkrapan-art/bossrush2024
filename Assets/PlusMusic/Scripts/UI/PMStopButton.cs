
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PlusMusic;
using PlusMusicTypes;


namespace PlusMusic
{
    public class PMStopButton: MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public Image stopButton;

        private bool isStopped = false;
        private Color32 white = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        private Color32 gray  = new Color(0.5f, 0.5f, 0.5f, 1.0f);


        // Start is called before the first frame update
        void Start()
        {
            if (null == stopButton)
            {
                Debug.LogWarning("StopButton.Start(): stopButton is null!");
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
            if (!isStopped)
            {
                isStopped = true;
                PlusMusicCore.Instance.StopPlay();
            }
        }

        private void SetState()
        {
            if (isStopped)
                stopButton.color = gray;
            else
                stopButton.color = white;
        }

        public void StateChanged(PMAudioState state)
        {
            //Debug.LogFormat("StopButton.StateChanged(): state = {0}", state);

            // 1 = Playing, 2 = Stopped, 3 = Paused, 4 = Unpaused, 5 = Muted, 6 = Unmuted
            if ((int)state < 5)
            {
                if (PMAudioState.StateStopped == state)
                    isStopped = true;
                else
                    isStopped = false;

                SetState();
            }
        }

    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PlusMusic;


namespace PlusMusic
{
    public class PMResizeButton: MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public GameObject menuPanel;
        public GameObject contentPanel;
        public int smallSize = 24;
        public int largeSize = 150;
        public bool startSmall = true;

        private bool isSmall = true;


        // Start is called before the first frame update
        void Start()
        {
            if (null == menuPanel)
            {
                Debug.LogWarning("ResizeButton.Start(): menuPanel is null!");
                return;
            }
            if (null == contentPanel)
            {
                Debug.LogWarning("ResizeButton.Start(): contentPanel is null!");
                return;
            }

            isSmall = startSmall;
            SetSize();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        // Do this when the mouse click on this selectable UI object is released
        public void OnPointerUp(PointerEventData eventData)
        {
            ToggleSize();
        }

        private void SetSize()
        {
            RectTransform rt = menuPanel.GetComponent<RectTransform>();

            if (isSmall)
            {
                contentPanel.SetActive(false);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, smallSize);
            }
            else
            {
                contentPanel.SetActive(true);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, largeSize);
            }
        }

        private void ToggleSize()
        {
            isSmall = !isSmall;
            SetSize();
        }

    }
}

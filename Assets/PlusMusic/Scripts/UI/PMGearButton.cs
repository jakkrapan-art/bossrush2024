
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PlusMusic;


namespace PlusMusic
{
    public class PMGearButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public GameObject menuPanel;
        public bool startOpen = false;

        private bool isOpen = false;


        // Start is called before the first frame update
        void Start()
        {
            if (null == menuPanel)
            {
                Debug.LogWarning("GearButton.Start(): menuPanel is null!");
                return;
            }

            if (startOpen)
                isOpen = true;
            else
                isOpen = false;
            menuPanel.SetActive(isOpen);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        // Do this when the mouse click on this selectable UI object is released
        public void OnPointerUp(PointerEventData eventData)
        {
            isOpen = !isOpen;   // Toggle menu panel
            menuPanel.SetActive(isOpen);
        }

    }
}

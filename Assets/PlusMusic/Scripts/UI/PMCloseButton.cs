
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PlusMusic;


namespace PlusMusic
{
    public class PMCloseButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public GameObject menuPanel;

        // Start is called before the first frame update
        void Start()
        {
            if (null == menuPanel)
            {
                Debug.LogWarning("CloseButton.Start(): menuPanel is null!");
                return;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        // Do this when the mouse click on this selectable UI object is released
        public void OnPointerUp(PointerEventData eventData)
        {
            menuPanel.SetActive(false); // Close menu panel
        }

    }
}

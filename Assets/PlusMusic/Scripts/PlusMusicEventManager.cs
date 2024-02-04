/* ---------------------------------------------------------------------------
Application:    PlusMusic Unity Plugin - Event Manager
Copyright:      PlusMusic, (c) 2023
Author:         Andy Schmidt
Description:    Manages a list of events

TODO:
    Important todo items are marked with a $$$ comment

--------------------------------------------------------------------------- */

using System.Collections.Generic;
using UnityEngine;
using PlusMusicTypes;


namespace PlusMusic
{

    //----------------------------------------------------------
    class PlusMusicEventManager:MonoBehaviour
    {
        private List<PMEventObject> pm_events;
        private int currentEventIndex;
        private PMEventStatus lastEventStatus;
        private bool processNextEvent;
        private bool showDebug = false;

        // Static instance of this Class
        public static PlusMusicEventManager Instance;
        [HideInInspector]
        public bool isProcessing = false;


        //----------------------------------------------------------
        // Private Functions
        //----------------------------------------------------------
        #region private_functions


        //----------------------------------------------------------
        private void Awake()
        {
            Instance = this;
            isProcessing = false;
            lastEventStatus = PMEventStatus.EventWasSuccessful;
            currentEventIndex = -1;
            pm_events = new List<PMEventObject>();
            pm_events.Clear();
        }

        //----------------------------------------------------------
        private void Start() {

            showDebug = PlusMusicCore.Instance.GetDebugMode;
        }

        //----------------------------------------------------------
        private void Update()
        {
            if ((processNextEvent) && (isProcessing))
            {
                processNextEvent = false;
                ProcessNextEvent();
            }
        }

        //----------------------------------------------------------
        // Process the next event in the queue
        //----------------------------------------------------------
        private void ProcessNextEvent()
        {
            // If we aren't processing, return and do nothing
            if (!isProcessing) { return; };

            // Remove previous event from the list
            if ((0 == currentEventIndex) && (pm_events.Count > 0))
            {
                lastEventStatus = pm_events[currentEventIndex].status;
                pm_events.RemoveAt(currentEventIndex);
            }

            // Process next event
            if (pm_events.Count > 0)
            {
                // Get the event
                currentEventIndex = 0;
                PMEventObject pm_event = pm_events[currentEventIndex];

                // Check previous status and abort this event if needed
                if (pm_event.dependsOnPrevious)
                {
                    if (lastEventStatus >= pm_event.abortThreshold)
                    {
                        Debug.LogWarning("PM> Event.ProcessNextEvent(): Aborting due to previous errors ...");

                        // Copy error status to the current event and continue processing
                        // which will remove it from the list without executing it
                        pm_event.status = lastEventStatus;
                        ContinueProcessing();
                        return;
                    }
                }

                if (showDebug)
                    Debug.LogFormat("PM> Event.ProcessNextEvent(): type, dependsOnPrevious, abortThreshold = {0}, {1}, {2}",
                        pm_event.type, pm_event.dependsOnPrevious, pm_event.abortThreshold);

                // Call the event function
                if (null != pm_event.func)
                    pm_event.func?.Invoke(pm_event.type, pm_event.args);
                else
                    Debug.LogError("PM> ERROR:Event.ProcessNextEvent(): Func is null!");
            }
            else
            {
                StopAndReset();

                if (showDebug)
                    Debug.Log("PM> Event.ProcessNextEvent(): No more events to process ...");
            }
        }


        #endregion
        //----------------------------------------------------------
        // Public Functions
        //----------------------------------------------------------
        #region public_functions


        //----------------------------------------------------------
        // Start processing the event queue
        //----------------------------------------------------------
        public void StartProcessing()
        {
            isProcessing = true;
            processNextEvent = true;
        }

        //----------------------------------------------------------
        // Cancel processing the current event
        //----------------------------------------------------------
        public void CancelCurrentEvent()
        {
            // If we aren't processing, return and do nothing
            if (!isProcessing) { return; };

            // Remove this event from the list
            if ((0 == currentEventIndex) && (pm_events.Count > 0))
            {
                lastEventStatus = pm_events[currentEventIndex].status;
                pm_events.RemoveAt(currentEventIndex);
            }
        }

        //----------------------------------------------------------
        // Skip processing of the current event
        //----------------------------------------------------------
        public void SkipEvent()
        {
            CancelCurrentEvent();
        }

        //----------------------------------------------------------
        // Continue processing the event queue
        //----------------------------------------------------------
        public void ContinueProcessing()
        {
            processNextEvent = true;
        }

        //----------------------------------------------------------
        // Add an event to the queue
        //----------------------------------------------------------
        public void AddEvent(PMEventObject newEvent)
        {
            if (null != newEvent)
                pm_events.Add(newEvent);
            else
                Debug.LogError("PM> ERROR:Event.AddEvent(): Event is null!");
        }

        //----------------------------------------------------------
        // Set the status of the current event object
        //----------------------------------------------------------
        public void SetStatus(PMEventStatus newStatus)
        {
            if (-1 != currentEventIndex && pm_events.Count > 0)
                pm_events[currentEventIndex].status = newStatus;
        }

        //----------------------------------------------------------
        // Get the status of the current event object
        // 
        // If the event queue is empty, we return lastEventStatus
        //----------------------------------------------------------
        public PMEventStatus GetStatus()
        {
            if (-1 != currentEventIndex && pm_events.Count > 0)
                return pm_events[currentEventIndex].status;
            else
                return lastEventStatus;
        }

        //----------------------------------------------------------
        // Reset the manager to it's initial state
        // NOTE: If the manager is currently still processing,
        // all current events will be lost!
        //----------------------------------------------------------
        public void StopAndReset()
        {
            isProcessing = false;
            pm_events.Clear();
            currentEventIndex = -1;
            lastEventStatus = PMEventStatus.EventWasSuccessful;
        }

        #endregion
    }
}

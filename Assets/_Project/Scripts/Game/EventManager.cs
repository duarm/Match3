using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    // General communication without direct dependecy
    public class EventManager : MonoBehaviour
    {
        static EventManager Instance;

        //TODO: <int,Action> hash
        Dictionary<string, Action> eventDictionary = new Dictionary<string, Action> ();

        void Awake ()
        {
            if (Instance != null)
                Destroy (this);

            Instance = this;
        }

        public static void StartListening (string eventName, Action listener)
        {
            if (listener == null)
                Debug.LogError ("You can't pass null as a listener to the Event Manager (StartListening)");

            if (Instance.eventDictionary.TryGetValue (eventName, out Action thisEvent))
            {
                thisEvent += listener;
            }
            else
            {
                thisEvent += listener;
                Instance.eventDictionary.Add (eventName, thisEvent);
            }
        }

        public static void StopListening (string eventName, Action listener)
        {
            if (listener == null)
                Debug.LogError ("You can't pass null as a listener to the Event Manager (StopListening)");

            if (Instance.eventDictionary.TryGetValue (eventName, out Action thisEvent))
            {
                thisEvent -= listener;
            }
        }

        public static void TriggerEvent (string eventName)
        {
            if (Instance.eventDictionary.TryGetValue (eventName, out Action thisEvent))
            {
                thisEvent?.Invoke ();
            }
        }
    }
}
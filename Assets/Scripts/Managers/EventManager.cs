using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoSingleton<EventManager>
{
    private Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

    public void Subscribe<T>(string eventName, Action<T> listener)
    {
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary.Add(eventName, null);
        }

        eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], listener);
    }

    public void Subscribe(string eventName, Action listener)
    {
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary.Add(eventName, null);
        }

        eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], listener);
    }

    public void Unsubscribe<T>(string eventName, Action<T> listener)
    {
        if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
        {
            eventDictionary[eventName] = Delegate.Remove(thisEvent, listener);

            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    public void Unsubscribe(string eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
        {
            eventDictionary[eventName] = Delegate.Remove(thisEvent, listener);

            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    public void TriggerEvent<T>(string eventName, T arg)
    {
        if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
        {
            if (thisEvent is Action<T> action)
            {
                action.Invoke(arg);
            }
            else
            {
                Debug.LogWarning($"Event {eventName} has wrong parameter type");
            }
        }
    }

    public void TriggerEvent(string eventName)
    {
        if (eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
        {
            if (thisEvent is Action action)
            {
                action.Invoke();
            }
            else
            {
                Debug.LogWarning($"Event {eventName} has wrong parameter type");
            }
        }
    }

    public void ClearAllEvents()
    {
        eventDictionary.Clear();
    }
}

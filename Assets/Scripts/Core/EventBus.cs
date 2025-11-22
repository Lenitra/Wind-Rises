using System;
using System.Collections.Generic;
using UnityEngine;

namespace WindRises.Core
{
    public class EventBus : MonoBehaviour
    {
        private static EventBus _instance;
        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("EventBus");
                    _instance = go.AddComponent<EventBus>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();

        public void Subscribe<T>(Action<T> listener) where T : struct
        {
            Type type = typeof(T);
            if (_events.ContainsKey(type))
                _events[type] = Delegate.Combine(_events[type], listener);
            else
                _events[type] = listener;
        }

        public void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            Type type = typeof(T);
            if (_events.ContainsKey(type))
            {
                _events[type] = Delegate.Remove(_events[type], listener);
                if (_events[type] == null)
                    _events.Remove(type);
            }
        }

        public void Publish<T>(T data) where T : struct
        {
            if (_events.TryGetValue(typeof(T), out Delegate d))
                (d as Action<T>)?.Invoke(data);
        }
    }
}

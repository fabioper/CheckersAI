using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventsStore : MonoBehaviour
{
    private List<GameEvent> _events; 

    public static EventsStore Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _events = new List<GameEvent>();
    }

    public void NotifyEvent(GameEventType type)
    {
        foreach (var gameEvent in _events.Where(ev => ev.Type == type))
            gameEvent.Execute();
    }

    public void OnEvent(GameEventType eventType, Action action)
    {
        _events.Add(new GameEvent(action, eventType));
    }
}
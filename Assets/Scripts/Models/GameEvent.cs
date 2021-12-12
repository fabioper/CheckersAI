using System;

public class GameEvent
{
    public GameEvent(Action execute, GameEventType type)
    {
        Execute = execute;
        Type = type;
    }

    public GameEventType Type { get; set; }
    public Action Execute { get; set; }
}
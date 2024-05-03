using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StateCanvas
{
    public GameState State;
    public Canvas Canvas;
}

public class UIController : MonoBehaviour
{
    public StateCanvas[] CanvasArray;

    private void Start()
    {
        StateChange(GameState.Idle);
        GameController.OnStateChange += StateChange;
    }

    public void StateChange(GameState state)
    {
        foreach(var current in CanvasArray) 
        {
            if (current.State == state) current.Canvas.enabled = true;
            else current.Canvas.enabled = false;
        }
    }
}

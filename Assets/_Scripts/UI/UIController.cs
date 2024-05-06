using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct StateCanvas
{
    public GameState State;
    public Canvas Canvas;
}

[System.Serializable]
public class PlanetsToCanvas
{
    public GameObject Planet;
    public PlanetsScriptable DataPlanet;
}

public class UIController : MonoBehaviour
{
    public StateCanvas[] CanvasArray;
    public PlanetsToCanvas[] PlanetsArray;

    [SerializeField] private TextMeshProUGUI _tmpText;
    private PlanetsToCanvas _currentSelected = null;

    private void Awake()
    {
        GameController.OnStateChange += StateChange;
        ARSelectable.OnSendData += CheckSelected;
    }
    private void OnDestroy()
    {
        GameController.OnStateChange -= StateChange;
        ARSelectable.OnSendData -= CheckSelected;
    }

    private void CheckSelected(PlanetsScriptable selected)
    {
        foreach(var planet in PlanetsArray)
        {
            if(planet.DataPlanet == selected)
            {
                planet.Planet.SetActive(true);
                _tmpText.text = selected.Info;
                _currentSelected = planet;
                return;
            }
        }
    }

    private void Deselected()
    {
        if(_currentSelected != null)
        {
            _currentSelected.Planet.SetActive(false);
            _tmpText.text = "";
            _currentSelected = null;
        }
    }

    public void StateChange(GameState state)
    {
        if(state != GameState.Selecting) Deselected();
        foreach (var current in CanvasArray) 
        {
            if (current.State == state) current.Canvas.enabled = true;
            else current.Canvas.enabled = false;
        }
        
    }
}

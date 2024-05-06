using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ARSelectable : MonoBehaviour
{
    public static Action<PlanetsScriptable> OnSendData;
    [SerializeField] private PlanetsScriptable _dataPlanet;

    public void Selected(ARSelectable selected)
    {
        if (selected != this) return;
        // Feedback and data
        OnSendData?.Invoke(_dataPlanet);
    }

    private void OnEnable()
    {
        GameController.OnSelected += Selected;
    }

    private void OnDisable()
    {
        GameController.OnSelected -= Selected;
    }

    private void OnValidate()
    {
        if(_dataPlanet == null && TryGetComponent<RotationPeriod>(out var planet))
        {
            _dataPlanet = planet.GetData;
        }
    }
}

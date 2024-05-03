using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ARSelectable : MonoBehaviour
{
    [SerializeField] private PlanetsScriptable _dataPlanet;

    public void Selected(ARSelectable selected)
    {
        if (selected != this) return;
        
        // Feedback and data
    }

    private void OnValidate()
    {
        if(_dataPlanet == null && TryGetComponent<RotationPeriod>(out var planet))
        {
            _dataPlanet = planet.GetData;
        }
    }
}

using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public static Action<bool> OnPause;
    public static Action OnStartSimulation;

    [SerializeField]
    private GameObject _solarSystem;

    private void Start()
    {
        PinchDetection.OnScale += ScaleSolarSystem;
    }

    private void OnDestroy()
    {
        PinchDetection.OnScale -= ScaleSolarSystem;
    }

    private void ScaleSolarSystem(float value)
    {
        if(_solarSystem.activeInHierarchy)
            _solarSystem.transform.localScale += Vector3.one * value;
    }

#if UNITY_EDITOR
    [Header("TEST")]
    public bool Activate = false;
    public bool Pause = false;
    private bool currentStatePause = false;
    private void OnValidate()
    {
        if(Activate) OnStartSimulation.Invoke();
        if (currentStatePause != Pause)
        {
            currentStatePause = Pause;
            OnPause.Invoke(Pause);
        }
    }
#endif
}

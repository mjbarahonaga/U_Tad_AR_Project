using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public static Action<bool> OnPause;
    public static Action OnStartSimulation;
    public static Action OnCheckData;

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
        {
            _solarSystem.transform.localScale += Vector3.one * value;
            OnCheckData?.Invoke();
        }
            
    }

#if UNITY_EDITOR
    [Header("TEST")]
    public bool Activate = false;
    public bool Pause = false;
    public float ScaleFactor = 1.0f;
    public bool IncreaseSize = false;
    public bool ReduceSize = false;
    private bool currentStatePause = false;

    private void OnValidate()
    {
        if(Activate) OnStartSimulation.Invoke();
        if (currentStatePause != Pause)
        {
            currentStatePause = Pause;
            OnPause.Invoke(Pause);
        }

        if(IncreaseSize)
        {
            ScaleSolarSystem(ScaleFactor);
            IncreaseSize = false;
        }
        if(ReduceSize)
        {
            ScaleSolarSystem(-ScaleFactor);
            ReduceSize = false;
        }
    }
#endif
}

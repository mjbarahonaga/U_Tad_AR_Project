using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static Action<bool> OnPause;
    public static Action OnStartSimulation;


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
        //if (currentStatePause != Pause && Pause == true)
        //{
        //    currentStatePause = Pause;
        //    Timing.PauseCoroutines();

        //}
        //else if (currentStatePause != Pause && Pause == false)
        //{
        //    currentStatePause = Pause;
        //    Timing.ResumeCoroutines();
        //}
    }
#endif
}

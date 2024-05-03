using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class PinchDetection : MonoBehaviour
{
    public static Action<float> OnScale;
    [SerializeField] private float _scaleFactor = 1f;

    private TouchControls _controls;
    private CoroutineHandle _zoomCoroutine;


    private void Awake()
    {
        _controls = new TouchControls();
    }
    private void Start()
    {
        _controls.Touch.SecondaryTouchContact.started += _ => ScaleStart();
        _controls.Touch.SecondaryTouchContact.canceled += _ => ScaleEnd();
        GameController.OnStateChange += StateChange;
    }

    private void StateChange(GameState state)
    {
        //We just want to detect touches while we are in View State
        if (state == GameState.View) _controls.Enable();
        else _controls.Disable();
    }

    private void ScaleStart()
    {
        _zoomCoroutine = Timing.RunCoroutine(PinchDetectionCoroutine());
    }

    private void ScaleEnd()
    {
        Timing.KillCoroutines(_zoomCoroutine);
    }


    private IEnumerator<float> PinchDetectionCoroutine()
    {
        float prevDistance = 0f, distance = 0f;
        while(true)
        {
            distance = Vector2.Distance(_controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>(),
                _controls.Touch.SecondaryFingerPosition.ReadValue<Vector2>());

            if(distance > prevDistance) 
            {
                OnScale?.Invoke(_scaleFactor * Time.deltaTime);
            }
            else if ( distance < prevDistance) 
            {
                OnScale?.Invoke(-_scaleFactor * Time.deltaTime);
            }

            prevDistance = distance;

            yield return Timing.WaitForOneFrame;
        }
    }
}

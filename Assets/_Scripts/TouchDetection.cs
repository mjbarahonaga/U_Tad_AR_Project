using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class TouchDetection : MonoBehaviour
{
    #region Events
    public static Func<Vector2> OnGetPrimaryFingerPos;
    public static Func<Vector2> OnGetSecondaryFingerPos;
    public static Action<Vector2, float> OnStartTouch;
    public static Action<Vector2, float> OnEndTouch;
    public static Action<float> OnScale;
    public static Action<Vector2> OnTapScreen;
    #endregion

    [SerializeField] private float _rotateFactor = 1f;
    [SerializeField] private float _scaleFactor = 1f;

    private TouchControls _controls;
    private CoroutineHandle _zoomCoroutine;
    private Camera _camera;

    private void Awake()
    {
        _controls = new TouchControls();
        _camera = Camera.main;
        _controls.Enable();
    }
    private void Start()
    {
        _controls.Touch.PrimaryTouchContact.started += ctx => StartTouchPrimary(ctx);
        _controls.Touch.PrimaryTouchContact.canceled += ctx => EndTouchPrimary(ctx);
        _controls.Touch.SecondaryTouchContact.started += _ => ScaleStart();
        _controls.Touch.SecondaryTouchContact.canceled += _ => ScaleEnd();
        _controls.Touch.Tap.started += _ => TapOnScreen();

        OnGetPrimaryFingerPos += PrimaryFingerPosition;
        OnGetSecondaryFingerPos += SecondaryFingerPosition;
        //GameController.OnStateChange += StateChange;
    }

    private void OnDestroy()
    {
        _controls.Disable();
    }

    private void TapOnScreen()
    {
        OnTapScreen?.Invoke(_controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>());
#if UNITY_EDITOR
        Debug.Log("Touch position : " + _controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>());
#endif
    }

    //private void StateChange(GameState state)
    //{

    //    //We just want to detect touches while we are in View State
    //    if (state == GameState.View) _controls.Enable();
    //    else
    //    {
    //        Timing.KillCoroutines(_zoomCoroutine);
    //        _controls.Disable();
    //    }
    //}

    private void StartTouchPrimary(InputAction.CallbackContext context)
    {
        OnStartTouch?.Invoke(_controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>(), (float)context.startTime);
    }

    private void EndTouchPrimary(InputAction.CallbackContext context)
    {
        OnEndTouch?.Invoke(_controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>(), (float)context.time);
    }

    private Vector2 PrimaryFingerPosition() =>  _controls.Touch.PrimaryFingerPosition.ReadValue<Vector2>();
    private Vector2 SecondaryFingerPosition() => _controls.Touch.SecondaryFingerPosition.ReadValue<Vector2>();

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

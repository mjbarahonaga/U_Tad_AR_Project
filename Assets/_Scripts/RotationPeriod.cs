using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;

public class RotationPeriod : MonoBehaviour, IPausable
{
    [SerializeField] private PlanetsScriptable _planet;
    [SerializeField] private float _rotationPeriod;
    private float _rotationSpeed;

    private CoroutineHandle _coroutineUpdate;

    private bool _pause = false;
    public void Pause(bool pause)
    {
        if (_coroutineUpdate != default(CoroutineHandle))
        {
            if (pause == true)
            {
                //Timing.KillCoroutines(_coroutineUpdate);
                _pause = true;
                Timing.PauseCoroutines(_coroutineUpdate);
            }
            else
            {
                //_coroutineUpdate = Timing.RunCoroutine(UpdateCoroutine());
                _pause = false;
                Timing.ResumeCoroutines(_coroutineUpdate);
            }
        }

    }

    public void ActivateUpdate()
    {
        _coroutineUpdate = Timing.RunCoroutine(UpdateCoroutine(), Segment.FixedUpdate);
    }

    public IEnumerator<float> UpdateCoroutine()
    {
        if (_coroutineUpdate != default(CoroutineHandle)) yield break;
        while (true)
        {
            if (_pause == false)
                transform.Rotate(Vector3.up, _rotationSpeed * Time.fixedDeltaTime);
            
            yield return Timing.WaitForOneFrame;
        }
    }

    private void OnEnable()
    {
        GameController.OnPause += Pause;
        GameController.OnStartSimulation += ActivateUpdate;

        

    }

    private void Awake()
    {
        _rotationPeriod = _planet.RotationPeriod;
        _rotationSpeed = 360f / _rotationPeriod;
    }

    private void OnDestroy()
    {
        GameController.OnPause -= Pause;
        GameController.OnStartSimulation -= ActivateUpdate;
    }



}

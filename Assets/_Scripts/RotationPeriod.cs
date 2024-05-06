using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using Unity.VisualScripting;

public class RotationPeriod : MonoBehaviour, IPausable
{
    [SerializeField] private bool _isVariantPlanet;
    [SerializeField] private PlanetsScriptable _planet;
    [SerializeField] private float _rotationPeriod;
    private float _rotationSpeed;

    private CoroutineHandle _coroutineUpdate;

    private bool _pause = false;
    public PlanetsScriptable GetData { get => _planet; }
    public void Pause(bool pause)
    {
        if (_isVariantPlanet) return;

        if (_coroutineUpdate != default(CoroutineHandle))
        {
            if (pause == true)
            {
                _pause = true;
                Timing.PauseCoroutines(_coroutineUpdate);
            }
            else
            {
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
            if (_pause == false || _isVariantPlanet)
            {
                float speedSimulation = GameController.SpeedSimulation;
                if (_isVariantPlanet) speedSimulation = 1f;
                transform.Rotate(Vector3.up, _rotationSpeed * Time.fixedDeltaTime * speedSimulation);
            }
                
            
            yield return Timing.WaitForOneFrame;
        }
    }
    private void Awake()
    {
        _rotationPeriod = _planet.RotationPeriod;
        _rotationSpeed = 360f / _rotationPeriod;
        if (_isVariantPlanet)
        {
            _rotationSpeed = 30f;
        }
        
    }

    private void OnEnable()
    {
        if(_isVariantPlanet == false)
        {
            GameController.OnPause += Pause;
            GameController.OnStartSimulation += ActivateUpdate;
            
        }

        ActivateUpdate();
    }

    private void OnDisable()
    {
        if (_isVariantPlanet == false)
        {
            GameController.OnPause -= Pause;
            GameController.OnStartSimulation -= ActivateUpdate;
        }
        else
        {
            Timing.KillCoroutines(_coroutineUpdate);
        }
    }



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;

public class Orbits : MonoBehaviour, IPausable
{
    [SerializeField] private PlanetsScriptable _planetData;
    [SerializeField] private Transform _transformTarget;
    [SerializeField] private float _orbitalRadius;
    [SerializeField] private float _orbitalSpeed;   
    [SerializeField] private float _angle;

    private CoroutineHandle _coroutineUpdate;

    private void OnEnable()
    {
        GameController.OnPause += Pause;
        GameController.OnStartSimulation += ActivateUpdate;
        GameController.OnCheckData += CheckData;

    }
    private void OnDestroy()
    {
        GameController.OnPause -= Pause;
        GameController.OnStartSimulation -= ActivateUpdate;
        GameController.OnCheckData -= CheckData;
        Timing.KillCoroutines(_coroutineUpdate);
    }

    private void Awake()
    {
        _angle = 0;
        float radians = Mathf.Deg2Rad * _angle;

        float x = _orbitalRadius * Mathf.Cos(radians);
        float z = _orbitalRadius * Mathf.Sin(radians);

        _transformTarget.position = new Vector3(x, 0, z) + transform.position;
        _orbitalSpeed = 360f / _planetData.ObitalPeriod;

        _orbitalRadius = (transform.position - _transformTarget.position).magnitude;
    }

    private bool _pause = false;
    public void Pause(bool pause)
    {
        if(_coroutineUpdate != default(CoroutineHandle))
        {
            if (pause == true)
            {
                _pause = true;
                Timing.PauseCoroutines(_coroutineUpdate);
                //Timing.KillCoroutines(_coroutineUpdate);
            }
            else
            {
                _pause = false;
                Timing.ResumeCoroutines(_coroutineUpdate);
                //_coroutineUpdate = Timing.RunCoroutine(UpdateCoroutine());
            }
        }
        
    }


    public void ActivateUpdate()
    {
        _coroutineUpdate = Timing.RunCoroutine(UpdateCoroutine(), Segment.FixedUpdate);
    }

    public void CheckData()
    {
        _orbitalRadius = (transform.position - _transformTarget.position).magnitude;
    }

    public IEnumerator<float> UpdateCoroutine()
    {
        if (_coroutineUpdate != default(CoroutineHandle)) yield break;
        while (true)
        {
            if(_pause == false)
            {
                _angle += _orbitalSpeed * Time.fixedDeltaTime; // * Global Speed (Manager data )
                float radians = Mathf.Deg2Rad * _angle;

                float x = _orbitalRadius * Mathf.Cos(radians);
                float z = _orbitalRadius * Mathf.Sin(radians);

                Vector3 relativePos = transform.parent.transform.rotation * new Vector3(x, 0, z);

                _transformTarget.position = relativePos + transform.parent.transform.position;

            }
            yield return Timing.WaitForOneFrame;
        }
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        _orbitalRadius = (transform.position - _transformTarget.position).magnitude;

    }

    [Header("Gizmos")]
    public int Segments = 40;
    public Color ColorLine = Color.yellow;

    private void OnDrawGizmos()
    {
        if (!_transformTarget) return;

        var targetPos = _transformTarget.position;
        var orbitPos = transform.position;
        Gizmos.color = ColorLine;
        Gizmos.DrawLine(targetPos, orbitPos);

        int segments = Segments;
        float angle = 0f;
        float steps = 2 * Mathf.PI/ segments;

        for(int i =  0; i< segments; ++i)
        {
            Vector3 prevPoint = new Vector3(_orbitalRadius * Mathf.Cos(angle), 0f, _orbitalRadius * Mathf.Sin(angle)) + orbitPos;
            angle+= steps;
            Vector3 curPoint = new Vector3(_orbitalRadius * Mathf.Cos(angle), 0f, _orbitalRadius * Mathf.Sin(angle)) + orbitPos;
            Gizmos.DrawLine(prevPoint, curPoint);
        }
        
    }

    
#endif
}

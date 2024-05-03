using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Idle = 0,
    Discovering = 1,
    Placing = 2,
    View = 3,
    Selecting = 4
}

public class GameController : MonoBehaviour
{
    public static Action<bool> OnPause;
    public static Action OnStartSimulation;
    public static Action OnCheckData;

    public static Action<GameState> OnStateChange;
    public static Action<ARSelectable> OnSelected;
    public static Func<GameState> OnGetState;

    [SerializeField] private ARRaycastInteraction _arRaycastInteraction;
    [SerializeField] private GameObject _solarSystem;

    [SerializeField] private GameState _currentState = GameState.Idle;

    private CoroutineHandle _currentCoroutine;

    public GameState CurrentState
    {
        get { return _currentState; }
        private set
        {
            if (_currentState == value) return;
            _currentState = value;
            OnStateChange?.Invoke(value);
            UpdateState();
        }
    }

    #region CUSTOM METHODS
    public void SetState(int value) => CurrentState = (GameState)value;

    public void SetState(GameState value) => CurrentState = value;

    private void UpdateState()
    {
        Timing.KillCoroutines(_currentCoroutine);

        switch (_currentState)
        {
            case GameState.Idle:
                break;
            case GameState.Discovering:
                _currentCoroutine = Timing.RunCoroutine(UpdateDiscovering());
                break;
            case GameState.Placing:
                _currentCoroutine = Timing.RunCoroutine(UpdatePlacing());
                break;
            case GameState.View:
                _currentCoroutine = Timing.RunCoroutine(UpdateView(), Segment.SlowUpdate);
                break;
            case GameState.Selecting:
                _currentCoroutine = Timing.RunCoroutine(UpdateSelecting());
                break;
        }
    }

    private void ScaleSolarSystem(float value)
    {
        if(_solarSystem.activeInHierarchy)
        {
            _solarSystem.transform.localScale += Vector3.one * value;
            OnCheckData?.Invoke();
        }
            
    }

    private IEnumerator<float> UpdateDiscovering()
    {

        while(_currentState == GameState.Discovering)
        {
            if (_arRaycastInteraction.DetectEnvironment())
            {
                SetState(GameState.Placing);
            }
            yield return Timing.WaitForOneFrame;
        }
    }

    private IEnumerator<float> UpdatePlacing()
    {
        while(_solarSystem == null)
        {
            _arRaycastInteraction.DetectEnvironment();
            _solarSystem = _arRaycastInteraction.UpdatePlacing();
            yield return Timing.WaitForOneFrame;
        }
        SetState(GameState.View);
    }

    private IEnumerator<float> UpdateView()
    {
        ARSelectable _currentSelected = null;
        while(_currentSelected == null)
        {
            _currentSelected = _arRaycastInteraction.DetectPlanetSelection();
            if(_currentSelected != null )
            {
                OnSelected?.Invoke(_currentSelected);
                SetState(GameState.Selecting);
            }
            yield return Timing.WaitForOneFrame;
        }
    }

    private IEnumerator<float> UpdateSelecting()
    {
        yield return 0;
    }

    #endregion
    #region UNITY METHODS
    private void Start()
    {
        PinchDetection.OnScale += ScaleSolarSystem;
        OnGetState += () => _currentState;
        CurrentState = GameState.Discovering;
    }

    private void OnDestroy()
    {
        PinchDetection.OnScale -= ScaleSolarSystem;
    }

#endregion

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

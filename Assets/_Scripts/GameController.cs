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
    #region Events
    public static Action<bool> OnPause;
    public static Action OnStartSimulation;
    public static Action OnCheckData;

    public static Action<GameState> OnStateChange;
    public static Action<ARSelectable> OnSelected;
    public static Func<GameState> OnGetState;
    #endregion

    [Range(1f, 20f)]
    public static float SpeedSimulation = 1f;

    [SerializeField] private ARRaycastInteraction _arRaycastInteraction;
    [SerializeField] private GameState _currentState = GameState.Idle;

    [SerializeField] private float _minDistanceSwipping = 0.2f;
    [SerializeField] private float _maxTimeSwipping = 1f;

    private GameObject _solarSystem;
    private CoroutineHandle _currentCoroutine;
    private ARSelectable _currentSelected = null;

    private Vector2 _startPosSwipe;
    private Vector2 _endPosSwipe;
    private float _startTimeSwipe;
    private float _endTimeSwipe;

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

    public void SetSpeedSimulation(float value)
    {
        SpeedSimulation = value;
    }
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
                _currentCoroutine = Timing.RunCoroutine(UpdateView());
                break;
            case GameState.Selecting:
                //_currentCoroutine = Timing.RunCoroutine(UpdateSelecting());
                break;
        }
    }

    //private void SwipeEnd(Vector2 pos, float time)
    //{
    //    _startPosSwipe = pos;
    //    _startTimeSwipe = time;
    //}

    //private void SwipeStart(Vector2 pos, float time)
    //{
    //    _endPosSwipe = pos;
    //    _endTimeSwipe = time;
    //    DetectSwipe();
    //}

    //private void DetectSwipe()
    //{
    //    if(Vector3.Distance(_startPosSwipe, _endPosSwipe) >= _minDistanceSwipping &&
    //        (_endTimeSwipe - _startTimeSwipe) >= _maxTimeSwipping)
    //    {
    //        Debug.DrawLine(_startPosSwipe, _endPosSwipe, Color.red, 2f);
    //        Vector3 direction = _endPosSwipe - _startPosSwipe;
    //        Vector2 direction2D = new Vector2(direction.x, direction.y).normalized;
    //        //SwipeDirection(direction2D);
    //    }
    //}

    //private void SwipeDirection(Vector2 direction)
    //{
    //    if (Vector2.Dot(Vector2.left, direction) > 0.3f) CheckToRotateSolarSystem(1f);
    //    if (Vector2.Dot(Vector2.right, direction) > 0.3f) CheckToRotateSolarSystem(-1f);
    //}

    private void CheckToRotateSolarSystem()
    {
        if(_solarSystem.activeInHierarchy && _currentState == GameState.View)
        {
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                float rotateHorizontal = Input.GetTouch(0).deltaPosition.x * 0.5f;
                _solarSystem.transform.Rotate(Vector3.up, rotateHorizontal, Space.World);
            }
            
        }
    }

    private void ScaleSolarSystem(float value)
    {
        if(_solarSystem.activeInHierarchy && _currentState == GameState.View)
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
        bool selected = false;
        while(selected == false)
        {
            CheckToRotateSolarSystem();
            if (_currentSelected != null )
            {
                selected = true;
                OnSelected?.Invoke(_currentSelected);
                SetState(GameState.Selecting);
            }
            yield return Timing.WaitForOneFrame;
        }
        _currentSelected = null;
    }


    private IEnumerator<float> UpdateSelecting()
    {
        yield return 0;
    }

    public void TappedScreen(Vector2 touchPos)
    {
        _currentSelected =  _arRaycastInteraction.DetectPlanetSelection(touchPos);
#if UNITY_EDITOR
        Debug.Log(_currentSelected?.ToString());
#endif
    }

    public void PlaySimulation() => OnPause?.Invoke(false);
    public void StopSimulation() => OnPause?.Invoke(true);

    #endregion
    #region UNITY METHODS
    private void Start()
    {
        //TouchDetection.OnStartTouch += SwipeStart;
        //TouchDetection.OnEndTouch += SwipeEnd;

        TouchDetection.OnScale += ScaleSolarSystem;
        TouchDetection.OnTapScreen += TappedScreen;
        OnGetState += () => _currentState;
        CurrentState = GameState.Discovering;
    }

    private void OnDestroy()
    {
        //TouchDetection.OnStartTouch -= SwipeStart;
        //TouchDetection.OnEndTouch -= SwipeEnd;

        TouchDetection.OnScale -= ScaleSolarSystem;
        TouchDetection.OnTapScreen -= TappedScreen;
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

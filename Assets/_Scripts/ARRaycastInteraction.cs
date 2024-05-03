using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARRaycastInteraction : MonoBehaviour
{
    [SerializeField] private ARRaycastManager _arRaycastManager;
    [SerializeField] private GameObject _prefab; //SolarSystem
    [SerializeField] private GameObject _indicator;
    [SerializeField] private Camera _arCamera;

    private Pose _placementPose;
    private bool _placementPoseIsValid = false;
    //private bool _isCreating = false;
    private GameObject _placedObject = null;

    //public bool IsCreating { get => _isCreating; }
    #region UNITY METHODS
    private void Start()
    {
        if(_arRaycastManager == null) 
            _arRaycastManager= GetComponent<ARRaycastManager>();
    }

    #endregion

    #region CUSTOM METHODS

    public GameObject UpdatePlacing()
    {
        GameObject result = null;
        UpdatePlacementPose();
        UpdatePlacementIndicator();
        if(_placementPoseIsValid == false) return null;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began){
#endif
            if (_placedObject == null) PlacePrefab();
            else _placedObject.transform.position = _placementPose.position;
        }

        if (_placedObject != null)
        {
#if UNITY_EDITOR
            if ( Input.GetMouseButton(0))
            {
                _placedObject.transform.position = _placementPose.position;
#else
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved){
            float rotateHorizontal = Input.GetTouch(0).deltaPosition.x * 0.5f;
#endif
                _placedObject.transform.Rotate(0f, -0.5f, 0f, Space.World);
            }

            if (Input.touchCount == 1)
            {
                _placedObject.transform.position = _placementPose.position;
            }

#if UNITY_EDITOR
            if (Input.GetMouseButtonUp(0))
            {
#else
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended){
#endif
                result = _placedObject;
                _placedObject = null;
            }
        }

        return result;
    }
    
    public void PlacePrefab()
    {
        if(_placementPoseIsValid)
            _placedObject = Instantiate(_prefab, _placementPose.position, _placementPose.rotation); 
        else
        {
            //Feedback?
        }

    }

    private void UpdatePlacementPose()
    {
        var screenCenter = _arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        _arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        _placementPoseIsValid = hits.Count > 0;
        if(_placementPoseIsValid)
        {
            _placementPose = hits[0].pose;
            var cameraForward = _arCamera.transform.forward;
            _placementPose.rotation *= Quaternion.AngleAxis(0.1f * Time.fixedDeltaTime, Vector3.up);
        }
    }

    private void UpdatePlacementIndicator()
    {
        if(_placementPoseIsValid)
        {
            SetIndicatorState(true);
            _indicator.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);
            return;
        }

        SetIndicatorState(false);
        
    }

    private void SetIndicatorState(bool state) => _indicator.SetActive(state);

    public bool DetectEnvironment()
    {
        List<ARRaycastHit> hits = new();
        return _arRaycastManager.Raycast(_arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f)),
            hits, TrackableType.Planes);
    }

    public ARSelectable DetectPlanetSelection()
    {
        Ray ray;
#if UNITY_EDITOR
        var screenCenter = _arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        _arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);
        if (hits.Count <= 0) return null;
        ray = _arCamera.ScreenPointToRay(hits[0].pose.position);
        if (Input.GetMouseButtonDown(0))
        {
#else
        ray = _arCamera.ScreenPointToRay(Input.GetTouch(0).position);
        if(Input.GetTouch(0).phase == TouchPhase.Began) { 
#endif
            if(Physics.Raycast(ray, out RaycastHit hit, 50f))
            {
                ARSelectable result = hit.collider.gameObject.GetComponent<ARSelectable>();
                return result;
            }
        }
        return null;
    }
    #endregion
}

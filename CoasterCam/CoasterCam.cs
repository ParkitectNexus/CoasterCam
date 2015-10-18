using System;
using System.Collections.Generic;
using Parkitect.UI;
using UnityEngine;
using UnityEngine.VR;

namespace CoasterCam
{
    internal class CoasterCam : MonoBehaviour
    {
        private GameObject _coasterCam;
        private GameObject _origCam;

        private bool _isOnRide;
        
        public static CoasterCam Instance;
        
        private float _origShadowDist;
        private int _origQualityLevel;
        private int _origResoWidth;
        private int _origResoHeight;

        private Camera _cam;
        
        float _fps = 0.0f;
        
        private Rect _rect;

        private List<Transform> _seats = new List<Transform>();

        private int _seatIndex = 0;

        private void Awake()
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.R) && !_isOnRide)
            {
                GameObject ride = GameObjectUnderMouse();

                if (ride != null)
                {
                    _seats.Clear();
                    _seatIndex = 0;

                    Utility.recursiveFindTransformsStartingWith("seat", ride.GetComponentInParent<Attraction>().transform, _seats);

                    if (_seats.Count > 0)
                        EnterCoasterCam(_seats[_seatIndex].gameObject);
                }
            }
            else if (Input.GetKeyUp(KeyCode.R))
            {
                LeaveCoasterCam();
            }

            if (_isOnRide)
            {
                if (Math.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.1)
                {
                    LeaveCoasterCam();

                    if (Input.GetAxis("Mouse ScrollWheel") > 0)
                    {
                        if (++_seatIndex == _seats.Count)
                            _seatIndex = 0;
                    }
                    
                    if (Input.GetAxis("Mouse ScrollWheel") < 0)
                    {
                        if (--_seatIndex < 0)
                            _seatIndex = _seats.Count - 1;
                    }

                    EnterCoasterCam(_seats[_seatIndex].gameObject);
                }

                AdaptFarClipPaneToFps();
            }
        }

        private void AdaptFarClipPaneToFps()
        {
            _fps = 1.0f / Time.deltaTime;

            if (_fps < 50)
            {
                _cam.farClipPlane = Math.Max(40, _cam.farClipPlane - 0.3f);
            }

            if (_fps > 55)
            {
                _cam.farClipPlane = Math.Min(120, _cam.farClipPlane + 0.2f);
            }
        }

        private GameObject GameObjectUnderMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                return hit.transform.gameObject;
            }

            return null;
        }

        private void LowerGraphicSettings()
        {
            Application.targetFrameRate = -1;

            _origShadowDist = QualitySettings.shadowDistance;
            _origQualityLevel = QualitySettings.GetQualityLevel();
            _origResoWidth = Screen.width;
            _origResoHeight = Screen.height;
            
            QualitySettings.SetQualityLevel(0);
            QualitySettings.shadowDistance = 0f;
            QualitySettings.antiAliasing = 2;

            if (Screen.fullScreen)
                Screen.SetResolution((int)(_origResoWidth / 1.2f), (int)(_origResoHeight / 1.2f), Screen.fullScreen);
        }

        private void RestoreGraphicSettings()
        {
            Application.targetFrameRate = 60;

            QualitySettings.shadowDistance = _origShadowDist;
            QualitySettings.SetQualityLevel(_origQualityLevel);

            if (Screen.fullScreen)
                Screen.SetResolution(_origResoWidth, _origResoHeight, Screen.fullScreen);
        }

        public void EnterCoasterCam(GameObject onGo)
        {
            if (_isOnRide)
                return;

            LowerGraphicSettings();

            UIWorldOverlayController.Instance.gameObject.SetActive(false);

            _origCam = Camera.main.gameObject;

            _origCam.SetActive(false);

            //if (VRDevice.isPresent)
            //{
            //    VRSettings.loadedDevice = VRDeviceType.Oculus;
            //}

            _coasterCam = new GameObject();
            _coasterCam.AddComponent<Camera>();
            _coasterCam.GetComponent<Camera>().nearClipPlane = 0.05f;
            _coasterCam.GetComponent<Camera>().farClipPlane = 100f;

            _coasterCam.AddComponent<AudioListener>();

            _coasterCam.transform.parent = onGo.transform;
            _coasterCam.transform.localPosition = new Vector3(0, 0.35f, 0.1f);
            _coasterCam.transform.localRotation = Quaternion.identity;

            _coasterCam.AddComponent<MouseLookAround>();
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _isOnRide = true;

            InputTracking.Recenter();
        }

        public void LeaveCoasterCam()
        {
            if (!_isOnRide)
                return;

            RestoreGraphicSettings();

            //if (VRDevice.isPresent)
            //{
            //    VRSettings.loadedDevice = VRDeviceType.None;
            //}

            _origCam.SetActive(true);

            Destroy(_coasterCam);

            UIWorldOverlayController.Instance.gameObject.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _isOnRide = false;
        }

        void OnDestroy()
        {
            LeaveCoasterCam();
        }
    }
}

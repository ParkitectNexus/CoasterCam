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
        
        float _fps;
       
        private readonly List<Transform> _seats = new List<Transform>();

        private int _seatIndex;

        private void Awake()
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
		}
		
		private void Update()
        {
            if (Input.GetKeyUp(KeyCode.R) && !_isOnRide && !UIUtility.isInputFieldFocused()) {
	            SerializedMonoBehaviour ride = Utility.getObjectBelowMouse().hitObject;
                
                if (ride != null)
                {
                    Attraction attr = ride.GetComponentInParent<Attraction>();

                    if (attr == null)
                    {
                        attr = ride.GetComponentInChildren<Attraction>();
                    }

                    if (attr != null)
                    {
                        _seats.Clear();
                        _seatIndex = 0;

                        Utility.recursiveFindTransformsStartingWith("seat", attr.transform, _seats);

                        if (_seats.Count > 0)
                            EnterCoasterCam(_seats[_seatIndex].gameObject);
                    }
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

        public void EnterCoasterCam(GameObject onGo)
        {
            if (_isOnRide)
                return;

            UIWorldOverlayController.Instance.gameObject.SetActive(false);

            string tag = Camera.main.tag;

            _origCam = Camera.main.gameObject;

            _origCam.SetActive(false);

            _coasterCam = new GameObject();
            _coasterCam.tag = tag;
            _coasterCam.AddComponent<Camera>();

			CullingGroupManager.Instance.setTargetCamera(_coasterCam.GetComponent<Camera>());
			_coasterCam.GetComponent<Camera>().nearClipPlane = 0.05f;
            _coasterCam.GetComponent<Camera>().farClipPlane = 100f;
            _coasterCam.GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;

            _coasterCam.AddComponent<AudioListener>();

            _coasterCam.transform.parent = onGo.transform;
            _coasterCam.transform.localPosition = new Vector3(0, 0.35f, 0.1f);
            _coasterCam.transform.localRotation = Quaternion.identity;

            _coasterCam.AddComponent<MouseLookAround>();

            _cam = _coasterCam.GetComponent<Camera>();
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _isOnRide = true;

            InputTracking.Recenter();
        }

        public void LeaveCoasterCam()
        {
            if (!_isOnRide)
                return;
            
            _origCam.SetActive(true);
			CullingGroupManager.Instance.setTargetCamera(_origCam.GetComponent<Camera>());

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

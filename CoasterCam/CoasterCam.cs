using System;
using Parkitect.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CoasterCam
{
    internal class CoasterCam : MonoBehaviour
    {
        private GameObject _coasterCam;

        private bool _isOnRide;

        private Attraction _currentAttraction;

        public static CoasterCam Instance;

        private ICamLocationFinder _locationFinder;

        private float _origShadowDist;
        private int _origQualityLevel;
        private int _origResoWidth;
        private int _origResoHeight;

        private Camera _cam;

        // fps stuff
        int qty = 0;
        float currentAvgFPS = 0;

        int frameCount = 0;
        float nextUpdate = 0.0f;
        float fps = 0.0f;
        float updateRate = 10.0f;  // 4 updates per sec.

        private void Awake()
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);

            nextUpdate = Time.time;
        }

        private void Update()
        {
            UnityEngine.Object[] attractionStates = FindObjectsOfType(typeof(AttractionStatsTab));

            //Debug.Log(attractionStates.Length);

            foreach (AttractionStatsTab attractionState in attractionStates)
            {
                if (attractionState.GetComponentInChildren<CoasterCamStarter>() == null)
                {
                    Debug.Log("Creating button");
                    GameObject button = new GameObject();
                    button.transform.parent = attractionState.transform;
                    button.AddComponent<RectTransform>();
                    button.AddComponent<Button>();
                    button.transform.position = new Vector3(20, 20, 20);
                    //button.GetComponent<RectTransform>().SetSize(size);
                    //button.GetComponent<Button>().onClick.AddListener(method);
                    //GameObject btn = Instantiate(new UnityEngine.UI.Button);
                }
            }


            if (Input.GetKeyUp(KeyCode.R) && !_isOnRide)
            {
                GameObject _ride = GameObjectUnderMouse();

                AttractionTypeDecider atd = new AttractionTypeDecider(_ride);

                if(atd.GetType() == AttractionTypeDecider.AttractionType.Tracked)
                {
                    _locationFinder = new CarFinder(_ride.GetComponentInParent<TrackedRide>());

                    EnterCoasterCam(_locationFinder.GetNextLocation());
                }
            }
            else if (Input.GetKeyUp(KeyCode.R))
            {
                LeaveCoasterCam();
            }

            if (_isOnRide)
            {
                if (Input.GetKeyUp(KeyCode.F))
                {
                    EnterCoasterCam(_locationFinder.GetNextLocation());
                }

                AdaptFarClipPaneToFPS();
            }
        }

        private void AdaptFarClipPaneToFPS()
        {
            fps = 1.0f / Time.deltaTime;

            if (fps < 50)
            {
                _cam.farClipPlane = Math.Max(40, _cam.farClipPlane - 0.3f);
            }

            if (fps > 55)
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

            _coasterCam = new GameObject();

            _cam = _coasterCam.AddComponent<Camera>();

            UIWorldOverlayController.Instance.gameObject.SetActive(false);

            //EmotiFloat[] emotis = GameObject.FindObjectsOfType<EmotiFloat>() as EmotiFloat[];

            //foreach (EmotiFloat emotiFloat in emotis)
            //{
            //    emotiFloat.gameObject.SetActive(false);
            //}

            _cam.nearClipPlane = 0.1f;
            _cam.farClipPlane = 50f;

            _coasterCam.AddComponent<AudioListener>();

            _coasterCam.transform.parent = onGo.transform;
            _coasterCam.transform.localPosition = new Vector3(0, 0.7f, 0);
            _coasterCam.transform.localRotation = Quaternion.identity;

            _coasterCam.AddComponent<MouseLookAround>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _isOnRide = true;
        }

        public void LeaveCoasterCam()
        {
            if (!_isOnRide)
                return;

            RestoreGraphicSettings();

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

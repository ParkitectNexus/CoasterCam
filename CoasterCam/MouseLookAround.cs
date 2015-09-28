using UnityEngine;

namespace CoasterCam
{
    public class MouseLookAround : MonoBehaviour
    {
        public enum RotationAxes
        {
            MouseXAndY = 0, MouseX = 1, MouseY = 2
        }

        public RotationAxes Axes = RotationAxes.MouseXAndY;

        private float _sensitivityX = 10F;
        private float _sensitivityY = 10F;
        private float _minimumX = -135F;
        private float _maximumX = 135F;
        private float _minimumY = -60F;
        private float _maximumY = 60F;

        private float _rotationY;

        void Update()
        {
            if (Axes == RotationAxes.MouseXAndY)
            {
                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * _sensitivityX;
                
                _rotationY += Input.GetAxis("Mouse Y") * _sensitivityY;
                _rotationY = Mathf.Clamp(_rotationY, _minimumY, _maximumY);

                transform.localEulerAngles = new Vector3(-_rotationY, rotationX, 0);
            }
            else if (Axes == RotationAxes.MouseX)
            {
                transform.Rotate(0, Input.GetAxis("Mouse X") * _sensitivityX, 0);
            }
            else
            {
                _rotationY += Input.GetAxis("Mouse Y") * _sensitivityY;
                _rotationY = Mathf.Clamp(_rotationY, _minimumY, _maximumY);

                transform.localEulerAngles = new Vector3(-_rotationY, transform.localEulerAngles.y, 0);
            }
        }
    }
}

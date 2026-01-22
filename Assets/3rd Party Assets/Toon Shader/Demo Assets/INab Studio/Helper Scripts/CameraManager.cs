using UnityEngine;
using UnityEngine.UI;

namespace INab.Demo
{
    public class CameraManager : MonoBehaviour
    {
        public Transform target;

        public Vector3 transformv;

        public float xRot = 0f;
        public float RestartRotation = 0f;
        public float yRot = 0f;

        public float distance = 5f;
        public float sensitivity = 10f;

        //UI elements
        public Slider rotationSlider;
        public Slider distanceSlider;

        //private members
        private bool m_Locked = false;

        private void Start()
        {
            rotationSlider.onValueChanged.AddListener(delegate { RotationValueChange(); });
            distanceSlider.onValueChanged.AddListener(delegate { DistanceValueChange(); });
        }

        public void RotationValueChange()
        {
            if (m_Locked)
            {
                yRot = rotationSlider.value;
                transform.position = target.position + transformv + Quaternion.Euler(xRot, yRot, 0f) * (distance * -Vector3.back);
                transform.LookAt(target.position + transformv, Vector3.up);
            }
            else
            {
                yRot = rotationSlider.value;
            }
        }

        public void DistanceValueChange()
        {
            if (m_Locked)
            {
                distance = distanceSlider.value;
                transform.position = target.position + transformv + Quaternion.Euler(xRot, yRot, 0f) * (distance * -Vector3.back);
                transform.LookAt(target.position + transformv, Vector3.up);
            }
            else
            {
                distance = distanceSlider.value;
            }
        }

        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {
                LockRotation();
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                yRot = RestartRotation;
            }

            if (m_Locked) return;

            yRot += sensitivity * Time.deltaTime;

            transform.position = target.position + transformv + Quaternion.Euler(xRot, yRot, 0f) * (distance * -Vector3.back);
            transform.LookAt(target.position + transformv, Vector3.up);

            if (yRot >= 360) yRot = 0;

            UiManager();

        }

        void UiManager()
        {
            rotationSlider.value = yRot;
        }

        public void LockRotation()
        {
            m_Locked = !m_Locked;
        }
    }
}
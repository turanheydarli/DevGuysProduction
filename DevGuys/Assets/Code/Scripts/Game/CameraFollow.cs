using UnityEngine;

namespace Code.Scripts.Game
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothing = 5f;


        private Vector3 _offset;


        void Start()
        {
            _offset = transform.position - target.position;
        }

        void FixedUpdate()
        {
            Vector3 targetCamPos = target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
        }
    }
}
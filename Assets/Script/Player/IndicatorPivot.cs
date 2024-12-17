using Fusion;
using UnityEngine;

namespace Tangar.io
{
    public class IndicatorPivot : NetworkBehaviour
    {
        private Rigidbody _playerRigidbody;

        [SerializeField] private float rotationSmoothing = 100f;
        [SerializeField] private float movementThreshold = 0.1f; 

        private Vector3 _lastDirection = Vector3.forward; 

        public override void FixedUpdateNetwork()
        {
            if (_playerRigidbody == null)
            {
                _playerRigidbody = GetComponentInParent<Rigidbody>();
                if (_playerRigidbody == null)
                {
                    Debug.LogWarning("Player Rigidbody not found!");
                    return;
                }
            }

            Vector3 velocity = _playerRigidbody.velocity;
            Vector3 movementDirection = new Vector3(velocity.x, 0, velocity.z);


            if (movementDirection.magnitude > movementThreshold)
            {
                _lastDirection = movementDirection.normalized;
            }

            float targetAngle = Mathf.Atan2(_lastDirection.x, _lastDirection.z) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing * Time.deltaTime);
        }
    }
}

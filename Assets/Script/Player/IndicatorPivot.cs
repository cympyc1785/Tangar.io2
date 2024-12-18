using Fusion;
using UnityEngine;

namespace Tangar.io
{
    public class IndicatorPivot : NetworkBehaviour
    {
        [SerializeField] private float rotationSmoothing = 100f;
        [SerializeField] private float movementThreshold = 0.1f;
        [SerializeField] private PlayerMovementController _playerMovementController;

        private Vector3 _lastDirection { get; set; }

        public override void FixedUpdateNetwork()
        {
            Vector3 velocity = _playerMovementController._lastDirection;
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

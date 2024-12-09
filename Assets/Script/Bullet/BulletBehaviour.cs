using Fusion;
using UnityEngine;

namespace Tangar.io
{
    // Defines how the bullet behaves
    public class BulletBehaviour : NetworkBehaviour
    {
        // The settings
        [SerializeField] private float _maxLifetime = 3.0f;
        [SerializeField] private float _speed = 200.0f;
        [SerializeField] private LayerMask _playerLayer;

        // The countdown for a bullet lifetime.
        [Networked] private TickTimer _currentLifetime { get; set; }
        [Networked] public Vector3 _direction { get; set; }

        public override void Spawned()
        {
            if (Object.HasStateAuthority == false) return;

            // The network parameters get initializes by the host. These will be propagated to the clients since the
            // variables are [Networked]
            _currentLifetime = TickTimer.CreateFromSeconds(Runner, _maxLifetime);
        }

        public override void FixedUpdateNetwork()
        {
            // If the bullet has not hit an asteroid, moves forward.
            if (HasHitPlayer() == false)
            {
                transform.Translate(_direction * _speed * Runner.DeltaTime, Space.World);
            }
            else
            {
                Runner.Despawn(Object);
                return;
            }

            CheckLifetime();
        }

        // If the bullet has exceeded its lifetime, it gets destroyed
        private void CheckLifetime()
        {
            if (_currentLifetime.Expired(Runner) == false) return;

            Runner.Despawn(Object);
        }

        // Check if the bullet will hit player in the next tick.
        private bool HasHitPlayer()
        {
            var hitPlayer = Runner.LagCompensation.Raycast(transform.position, transform.forward, _speed * Runner.DeltaTime,
                Object.InputAuthority, out var hit, _playerLayer);

            if (hitPlayer == false) return false;

            var playerController = hit.GameObject.GetComponent<PlayerController>();

            if (playerController._isAlive == false)
                return false;

            playerController.PlayerWasHit(Object.InputAuthority);

            return true;
        }
    }
}

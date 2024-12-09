using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

namespace Tangar.io
{
    public class TanmakBehaviour : NetworkBehaviour
    {
        [SerializeField] private int _points = 1;

        // Used to delay the despawn after the hit and play the destruction animation.
        [Networked] private NetworkBool _wasHit { get; set; }

        private NetworkRigidbody3D _networkRigidbody;

        public bool IsAlive => !_wasHit;

        public override void Spawned()
        {
            _networkRigidbody = GetComponent<NetworkRigidbody3D>();
            //_networkRigidbody.InterpolationTarget.localScale = Vector3.one;
        }

        // When hit by another object, this method is called to decide what to do next.
        public void HitPlayer(PlayerRef player)
        {
            // The asteroid hit only triggers behaviour on the host and if the asteroid had not yet been hit.
            if (Object == null) return;
            if (Object.HasStateAuthority == false) return;
            if (_wasHit) return;

            // If this hit was triggered by a projectile, the player who shot it gets points
            // The player object is retrieved via the Runner.
            if (Runner.TryGetPlayerObject(player, out var playerNetworkObject))
            {
                playerNetworkObject.GetComponent<PlayerDataNetworked>().AddToScore(_points);
            }

            _wasHit = true;
            Runner.Despawn(Object);
        }
    }
}

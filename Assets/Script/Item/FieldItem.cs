using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tangar.io
{
    // Class for spawned items on scene.
    public class FieldItem : NetworkBehaviour
    {
        public NetworkPrefabRef ItemPrefab;

        [Networked] private NetworkBool _wasHit { get; set; }

        public bool IsAlive => !_wasHit;

        public override void Spawned()
        {
            _wasHit = false;
        }

        public void GotItem(PlayerRef player)
        {
            if (Object == null) return;
            if (Object.HasStateAuthority == false) return;
            if (_wasHit) return;

            Debug.Log($"{player.PlayerId} Got Item!");

            _wasHit = true;
            Runner.Despawn(Object);
        }
    }
}


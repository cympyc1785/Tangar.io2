using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tangar.io
{
    public class Barrier : Item
    {
        private Inventory _inventory = null;
        private PlayerController _playerController = null;

        public void Start()
        {
            _inventory = transform.parent.GetComponent<Inventory>();
            _playerController = _inventory.PlayerController;
        }

        public override void Use()
        {
            Debug.Log("Barrier Invoked!");
            _playerController.SetInvincible(true);
            Runner.Despawn(Object);
        }
    }
}


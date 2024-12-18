using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tangar.io
{
    public class Dash : Item
    {
        private PlayerMovementController _movementController;

        public void Start()
        {
            _movementController = GetComponentInParent<PlayerMovementController>();
        }

        public override bool Use()
        {
            if (!_movementController._isControllable) return false;

            _movementController.Dash();

            Runner.Despawn(Object);

            return true;
        }
    }
}

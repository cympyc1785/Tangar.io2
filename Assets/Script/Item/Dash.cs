using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tangar.io
{
    public class Dash : Item
    {
        private PlayerMovementController _movementController;


        public override void Use()
        {
            GetComponentInParent<PlayerMovementController>().Dash();
            Runner.Despawn(Object);
        }
    }
}

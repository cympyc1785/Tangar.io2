using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tangar.io
{
    public class DummyItem : Item
    {
        public override void Use()
        {
            Debug.Log("Item Invoked!");
            Runner.Despawn(Object);
        }
    }
}


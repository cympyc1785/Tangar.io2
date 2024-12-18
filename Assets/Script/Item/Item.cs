using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tangar.io
{
    // Parent of all items
    public abstract class Item : NetworkBehaviour
    {
        public int ItemId;
        public string ItemName;
        public string ItemDescription;
        public Sprite ItemIcon;

        public abstract void StartItem();
        // Return true if executed successfully
        public abstract bool Use();
    }
}

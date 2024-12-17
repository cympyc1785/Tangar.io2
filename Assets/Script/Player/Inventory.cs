using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Tangar.io
{
    public class Inventory : NetworkBehaviour
    {
        public List<Item> _inventory = new List<Item>(2);

        public void AddItem(NetworkPrefabRef itemPrefab)
        {
            if (itemPrefab != null && _inventory.Count < 2)
            {
                var itemObject = Runner.Spawn(itemPrefab);
                if (itemObject.TryGetComponent<Item>(out var item))
                {
                    _inventory.Add(item);
                    Debug.Log($"{item.ItemName} is added to inventory.");
                }
            }
        }

        // Use First Item
        public void UseItem()
        {
            if (_inventory.Count > 0)
            {
                _inventory[0].Use();
                Debug.Log($"{_inventory[0].ItemName} is used.");
                _inventory.RemoveAt(0);
            }
        }

        // Change Item by changing order of inventory items
        public void ChangeItem()
        {
            Item item = _inventory[0];
            _inventory.RemoveAt(0);
            _inventory.Add(item);
        }

        public void DisplayInventory()
        {
            foreach (var item in _inventory)
            {
                Debug.Log($"Item: {item.ItemName}");
            }
        }
    }
}

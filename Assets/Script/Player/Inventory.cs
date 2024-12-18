using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

namespace Tangar.io
{
    public class Inventory : NetworkBehaviour
    {
        public PlayerController PlayerController = null;
        public List<Item> _inventory = new List<Item>(1);
        public Image _itemImage = null;
        public string inventoryChildName = "Inventory";

        private ChangeDetector _changeDetector;

        [Networked]
        [OnChangedRender(nameof(DisplayInventory))]
        private NetworkObject _itemObject {  get; set; }

        private Image GetItemImage()
        {
            if (_itemImage == null)
            {
                _itemImage = GameObject.Find("Item Image").GetComponent<Image>();
            }
            
            return _itemImage;
        }

        public void StartInventory()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        public void AddItem(NetworkPrefabRef itemPrefab)
        {
            if (itemPrefab != null && _inventory.Count < 1)
            {
                _itemObject = Runner.Spawn(itemPrefab);
                Transform inventoryTransform = transform.Find(inventoryChildName);

                _itemObject.transform.SetParent(inventoryTransform);
                
                if (_itemObject.TryGetComponent<Item>(out var item))
                {
                    item.StartItem();

                    _inventory.Add(item);

                    Debug.Log($"{item.ItemName} is added to inventory. Inventory Count : {_inventory.Count}");
                }
            }
        }

        // Use First Item
        public void UseItem()
        {
            if (_inventory.Count > 0)
            {
                if (_inventory[0].Use())
                {
                    Debug.Log($"{_inventory[0].ItemName} is used.");
                    
                    _inventory.RemoveAt(0);
                    _itemObject = null;

                    if (!GetComponent<NetworkObject>().HasInputAuthority) return;
                    GetItemImage().sprite = null;
                }
                else
                {
                    Debug.Log($"{_inventory[0].ItemName} not used.");
                }
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
            if (!GetComponent<NetworkObject>().HasInputAuthority) return;

            if (_itemObject != null)
            {
                GetItemImage().sprite = _itemObject.GetComponent<Item>().ItemIcon;
            }
            else
            {
                GetItemImage().sprite = null;
            }
        }
    }
}

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

        [SerializeField] private float _barrierTime = 3.0f;

        [Networked] private TickTimer _barrierTimer { get; set; }

        public override void StartItem()
        {
            _inventory = transform.GetComponentInParent<Inventory>();
            _playerController = _inventory.PlayerController;
        }

        public override void FixedUpdateNetwork()
        {
            if (_barrierTimer.Expired(Runner) || _barrierTimer.IsRunning && !_playerController._isInvincible)
            {
                _playerController.SetInvincible(false);

                Runner.Despawn(Object);
            }
        }

        public override bool Use()
        {
            if (_playerController._isInvincible) return false;
            Debug.Log("Barrier Invoked!");

            _barrierTimer = TickTimer.CreateFromSeconds(Runner, _barrierTime);
            
            _playerController.SetInvincible(true);

            return true;
        }
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using NetworkTransform = Fusion.NetworkTransform;

namespace Tangar.io
{
    // The class is dedicated to controlling Firing
    public class PlayerFireController : NetworkBehaviour
    {
        // Game Session AGNOSTIC Settings
        [SerializeField] private float _delayBetweenShots = 0.2f;
        [SerializeField] private NetworkPrefabRef _bullet = NetworkPrefabRef.Empty;

        // Local Runtime references
        private Rigidbody _rigidbody = null;
        private PlayerController _playerController = null;
        private PlayerMovementController _playerMovementController = null;
        private Inventory _inventory = null;
        private GameObject _spawnerObject = null;

        // Game Session SPECIFIC Settings
        [Networked] private NetworkButtons _buttonsPrevious { get; set; }
        [Networked] private TickTimer _shootCooldown { get; set; }

        public override void Spawned()
        {
            // --- Host & Client
            // Set the local runtime references.
            _rigidbody = GetComponent<Rigidbody>();
            _playerController = GetComponent<PlayerController>();
            _playerMovementController = GetComponent<PlayerMovementController>();
            _inventory = GetComponentInChildren<Inventory>();
            _spawnerObject = FindObjectOfType<PlayerSpawner>().gameObject;
        }

        public override void FixedUpdateNetwork()
        {
            // Bail out of FUN() if this player does not currently accept input
            if (_playerController.AcceptInput == false) return;

            // Bail out of FUN() if this Client does not have InputAuthority over this object or
            // if no PlayerInput struct is available for this tick
            if (GetInput<PlayerInput>(out var input) == false) return;

            Fire(input);
        }

        // Checks the Buttons in the input struct against their previous state to check
        // if the fire button was just pressed.
        private void Fire(PlayerInput input)
        {
            if (input.Buttons.WasPressed(_buttonsPrevious, PlayerButtons.Fire))
            {
                SpawnBullet();
            }

            else if (input.Buttons.WasPressed(_buttonsPrevious, PlayerButtons.UseItem))
            {
                _inventory.UseItem();
            }

            _buttonsPrevious = input.Buttons;
        }

        // Spawns a bullet which will be travelling in the direction the player is facing
        private void SpawnBullet()
        {
            if (_shootCooldown.ExpiredOrNotRunning(Runner) == false || !Runner.CanSpawn) return;

            var rot = _playerMovementController._lastDirection;
            var bullet = Runner.Spawn(_bullet, _rigidbody.position,
                Quaternion.FromToRotation(Vector3.forward, rot), Object.InputAuthority);

            bullet.transform.SetParent(_spawnerObject.transform);

            // Set initial direction of bullet
            if (bullet != null)
            {
                bullet.gameObject.GetComponent<BulletBehaviour>()._direction = rot.normalized;
            }

            _shootCooldown = TickTimer.CreateFromSeconds(Runner, _delayBetweenShots);
        }
    }
}
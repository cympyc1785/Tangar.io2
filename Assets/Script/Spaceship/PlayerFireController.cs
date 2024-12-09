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

        // Game Session SPECIFIC Settings
        [Networked] private NetworkButtons _buttonsPrevious { get; set; }
        [Networked] private TickTimer _shootCooldown { get; set; }

        public override void Spawned()
        {
            // --- Host & Client
            // Set the local runtime references.
            _rigidbody = GetComponent<Rigidbody>();
            _playerController = GetComponent<PlayerController>();
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

            _buttonsPrevious = input.Buttons;
        }

        // Spawns a bullet which will be travelling in the direction the player is facing
        private void SpawnBullet()
        {
            if (_shootCooldown.ExpiredOrNotRunning(Runner) == false || !Runner.CanSpawn) return;

            Runner.Spawn(_bullet, _rigidbody.position, _rigidbody.rotation, Object.InputAuthority);

            _shootCooldown = TickTimer.CreateFromSeconds(Runner, _delayBetweenShots);
        }
    }
}
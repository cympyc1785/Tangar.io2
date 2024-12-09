using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.LagCompensation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tangar.io
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private int _points = 5;

        // Game Session AGNOSTIC Settings
        [SerializeField] private float _respawnDelay = 4.0f;
        [SerializeField] private float _playerDamageRadius = 2.5f;
        [SerializeField] private LayerMask _tanmakCollisionLayer;

        // Local Runtime references
        private ChangeDetector _changeDetector;
        private Rigidbody _rigidbody = null;
        private PlayerDataNetworked _playerDataNetworked = null;
        private PlayerVisualController _visualController = null;

        private List<LagCompensatedHit> _lagCompensatedHits = new List<LagCompensatedHit>();

        // Game Session SPECIFIC Settings
        public bool AcceptInput => _isAlive && Object.IsValid;

        [Networked] public NetworkBool _isAlive { get; private set; }

        [Networked] private TickTimer _respawnTimer { get; set; }

        public override void Spawned()
        {
            // --- Host & Client
            // Set the local runtime references.
            _rigidbody = GetComponent<Rigidbody>();
            _playerDataNetworked = GetComponent<PlayerDataNetworked>();
            _visualController = GetComponent<PlayerVisualController>();
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            _visualController.SetColorFromPlayerID(Object.InputAuthority.PlayerId);

            // --- Host
            // The Game Session SPECIFIC settings are initialized
            if (Object.HasStateAuthority == false) return;
            _isAlive = true;

            
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
            {
                switch (change)
                {
                    case nameof(_isAlive):
                        var reader = GetPropertyReader<NetworkBool>(nameof(_isAlive));
                        var (previous, current) = reader.Read(previousBuffer, currentBuffer);
                        ToggleVisuals(previous, current);
                        break;
                }
            }
        }

        private void ToggleVisuals(bool wasAlive, bool isAlive)
        {
            // Check if the player was just brought to life
            if (wasAlive == false && isAlive == true)
            {
                _visualController.TriggerSpawn();
            }
            // or whether it just got destroyed.
            else if (wasAlive == true && isAlive == false)
            {
                _visualController.TriggerDestruction();
            }
        }

        public override void FixedUpdateNetwork()
        {
            // Checks if the player is ready to be respawned.
            if (_respawnTimer.Expired(Runner))
            {
                _isAlive = true;
                _respawnTimer = default;
            }

            // Checks if the player got hit by an tanmak
            if (_isAlive && HasHitTanmak())
            {
                Debug.Log("Got Point");
            }
        }

        // Check tanmak collision using a lag compensated OverlapSphere
        private bool HasHitTanmak()
        {
            _lagCompensatedHits.Clear();

            var count = Runner.LagCompensation.OverlapSphere(_rigidbody.position, _playerDamageRadius,
                Object.InputAuthority, _lagCompensatedHits,
                _tanmakCollisionLayer.value);

            if (count <= 0) return false;

            _lagCompensatedHits.SortDistance();

            var tanmakBehaviour = _lagCompensatedHits[0].GameObject.GetComponent<TanmakBehaviour>();
            if (tanmakBehaviour.IsAlive == false)
                return false;

            tanmakBehaviour.HitPlayer(PlayerRef.None);

            return true;
        }

        // Toggle the _isAlive boolean if the player was hit and check whether the player has any lives left.
        // If they do, then the _respawnTimer is activated.
        public void PlayerWasHit(PlayerRef player)
        {
            // Ignore my bullet
            if (player == Object.InputAuthority) return;

            _isAlive = false;

            ResetPlayer();

            if (Object.HasStateAuthority == false) return;

            // Give point for killing
            if (Runner.TryGetPlayerObject(player, out var playerNetworkObject))
            {
                playerNetworkObject.GetComponent<PlayerDataNetworked>().AddToScore(_points);
            }

            _respawnTimer = TickTimer.CreateFromSeconds(Runner, _respawnDelay);
        }

        // Resets the players movement velocity
        private void ResetPlayer()
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
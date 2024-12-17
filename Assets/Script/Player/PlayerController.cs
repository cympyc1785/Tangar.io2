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
        [SerializeField] private float _playerDamageRadius = 5.0f;
        [SerializeField] private LayerMask _tanmakCollisionLayer;
        [SerializeField] private LayerMask _itemCollisionLayer;
        [SerializeField] private GameObject _model;
        [SerializeField] private GameObject _firingIndicator;
        [SerializeField] private float _indicatorOffset = 7.0f;


        // Local Runtime references
        private ChangeDetector _changeDetector;
        private Rigidbody _rigidbody = null;
        private SphereCollider _sphereCollider = null;
        private HitboxRoot _hitBoxRoot = null;
        private Hitbox _hitBox = null;
        private PlayerDataNetworked _playerDataNetworked = null;
        private PlayerVisualController _visualController = null;
        private Inventory _inventory = null;

        private List<LagCompensatedHit> _lagCompensatedHits = new List<LagCompensatedHit>();

        

        // Game Session SPECIFIC Settings
        public bool AcceptInput => _isAlive && Object.IsValid;

        [Networked] public NetworkBool _isAlive { get; private set; }
        [Networked] private TickTimer _respawnTimer { get; set; }
        [Networked] private Vector3 _networkScale { get; set; }

        private float _minScale = 5.0f;
        private float _maxScale = 20.0f;
        [SerializeField] private float _scaleFactor = 2.0f;

        private Vector3 _lastMovementDirection = Vector3.forward;

        public override void Spawned()
        {
            // --- Host & Client
            // Set the local runtime references.
            _rigidbody = GetComponent<Rigidbody>();
            _sphereCollider = GetComponent<SphereCollider>();
            _playerDataNetworked = GetComponent<PlayerDataNetworked>();
            _visualController = GetComponent<PlayerVisualController>();
            _hitBoxRoot = GetComponent<HitboxRoot>();
            _hitBox = GetComponent<Hitbox>();
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            _inventory = GetComponent<Inventory>();

            _visualController.SetColorFromPlayerID(Object.InputAuthority.PlayerId);

            // --- Host
            // The Game Session SPECIFIC settings are initialized
            if (Object.HasStateAuthority == false) return;
            _isAlive = true;

            //UpdateIndicator();
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
                    case nameof(_networkScale):
                        _model.transform.localScale = _networkScale;
                        _sphereCollider.radius = _networkScale.x / 2.0f;
                        _playerDamageRadius = _networkScale.x / 2.0f;
                        _hitBoxRoot.BroadRadius = _networkScale.x / 2.0f;
                        _hitBox.SphereRadius = _networkScale.x / 2.0f;
                        //_indicatorOffset = _networkScale.x / 1.6f;
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
                // Point Added
                // Grow
                UpdateSize();
            }

            if (_isAlive && HasHitItem())
            {
                // item get effect?
            }
            //UpdateIndicator();
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

            tanmakBehaviour.HitPlayer(Object.InputAuthority);

            return true;
        }

        private bool HasHitItem()
        {
            _lagCompensatedHits.Clear();

            var count = Runner.LagCompensation.OverlapSphere(_rigidbody.position, _playerDamageRadius,
                Object.InputAuthority, _lagCompensatedHits,
                _itemCollisionLayer.value);

            if (count <= 0) return false;

            _lagCompensatedHits.SortDistance();

            var fieldItem = _lagCompensatedHits[0].GameObject.GetComponent<FieldItem>();
            if (fieldItem.IsAlive == false)
                return false;

            _inventory.AddItem(fieldItem.ItemPrefab);

            fieldItem.GotItem(Object.InputAuthority);

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

            _playerDataNetworked.ResetScore();
            _networkScale = new Vector3(_minScale, 1, _minScale);
        }

        private void UpdateSize()
        {
            float scale = Mathf.Clamp(_minScale + _playerDataNetworked.Score * _scaleFactor, _minScale, _maxScale);
            int score = _playerDataNetworked.Score;
            Debug.Log(score);
            _networkScale = new Vector3(scale, 1, scale);
        }
        //private void UpdateIndicator()
        //{
        //    if (_firingIndicator == null) return;

        //    // Get the current movement direction
        //    Vector3 movementDirection = _rigidbody.velocity.normalized;

        //    // If the player is moving, update the last valid movement direction
        //    if (movementDirection != Vector3.zero)
        //    {
        //        _lastMovementDirection = movementDirection;
        //    }

        //    // Adjust the indicator's position based on the last movement direction
        //    _firingIndicator.transform.position = transform.position + _lastMovementDirection * _indicatorOffset;

        //    // Update the indicator's rotation to point in the last movement direction
        //    _firingIndicator.transform.rotation = Quaternion.LookRotation(_lastMovementDirection, Vector3.up);
        //}

    }
}
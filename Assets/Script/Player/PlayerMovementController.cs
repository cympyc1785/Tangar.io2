using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

namespace Tangar.io
{
    // The class is dedicated to controlling the Player's movement
    public class PlayerMovementController : NetworkBehaviour
    {
        // Game Session AGNOSTIC Settings
        // [SerializeField] private float _rotationSpeed = 90.0f;
        [SerializeField] private float _movementSpeed = 3000.0f;
        [SerializeField] private float _maxSpeed = 500.0f;

        // Local Runtime references
        private Rigidbody
            _rigidbody =
                null; // The Unity Rigidbody (RB) is automatically synchronised across the network thanks to the NetworkRigidbody (NRB) component.

        private PlayerController _playerController = null;

        // Game Session SPECIFIC Settings
        [Networked] private float _screenBoundaryX { get; set; }
        [Networked] private float _screenBoundaryY { get; set; }

        [Networked] public Vector3 _lastDirection { get; private set; }

        public override void Spawned()
        {
            // --- Host & Client
            // Set the local runtime references.
            _rigidbody = GetComponent<Rigidbody>();
            _playerController = GetComponent<PlayerController>();

            // --- Host
            // The Game Session SPECIFIC settings are initialized
            if (Object.HasStateAuthority == false) return;

            _screenBoundaryX = Camera.main.orthographicSize * Camera.main.aspect;
            _screenBoundaryY = Camera.main.orthographicSize;

            _lastDirection = Vector3.forward;
        }

        public override void FixedUpdateNetwork()
        {
            // Bail out of FUN() if this player does not currently accept input
            if (_playerController.AcceptInput == false) return;

            // GetInput() can only be called from NetworkBehaviours.
            // In SimulationBehaviours, either TryGetInputForPlayer<T>() or GetInputForPlayer<T>() has to be called.
            // This will only return true on the Client with InputAuthority for this Object and the Host.
            if (Runner.TryGetInputForPlayer<PlayerInput>(Object.InputAuthority, out var input))
            {
                Move(input);
            }

            CheckExitScreen();
        }

        // Moves the player RB using the input for the client with InputAuthority over the object
        private void Move(PlayerInput input)
        {
            //Quaternion rot = _rigidbody.rotation *
            //                 Quaternion.Euler(0, input.HorizontalInput * _rotationSpeed * Runner.DeltaTime, 0);
            //_rigidbody.MoveRotation(rot);

            //Vector3 force = (rot * Vector3.forward) * input.VerticalInput * _movementSpeed * Runner.DeltaTime;
            //_rigidbody.AddForce(force);

            // Calculate direction
            Vector3 direction = (transform.forward * input.VerticalInput + transform.right * input.HorizontalInput);
            direction.Normalize();

            // Apply direct translation
            _rigidbody.velocity = direction * _movementSpeed * Runner.DeltaTime;

            if (_rigidbody.velocity.magnitude > _maxSpeed)
            {
                _rigidbody.velocity = _rigidbody.velocity.normalized * _maxSpeed;
            }

            if (input.VerticalInput != 0 || input.HorizontalInput != 0)
            {
                _lastDirection = direction;
            }
        }

        // Moves the ship to the opposite side of the screen if it exits the screen boundaries.
        private void CheckExitScreen()
        {
            var position = _rigidbody.position;

            if (Mathf.Abs(position.x) < _screenBoundaryX && Mathf.Abs(position.z) < _screenBoundaryY) return;

            if (Mathf.Abs(position.x) > _screenBoundaryX)
            {
                position = new Vector3(-Mathf.Sign(position.x) * _screenBoundaryX, 0, position.z);
            }

            if (Mathf.Abs(position.z) > _screenBoundaryY)
            {
                position = new Vector3(position.x, 0, -Mathf.Sign(position.z) * _screenBoundaryY);
            }

            position -= position.normalized *
                        0.1f; // offset a little bit to avoid looping back & forth between the 2 edges 
            GetComponent<NetworkRigidbody3D>().Teleport(position);
        }
    }
}
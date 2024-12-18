using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace Tangar.io
{
    // The TanmakSpawner does not execute any behaviour on the clients.
    // Therefore all of its parameters can remained local and not networked.
    public class TanmakSpawner : NetworkBehaviour
    {
        // The Network Object prefabs for small and big asteroids.
        // Using NetworkPrefabRef restricts the parameters to Prefabs which carry a NetworkObject component.
        [SerializeField] private NetworkPrefabRef _tanmakPrefab = NetworkPrefabRef.Empty;

        // The minimum and maximum amount of time between each big asteroid spawn.
        [SerializeField] private float _minSpawnDelay = 1.0f;
        [SerializeField] private float _maxSpawnDelay = 2.0f;
        [SerializeField] private float _maxSpeed = 2000.0f;

        // The TickTimer controls the time lapse between spawns.
        private TickTimer _spawnDelay;

        // The spawn boundaries are based of the camera settings
        private float _screenBoundaryX = 250.0f;
        private float _screenBoundaryY = 250.0f;

        private List<NetworkId> _tanmaks = new List<NetworkId>();

        // The spawner is started when the GameStateController switches to GameState.Running.
        public void StartTanmakSpawner()
        {
            if (Object.HasStateAuthority == false) return;

            // Triggers the delay until the first spawn.
            SetSpawnDelay();
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority == false) return;

            SpawnTanmak();

            CheckOutOfBoundsTanmaks();
        }

        private void SpawnTanmak()
        {
            if (_spawnDelay.Expired(Runner) == false) return;

            Vector2 direction = Random.insideUnitCircle;
            Vector3 position = Vector3.zero;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Make it appear on the left/right side
                position = new Vector3(Mathf.Sign(direction.x) * _screenBoundaryX, 0, direction.y * _screenBoundaryY);
            }
            else
            {
                // Make it appear on the top/bottom
                position = new Vector3(direction.x * _screenBoundaryX, 0, Mathf.Sign(direction.y) * _screenBoundaryY);
            }

            // Offset slightly so we are not out of screen at creation time (as it would destroy the asteroid right away)
            position -= position.normalized * 0.1f;

            var tanmak = Runner.Spawn(_tanmakPrefab, position,
                onBeforeSpawned: FireTanmak);

            tanmak.transform.rotation = Quaternion.Euler(90, 0, 0);

            tanmak.transform.SetParent(transform);

            _tanmaks.Add(tanmak.Id);

            // Sets the delay until the next spawn.
            SetSpawnDelay();
        }

        private void SetSpawnDelay()
        {
            // Chose a random amount of time until the next spawn.
            var time = Random.Range(_minSpawnDelay, _maxSpawnDelay);
            _spawnDelay = TickTimer.CreateFromSeconds(Runner, time);
        }

        // Fire Tanmak
        private void FireTanmak(NetworkRunner runner, NetworkObject tanmakNetworkObject)
        {
            Vector3 force = -tanmakNetworkObject.transform.position.normalized * 1000.0f;

            if (force.magnitude > _maxSpeed)
            {
                force = force.normalized * _maxSpeed;
            }

            var rb = tanmakNetworkObject.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.AddForce(force);

            var tanmakBehaviour = tanmakNetworkObject.GetComponent<TanmakBehaviour>();
        }

        // Checks whether any asteroid left the game boundaries. If it has, the asteroid gets despawned.
        private void CheckOutOfBoundsTanmaks()
        {
            for (int i = 0; i < _tanmaks.Count; i++)
            {
                // Checks if an asteroid is still exists.
                if (Runner.TryFindObject(_tanmaks[i], out var asteroid) == false)
                {
                    _tanmaks.RemoveAt(i);
                    i--;
                    continue;
                }

                if (IsWithinScreenBoundary(asteroid.transform.position)) continue;

                Runner.Despawn(asteroid);
                i--;
            }
        }

        // Checks whether a position is inside the screen boundaries
        private bool IsWithinScreenBoundary(Vector3 tanmakPosition)
        {
            return Mathf.Abs(tanmakPosition.x) < _screenBoundaryX && Mathf.Abs(tanmakPosition.z) < _screenBoundaryY;
        }
    }
}

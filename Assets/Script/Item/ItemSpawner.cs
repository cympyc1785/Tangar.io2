using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace Tangar.io
{
    // The ItemSpawner does not execute any behaviour on the clients.
    // Therefore all of its parameters can remained local and not networked.
    public class ItemSpawner : NetworkBehaviour
    {
        [SerializeField] private List<NetworkPrefabRef> _fieldItemPrefabs = new List<NetworkPrefabRef>();

        [SerializeField] private float _minSpawnDelay = 1.0f;
        [SerializeField] private float _maxSpawnDelay = 2.0f;
        [SerializeField] private int _maxSpawnCount = 10;

        private TickTimer _spawnDelay;

        private float _screenBoundaryX = 250.0f;
        private float _screenBoundaryY = 250.0f;

        private List<NetworkId> _fieldItemIds = new List<NetworkId>();

        // The spawner is started when the GameStateController switches to GameState.Running.
        public void StartItemSpawner()
        {
            if (Object.HasStateAuthority == false) return;

            // Triggers the delay until the first spawn.
            SetSpawnDelay();

        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority == false) return;

            SpawnFieldItem();
        }

        public void DespawnItem(NetworkId id)
        {
            // Remove for counting
            _fieldItemIds.Remove(id);
        }

        private void SpawnFieldItem()
        {
            if (_spawnDelay.Expired(Runner) == false) return;

            // Check if item count is beyond maximums. If then abort
            UpdateItems();

            if (_fieldItemIds.Count >= _maxSpawnCount) return;

            Vector3 randPos = new Vector3(
                Random.Range(-_screenBoundaryX, _screenBoundaryX),
                0,
                Random.Range(-_screenBoundaryY, _screenBoundaryY));

            int randItemIdx = Random.Range(0, _fieldItemPrefabs.Count);

            var fieldItemObject = Runner.Spawn(_fieldItemPrefabs[randItemIdx], randPos);

            fieldItemObject.transform.SetParent(transform);

            // Register for counting
            _fieldItemIds.Add(fieldItemObject);

            // Sets the delay until the next spawn.
            SetSpawnDelay();
        }

        // Update fieldItemIds list
        private void UpdateItems()
        {
            for (int i = 0; i < _fieldItemIds.Count; i++)
            {
                if (Runner.TryFindObject(_fieldItemIds[i], out var item) == false)
                {
                    _fieldItemIds.RemoveAt(i);
                    i--;
                }
            }
        }

        private void SetSpawnDelay()
        {
            // Chose a random amount of time until the next spawn.
            var time = Random.Range(_minSpawnDelay, _maxSpawnDelay);
            _spawnDelay = TickTimer.CreateFromSeconds(Runner, time);
        }
    }
}

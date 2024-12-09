using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Tangar.io
{
    // A purely utilitarian class which manages the display of player information (Nickname, Lives and Score)
    // All methods are called from PlayerDataNetworked when a change is detected there.
    public class PlayerOverviewPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerOverviewEntryPrefab = null;

        private Dictionary<PlayerRef, TextMeshProUGUI>
            _playerListEntries = new Dictionary<PlayerRef, TextMeshProUGUI>();

        private Dictionary<PlayerRef, string> _playerNickNames = new Dictionary<PlayerRef, string>();
        private Dictionary<PlayerRef, int> _playerScores = new Dictionary<PlayerRef, int>();

        // Creates a new Overview Entry
        public void AddEntry(PlayerRef playerRef, PlayerDataNetworked playerDataNetworked)
        {
            if (_playerListEntries.ContainsKey(playerRef)) return;
            if (playerDataNetworked == null) return;

            var entry = Instantiate(_playerOverviewEntryPrefab, this.transform);
            entry.transform.localScale = Vector3.one;
            entry.color = PlayerVisualController.GetColor(playerRef.PlayerId);

            string nickName = String.Empty;
            int score = 0;

            _playerNickNames.Add(playerRef, nickName);
            _playerScores.Add(playerRef, score);

            _playerListEntries.Add(playerRef, entry);

            UpdateEntry(playerRef, entry);
        }

        // Removes an existing Overview Entry
        public void RemoveEntry(PlayerRef playerRef)
        {
            if (_playerListEntries.TryGetValue(playerRef, out var entry) == false) return;

            if (entry != null)
            {
                Destroy(entry.gameObject);
            }

            _playerNickNames.Remove(playerRef);
            _playerScores.Remove(playerRef);

            _playerListEntries.Remove(playerRef);
        }

        public void UpdateScore(PlayerRef player, int score)
        {
            if (_playerListEntries.TryGetValue(player, out var entry) == false) return;

            _playerScores[player] = score;
            UpdateEntry(player, entry);
        }

        public void UpdateNickName(PlayerRef player, string nickName)
        {
            if (_playerListEntries.TryGetValue(player, out var entry) == false) return;

            _playerNickNames[player] = nickName;
            UpdateEntry(player, entry);
        }

        private void UpdateEntry(PlayerRef player, TextMeshProUGUI entry)
        {
            var nickName = _playerNickNames[player];
            var score = _playerScores[player];

            entry.text = $"{nickName}\nScore: {score}";
        }
    }
}
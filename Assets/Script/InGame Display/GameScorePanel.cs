using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

namespace Tangar.io
{
    public class RankInfo
    {
        public string nickname;
        public int score;
    }

    public class GameScorePanel : NetworkBehaviour
    {
        Dictionary<PlayerRef, RankInfo> _rankDic = new Dictionary<PlayerRef, RankInfo>();
        List<RankInfo> _rankList = new List<RankInfo>();

        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private TMP_Text _nickText;
        [SerializeField] private TMP_Text _scoreText;

        // Add new player info entry
        // sort : option for sort after modification
        public void AddEntry(PlayerRef playerRef, PlayerDataNetworked playerData)
        {
            if (playerData == null) return;

            RankInfo rankInfo = new RankInfo
            {
                nickname = playerData.NickName.ToString(),
                score = playerData.Score
            };

            _rankDic.Add(playerRef, rankInfo);
            _rankList.Add(rankInfo);

            SortAndDisplayRankInfo();
        }

        // Removes an existing Overview Entry
        public void RemoveEntry(PlayerRef playerRef)
        {
            if (_rankDic.TryGetValue(playerRef, out var rankInfo) == false) return;

            _rankDic.Remove(playerRef);
            _rankList.Remove(rankInfo);

            SortAndDisplayRankInfo();
        }

        // Update every player info
        //public void UpdateAllRankInfo(PlayerDataNetworked playerData)
        //{
        //    _rankList.Clear();

        //    foreach (var playerRef in Runner.ActivePlayers)
        //    {
        //        UpdateRankInfo(playerRef, playerData);
        //    }

        //    SortAndDisplayRankInfo();
        //}

        // Update player info and SortAndDisplay
        public void UpdateRankInfo(PlayerRef playerRef, PlayerDataNetworked playerData)
        {
            if (playerData == null) return;

            if (_rankDic.ContainsKey(playerRef))
            {
                _rankDic[playerRef].nickname = playerData.NickName.ToString();
                _rankDic[playerRef].score = playerData.Score;
            }
            else
            {
                AddEntry(playerRef, playerData);
            }

            SortAndDisplayRankInfo();
        }

        // Sort rank list by score in descending order and display
        private void SortAndDisplayRankInfo()
        {
            _rankList.Sort((A, B) => B.score.CompareTo(A.score));

            DisplayRankInfo();
        }

        private void DisplayRankInfo()
        {
            _rankText.text = "";
            _nickText.text = "";
            _scoreText.text = "";

            for (int i = 0; i < _rankList.Count; i++)
            {
                _rankText.text += (i + 1).ToString() + "\n";
                _nickText.text += _rankList[i].nickname + "\n";
                _scoreText.text += _rankList[i].score.ToString() + "\n";
            }
        }
    }
}


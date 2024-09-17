using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class PlayersScoreRootUI : MonoBehaviour
    {
        [SerializeField] private PlayerScoreUI m_playerScorePrefab = null;

        private PlayerScoreUI [] m_playerScoreUiList = new PlayerScoreUI [GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE];

        const float LOCATION_AREA_WIDTH = 1920 - 80;    // 配置エリア幅 

        private int[] m_teamScore = new int[GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE];

        private Animator m_animator = null;

        class PlayerScore
        {
            public int m_playerNo = 0;
            public int m_score = 0;
        }

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }



        /// <summary>
        /// プレイヤー情報を登録する 
        /// </summary>
        /// <param name="playerNo"></param>
        /// <param name="comPlayer"></param>
        /// <param name="baseColor"></param>
        public void Entry(int playerNo, ComPlayerBase comPlayer, Color baseColor)
        {
            var instance = Instantiate(m_playerScorePrefab, this.transform);

            // 配置座標 
            float oneWidth = LOCATION_AREA_WIDTH / GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE;
            float locationX = -LOCATION_AREA_WIDTH / 2.0f + oneWidth*0.5f
                + oneWidth * playerNo;
            instance.Setup(playerNo, comPlayer, locationX);
            m_playerScoreUiList[playerNo] = instance;

            // リセット 
            m_teamScore[playerNo] = 0;
        }


        /// <summary>
        /// 新しいスコア表示設定 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="price"></param>
        /// <param name="menuName"></param>
        public void SetNewScore(int teamId, int price, string menuName, int totalPrice)
        {
            if (0 < price)
            {
                m_teamScore[teamId] = totalPrice;
                m_playerScoreUiList[teamId].UpdateNewScore(price, menuName, totalPrice);

                // 順位をソート 
                List<PlayerScore> playerScoreList = new();
                for (int i=0; i < m_teamScore.Length; ++i)
                {
                    playerScoreList.Add(new PlayerScore
                    {
                        m_playerNo = i,
                        m_score = m_teamScore[i],
                    });
                }
                playerScoreList.Sort((a, b) => b.m_score - a.m_score);

                // 順位を割り当て 
                int rank = 0;
                int lastScore = playerScoreList[0].m_score;
                for (int i=0; i < playerScoreList.Count; ++i)
                {
                    var playerScore = playerScoreList[i];
                    if (playerScore.m_score != lastScore)
                    {
                        rank = i;
                        lastScore = playerScore.m_score;
                    }
                    m_playerScoreUiList[playerScore.m_playerNo].SetRank(rank);
                }

            }
        }



        public void Enter()
        {
            m_animator.SetTrigger("Enter");
        }
        public void Leave()
        {
            m_animator.SetTrigger("Leave");
        }

    }


}


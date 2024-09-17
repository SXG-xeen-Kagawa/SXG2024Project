using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace SXG2024
{

    public class PlayerScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_playerNameText = null;
        [SerializeField] private TextMeshProUGUI m_priceText = null;
        [SerializeField] private Image m_faceImage = null;
        [SerializeField] private CanvasGroup m_canvasGroup = null;
        
        [SerializeField] private Image m_battleNameBaseImage = null;
        [SerializeField] private Image m_playerIconBaseImage = null;
        [SerializeField] private Image m_battleRankImage = null;

        [SerializeField] private Sprite[] m_battleNameBaseSprites;
        [SerializeField] private Sprite[] m_playerIconBaseSprites;
        [SerializeField] private Sprite[] m_battleRankSprites;

        private int m_playerNo = 0;
        private int m_currentPrice = 0;
        private int m_lastRank = int.MaxValue;

        private RectTransform m_rectTr = null;
        private Animator m_animator = null;


        private void Awake()
        {
            m_rectTr = GetComponent<RectTransform>();
            m_animator = GetComponent<Animator>();
        }



        /// <summary>
        /// セットアップ 
        /// </summary>
        /// <param name="playerNo"></param>
        /// <param name="comPlayer"></param>
        /// <param name="locationX"></param>
        public void Setup(int playerNo, ComPlayerBase comPlayer, float locationX)
        {
            m_playerNo = playerNo;

            // 配置座標 
            Vector2 location = m_rectTr.anchoredPosition;
            location.x = locationX;
            m_rectTr.anchoredPosition = location;

            // 下地の色設定 
            m_battleNameBaseImage.sprite = m_battleNameBaseSprites[playerNo];
            m_playerIconBaseImage.sprite = m_playerIconBaseSprites[playerNo];

            // 名前 
            m_playerNameText.text = comPlayer.YourName;

            // 顔画像 
            m_faceImage.sprite = comPlayer.FaceImage;

            // 価格 
            m_currentPrice = 0;
            m_priceText.text = GetPriceText(m_currentPrice);

            // 順位表示設定 
            SetRank(-1);
        }

        /// <summary>
        /// ランク表示設定 
        /// </summary>
        /// <param name="newRank"></param>
        public void SetRank(int newRank)
        {
            if (m_lastRank != newRank)
            {
                if (newRank < 0)
                {
                    m_battleRankImage.enabled = false;
                } else
                {
                    m_battleRankImage.enabled = true;
                    m_battleRankImage.sprite = m_battleRankSprites[newRank];
                    // ランクアップする時はアニメーション 
                    if (newRank < m_lastRank || (m_lastRank < 0 && newRank == 0))
                    {
                        m_animator.SetTrigger("Update");
                    }
                }
                m_lastRank = newRank;
            }
        }


        /// <summary>
        /// 価格のテキスト 
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        private string  GetPriceText(int price)
        {
            return string.Format("{0}円", price.ToString("N0"));
        }

        /// <summary>
        /// 新しいスコアで更新する 
        /// </summary>
        /// <param name="price"></param>
        /// <param name="menuName"></param>
        /// <param name="totalPrice"></param>
        public void UpdateNewScore(int price, string menuName, int totalPrice)
        {
            StopAllCoroutines();
            StartCoroutine(CoUpdateScore(totalPrice));
        }

        private IEnumerator CoUpdateScore(int newTotalPrice)
        {
            const float ANIMATION_TIME = 0.5f;
            int startPrice = m_currentPrice;
            float time = 0;
            while (time < ANIMATION_TIME)
            {
                time += Time.deltaTime;

                m_currentPrice = (int)Mathf.Lerp(startPrice, newTotalPrice, time / ANIMATION_TIME);
                m_priceText.text = GetPriceText(m_currentPrice);

                yield return null;
            }
            m_currentPrice = newTotalPrice;
            m_priceText.text = GetPriceText(m_currentPrice);
        }

    }


}


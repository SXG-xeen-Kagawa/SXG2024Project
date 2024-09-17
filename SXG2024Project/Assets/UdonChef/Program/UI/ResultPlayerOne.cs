using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SXG2024
{

    public class ResultPlayerOne : MonoBehaviour
    {
        [SerializeField] private RawImage m_renderTextureImage = null;
        [SerializeField] private NamePlateUI m_namePlate = null;
        [SerializeField] private RectTransform m_gaugeTr = null;
        [SerializeField] private ResultGaugeOneChip m_resultGaugeChipPrefab = null;

        [SerializeField] private ResultScorePlate m_scorePlate = null;
        [SerializeField] private ResultRanking m_ranking = null;

        private Vector2 m_gaugePosition = Vector2.zero;
        private Vector2 m_facePosition = Vector2.zero;
        private Vector2 m_faceTargetPosition = Vector2.zero;
        private RectTransform m_faceImageTr = null;
        private int m_teamNo = 0;


        /// <summary>
        /// セットアップ 
        /// </summary>
        /// <param name="comPlayer"></param>
        /// <param name="teamColor"></param>
        public void Setup(ComPlayerBase comPlayer, int teamNo, Texture charaTexture)
        {
            m_renderTextureImage.texture = charaTexture;

            m_gaugePosition = Vector2.zero;

            m_faceImageTr = m_renderTextureImage.GetComponent<RectTransform>();
            m_facePosition = m_faceImageTr.anchoredPosition;
            m_faceTargetPosition = m_facePosition;

            m_teamNo = teamNo;

            m_namePlate.Setup(comPlayer, teamNo);
            m_scorePlate.Setup(teamNo);
        }



        /// <summary>
        /// ゲージを一つ追加 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="baseColor"></param>
        /// <param name="height"></param>
        public void AddResultGaugeOneChip(string message, Color baseColor, float height, bool isSE, 
            int price, int step,
            ResultScreen.OnGetPriceDelegate onGetCallback)
        {
            var gaugeOne = Instantiate(m_resultGaugeChipPrefab, m_gaugeTr);
            gaugeOne.Setup(message, baseColor, height,
                (count)=>
                {
                    if (count == 1)
                    {
                        m_faceTargetPosition.y += height;
                        if (isSE)
                        {
                            // SE
                            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Register);
                        }
                        // コールバック
                        if (onGetCallback != null)
                        {
                            onGetCallback.Invoke(m_teamNo, price, step);
                        }
                        // スコア加算 
                        m_scorePlate.AddScore(price);
                    }
                });

            // 表示位置調整 
            RectTransform gaugeTr = gaugeOne.GetComponent<RectTransform>();
            gaugeTr.anchoredPosition = m_gaugePosition;

            // 位置をずらす 
            m_gaugePosition.y += height;
        }



        private void Update()
        {
            if (m_facePosition.y < m_faceTargetPosition.y)
            {
                m_facePosition.y = Mathf.Lerp(m_facePosition.y, m_faceTargetPosition.y,
                    (1.0f / 0.25f) * Time.deltaTime);
                m_faceImageTr.anchoredPosition = m_facePosition;
            }
        }

        public void SetRank(int newRank)
        {
            m_ranking.SetRank(newRank);
        }


    }


}


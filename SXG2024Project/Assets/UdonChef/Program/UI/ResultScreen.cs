using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace SXG2024
{

    public delegate void OnGaugeStopDelegate(int playerId);


    public class ResultScreen : MonoBehaviour
    {
        [SerializeField] private RectTransform m_dividedRootTr = null;
        [SerializeField] private ResultPlayerOne m_resultPlayerPrefab = null;
        [SerializeField] private TextMeshProUGUI m_roundText = null;

        private List<ResultPlayerOne> m_resultPlayersList = new();

        private OnGaugeStopDelegate m_onGaugeStopCallback = null;
        private int[] m_teamScore = new int[GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE];

        public delegate void OnGetPriceDelegate(int teamNo, int price, int step);


        private void Awake()
        {
            gameObject.SetActive(false);
        }


        /// <summary>
        /// スクリーン表示開始 
        /// </summary>
        /// <param name="challegers"></param>
        public void StartScreen(ComPlayerBase[] challengers,
            Color[] teamColors, Texture[] charaTextures, ResultBook[] resultBooks, int roundCount)
        {
            // 起こす 
            gameObject.SetActive(true);

            // スコア初期化 
            for (int i=0; i < m_teamScore.Length; ++i)
            {
                m_teamScore[i] = 0;
            }

            // 参加者を作る 
            for (int i = 0; i < challengers.Length; ++i)
            {
                var challger = challengers[i];
                var resultOne = Instantiate(m_resultPlayerPrefab, m_dividedRootTr);
                resultOne.Setup(challger, i, charaTextures[i]);
                m_resultPlayersList.Add(resultOne);
            }

            // カメラをぼかす 
            CameraDoF.Instance.Change(true);

            // 「第○回戦の結果」表示
            SetRoundCount(roundCount);

            // 集計開始 
            StartCoroutine(CoStartOfTally(resultBooks, teamColors));
        }

        public void SetOnGaugeStopDelegate(OnGaugeStopDelegate callback)
        {
            m_onGaugeStopCallback = callback;
        }

        private int[] m_gaugeFinishedSteps = new int[GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE];

        /// <summary>
        /// 集計開始 
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoStartOfTally(ResultBook[] resultBooks, Color[] teamColors)
        {
            float gaugeStopEfWaitTime = 0.1f;
            IEnumerator GaugeStopCallback(int i)
            {
                yield return new WaitForSeconds(gaugeStopEfWaitTime);
                gaugeStopEfWaitTime += 0.15f;
                m_onGaugeStopCallback.Invoke(i);
            }
            const float DRAWABLE_HEIGHT = 540;   // ゲージの表示可能幅 
            const int BASE_TOTAL_PRICE = 2000;  // 基準となる合計価格

            const float GAUGE_DELAY_TIME = 0.5f;
            const float GAUGE_EACH_PLAYER_DELAY = 0.05f;

            const float BORDER_HEIGHT_ONE_LINE = 50.0f;

            // 少し遅延 
            yield return new WaitForSeconds(0.5f);

            // BGM再生
            Effect.SoundController.instance?.PlayBGM(Effect.SoundController.BGMType.Roll);

            // 最大値を求める 
            int maxPrice = 0;
            foreach (var result in resultBooks)
            {
                if (maxPrice < result.TotalPrice)
                {
                    maxPrice = result.TotalPrice;
                }
            }

            // 1円の高さを計算する 
            float totalPrice = Mathf.Max(BASE_TOTAL_PRICE, maxPrice);
            float heightPerYen = DRAWABLE_HEIGHT / totalPrice;

            // 初期化 
            for (int i=0; i < m_gaugeFinishedSteps.Length; ++i)
            {
                m_gaugeFinishedSteps[i] = int.MaxValue;
            }

            // リザルトゲージ 
            int stopFlags = 0;
            float delayTime = GAUGE_DELAY_TIME;
            for (int step=0; step < 100; ++step)
            {
                bool available = false;
                bool isSE = true;
                for (int i=0; i < m_resultPlayersList.Count; ++i)
                {
                    int price = 0;
                    string menuName = null;
                    int madePlayerId = 0;
                    if (resultBooks[i].GetData(step, out price, out menuName, out madePlayerId, true))
                    {
                        available = true;

                        // ゲージ降らせる 
                        float gaugeHeight = heightPerYen * price;
                        string message = (BORDER_HEIGHT_ONE_LINE < gaugeHeight) ?
                            string.Format("{0}\n{1}円", menuName, price) :
                            string.Format("{0} {1}円", menuName, price);
                        m_resultPlayersList[i].AddResultGaugeOneChip(message,
                            teamColors[madePlayerId], gaugeHeight, isSE, price, step, OnGetPriceCallback);

                        isSE = false;

                        // delay
                        yield return new WaitForSeconds(GAUGE_EACH_PLAYER_DELAY);
                    } else
                    {
                        if (m_onGaugeStopCallback != null)
                        {
                            if ((stopFlags & (1 << i)) == 0)
                            {
                                stopFlags |= (1 << i);
                                m_gaugeFinishedSteps[i] = step;
                                StartCoroutine(GaugeStopCallback(i));
                            }
                        }
                    }
                }
                if (!available) break;

                // 遅延。徐々に短く 
                yield return new WaitForSeconds(delayTime);
                delayTime = Mathf.Max(delayTime * 0.8f, GAUGE_EACH_PLAYER_DELAY);
            }

            // 完了 
            if (m_onGaugeStopCallback != null)
            {
                for (int i=0; i < m_resultPlayersList.Count; ++i)
                {
                    if ((stopFlags & (1 << i)) == 0)
                    {
                        m_onGaugeStopCallback.Invoke(i);
                        break;
                    }
                }
            }

            // ゲージ完了遅延 
            yield return new WaitForSeconds(2);

            // BGM再生
            Effect.SoundController.instance?.StopBGM();
            // SE
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.RollEnd);

            // 遅延 
            yield return new WaitForSeconds(1);
            // SE
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Waaa);

            // 遅延 
            yield return new WaitForSeconds(1.5f);
            // BGM再生
            Effect.SoundController.instance?.FadeInBGM(Effect.SoundController.BGMType.Title);
        }



        /// <summary>
        /// 閉じる 
        /// </summary>
        public void CloseScreen()
        {
            Animator animator = GetComponent<Animator>();
            animator.SetTrigger("Leave");
        }



        public void AnimEvent_Finish()
        {
            gameObject.SetActive(false);

            // ぼかし解除 
            CameraDoF.Instance.Change(false);
        }

        /// <summary>
        /// 「第〇回戦の結果」のテキスト表示
        /// 1～99回戦まで表示可能
        /// </summary>
        /// <param name="roundCount">第○回</param>
        private void SetRoundCount(int roundCount)
        {
            roundCount = Mathf.Clamp(roundCount, 1, 99);

            var units = new string[] { "", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            var tens = new string[] { "", "十", "二十", "三十", "四十", "五十", "六十", "七十", "八十", "九十" };

            int ten = roundCount / 10;
            int unit = roundCount % 10;

            var kansuji = string.Empty;
            if (ten == 0)
                kansuji = units[unit];
            else if (unit == 0)
                kansuji = tens[ten];
            else
                kansuji = tens[ten] + units[unit];

            m_roundText.text = $"第{kansuji}回戦の結果";
        }


        class ScoreChip
        {
            public int m_teamNo = 0;
            public int m_score = 0;
        }


        /// <summary>
        /// プライス加算のコールバック 
        /// </summary>
        /// <param name="teamNo"></param>
        /// <param name="price"></param>
        private void OnGetPriceCallback(int teamNo, int price, int step)
        {
            // 加算 
            m_teamScore[teamNo] += price;

            // 順位をソートして、結果を伝える 
            List<ScoreChip> sortScoreChips = new();
            for (int i=0; i < m_resultPlayersList.Count; ++i)
            {
                sortScoreChips.Add(new ScoreChip
                {
                    m_teamNo = i,
                    m_score = m_teamScore[i]
                });
            }
            sortScoreChips.Sort((a, b) => { return b.m_score - a.m_score; });

            int currentRank = 0;
            int lastScore = 0;
            for (int i=0; i < sortScoreChips.Count; ++i)
            {
                var scoreChip = sortScoreChips[i];
                if (scoreChip.m_score == lastScore)
                {
                    // ランクはそのまま 
                } else
                {
                    // ランクを更新 
                    currentRank = i;
                    lastScore = scoreChip.m_score;
                }
                // 最終ステップ(もう獲得が無い)場合にランキングをセットする 
                if (m_gaugeFinishedSteps[scoreChip.m_teamNo] <= step+1)
                {
                    m_resultPlayersList[scoreChip.m_teamNo].SetRank(currentRank);
                }
            }
        }

    }


}


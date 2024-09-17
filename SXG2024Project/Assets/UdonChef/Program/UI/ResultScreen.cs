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
        /// �X�N���[���\���J�n 
        /// </summary>
        /// <param name="challegers"></param>
        public void StartScreen(ComPlayerBase[] challengers,
            Color[] teamColors, Texture[] charaTextures, ResultBook[] resultBooks, int roundCount)
        {
            // �N���� 
            gameObject.SetActive(true);

            // �X�R�A������ 
            for (int i=0; i < m_teamScore.Length; ++i)
            {
                m_teamScore[i] = 0;
            }

            // �Q���҂���� 
            for (int i = 0; i < challengers.Length; ++i)
            {
                var challger = challengers[i];
                var resultOne = Instantiate(m_resultPlayerPrefab, m_dividedRootTr);
                resultOne.Setup(challger, i, charaTextures[i]);
                m_resultPlayersList.Add(resultOne);
            }

            // �J�������ڂ��� 
            CameraDoF.Instance.Change(true);

            // �u�恛���̌��ʁv�\��
            SetRoundCount(roundCount);

            // �W�v�J�n 
            StartCoroutine(CoStartOfTally(resultBooks, teamColors));
        }

        public void SetOnGaugeStopDelegate(OnGaugeStopDelegate callback)
        {
            m_onGaugeStopCallback = callback;
        }

        private int[] m_gaugeFinishedSteps = new int[GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE];

        /// <summary>
        /// �W�v�J�n 
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
            const float DRAWABLE_HEIGHT = 540;   // �Q�[�W�̕\���\�� 
            const int BASE_TOTAL_PRICE = 2000;  // ��ƂȂ鍇�v���i

            const float GAUGE_DELAY_TIME = 0.5f;
            const float GAUGE_EACH_PLAYER_DELAY = 0.05f;

            const float BORDER_HEIGHT_ONE_LINE = 50.0f;

            // �����x�� 
            yield return new WaitForSeconds(0.5f);

            // BGM�Đ�
            Effect.SoundController.instance?.PlayBGM(Effect.SoundController.BGMType.Roll);

            // �ő�l�����߂� 
            int maxPrice = 0;
            foreach (var result in resultBooks)
            {
                if (maxPrice < result.TotalPrice)
                {
                    maxPrice = result.TotalPrice;
                }
            }

            // 1�~�̍������v�Z���� 
            float totalPrice = Mathf.Max(BASE_TOTAL_PRICE, maxPrice);
            float heightPerYen = DRAWABLE_HEIGHT / totalPrice;

            // ������ 
            for (int i=0; i < m_gaugeFinishedSteps.Length; ++i)
            {
                m_gaugeFinishedSteps[i] = int.MaxValue;
            }

            // ���U���g�Q�[�W 
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

                        // �Q�[�W�~�点�� 
                        float gaugeHeight = heightPerYen * price;
                        string message = (BORDER_HEIGHT_ONE_LINE < gaugeHeight) ?
                            string.Format("{0}\n{1}�~", menuName, price) :
                            string.Format("{0} {1}�~", menuName, price);
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

                // �x���B���X�ɒZ�� 
                yield return new WaitForSeconds(delayTime);
                delayTime = Mathf.Max(delayTime * 0.8f, GAUGE_EACH_PLAYER_DELAY);
            }

            // ���� 
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

            // �Q�[�W�����x�� 
            yield return new WaitForSeconds(2);

            // BGM�Đ�
            Effect.SoundController.instance?.StopBGM();
            // SE
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.RollEnd);

            // �x�� 
            yield return new WaitForSeconds(1);
            // SE
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Waaa);

            // �x�� 
            yield return new WaitForSeconds(1.5f);
            // BGM�Đ�
            Effect.SoundController.instance?.FadeInBGM(Effect.SoundController.BGMType.Title);
        }



        /// <summary>
        /// ���� 
        /// </summary>
        public void CloseScreen()
        {
            Animator animator = GetComponent<Animator>();
            animator.SetTrigger("Leave");
        }



        public void AnimEvent_Finish()
        {
            gameObject.SetActive(false);

            // �ڂ������� 
            CameraDoF.Instance.Change(false);
        }

        /// <summary>
        /// �u��Z���̌��ʁv�̃e�L�X�g�\��
        /// 1�`99���܂ŕ\���\
        /// </summary>
        /// <param name="roundCount">�恛��</param>
        private void SetRoundCount(int roundCount)
        {
            roundCount = Mathf.Clamp(roundCount, 1, 99);

            var units = new string[] { "", "��", "��", "�O", "�l", "��", "�Z", "��", "��", "��" };
            var tens = new string[] { "", "�\", "��\", "�O�\", "�l�\", "�܏\", "�Z�\", "���\", "���\", "��\" };

            int ten = roundCount / 10;
            int unit = roundCount % 10;

            var kansuji = string.Empty;
            if (ten == 0)
                kansuji = units[unit];
            else if (unit == 0)
                kansuji = tens[ten];
            else
                kansuji = tens[ten] + units[unit];

            m_roundText.text = $"��{kansuji}���̌���";
        }


        class ScoreChip
        {
            public int m_teamNo = 0;
            public int m_score = 0;
        }


        /// <summary>
        /// �v���C�X���Z�̃R�[���o�b�N 
        /// </summary>
        /// <param name="teamNo"></param>
        /// <param name="price"></param>
        private void OnGetPriceCallback(int teamNo, int price, int step)
        {
            // ���Z 
            m_teamScore[teamNo] += price;

            // ���ʂ��\�[�g���āA���ʂ�`���� 
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
                    // �����N�͂��̂܂� 
                } else
                {
                    // �����N���X�V 
                    currentRank = i;
                    lastScore = scoreChip.m_score;
                }
                // �ŏI�X�e�b�v(�����l��������)�ꍇ�Ƀ����L���O���Z�b�g���� 
                if (m_gaugeFinishedSteps[scoreChip.m_teamNo] <= step+1)
                {
                    m_resultPlayersList[scoreChip.m_teamNo].SetRank(currentRank);
                }
            }
        }

    }


}


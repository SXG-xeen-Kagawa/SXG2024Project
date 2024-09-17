using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SXG2024
{
    public class ParticipantsList : MonoBehaviour
    {
        [SerializeField]
        RectTransform m_cursor = null;

        [SerializeField]
        List<NamePlateUI> m_participants = new();

        int m_playerIndex = 0;
        float m_cursorPosX = 0f;
        float m_cursorInterpolate = 0f;

        /// <summary>
        /// 選択中のプレイヤーインデックス
        /// </summary>
        public int PlayerIndex => m_playerIndex;

        void Awake()
        {
            Debug.Assert(m_participants.Count == 4);

            for (var i = 0; i < m_participants.Count; i++)
            {
                var participant = m_participants[i];
                participant.Setup(null, i);

                var button = participant.transform.parent.GetComponent<Button>();
                var tmp = i;
                button.onClick.AddListener(() => SetCursor(tmp));
            }

            m_cursorPosX = m_cursor.position.x;
        }

        void Update()
        {
            if (1f <= m_cursorInterpolate)
                return;

            m_cursorInterpolate += Time.deltaTime * 1.5f;
            var posX = Mathf.Lerp(m_cursor.position.x, m_cursorPosX, m_cursorInterpolate);
            m_cursor.position = new(posX, m_cursor.position.y);
        }

        public void SetCursor(int playerId)
        {
            playerId = Mathf.Clamp(playerId, 0, m_participants.Count - 1);

            var participant = m_participants[playerId];
            m_cursorPosX = participant.GetComponent<RectTransform>().position.x;
            m_playerIndex = playerId;
            m_cursorInterpolate = 0f;
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Food);
        }

        public void SetPlayer(ComPlayerBase player)
        {
            var participant = m_participants[m_playerIndex];
            participant.Setup(player, m_playerIndex);
        }
    }
}

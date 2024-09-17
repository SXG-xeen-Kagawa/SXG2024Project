using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SXG2024
{
    public class ParticipantScrollView : MonoBehaviour
    {
        [SerializeField]
        ScrollRect m_scrollRect = null;

        [SerializeField]
        GameObject m_namePanelPrefab = null;

        public void Setup(ComPlayerBase[] participants, UnityAction<int> SelectAction)
        {
            var participantCount = participants.Length;

            for (var i = 0; i < participants.Length; i++)
            {
                var participant = participants[i];

                var obj = Instantiate(m_namePanelPrefab, m_scrollRect.content.transform);

                var namePlateUI = obj.GetComponent<NamePlateUI>();
                namePlateUI.Setup(participant, 0);

                var button = obj.GetComponent<Button>();
                var tmp = i;
                button.onClick.AddListener(() => SelectAction(tmp));
            }

            // Contentサイズ計算
            var layoutGroup = m_scrollRect.content.GetComponent<GridLayoutGroup>();
            var rowCount = participantCount / layoutGroup.constraintCount + 1;
            var contentSize = m_scrollRect.content.sizeDelta;
            contentSize.y = layoutGroup.padding.top + layoutGroup.padding.bottom + 
                rowCount * layoutGroup.cellSize.y + (rowCount - 1) * layoutGroup.spacing.y;
            m_scrollRect.content.sizeDelta = contentSize;
        }
    }
}

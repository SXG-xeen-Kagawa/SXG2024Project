using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace SXG2024
{

    public class MenuIsUp3DUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI [] m_text = null;
        [SerializeField] private GameObject m_shiningObj = null;

        private RectTransform m_rectTr = null;

        private void Awake()
        {
            m_rectTr = GetComponent<RectTransform>();
        }

        public void Setup(string menuName, int price, Vector2 pos2d)
        {
            if (0 < price)
            {
                m_text[0].text = string.Format("<i>{0}<size=80%> Ç®Ç‹ÇøÅI</size></i>\n+{1}â~", menuName, price);
                m_text[0].enabled = true;
                m_text[1].enabled = false;
                m_shiningObj.SetActive(true);
            } else
            {
                m_text[1].text = string.Format("<i>í≤óùé∏îsÅIÅI</i>\n+0â~");
                m_text[0].enabled = false;
                m_text[1].enabled = true;
                m_shiningObj.SetActive(false);
            }

            m_rectTr.anchoredPosition = pos2d;
        }

        public void Setup(string menuName, int price, Canvas3DRoot.CalcScreenPositionDelegate callback)
        {
            Vector2 pos2d = callback();
            Setup(menuName, price, pos2d);
            StartCoroutine(CoUpdate(callback));
        }

        private IEnumerator CoUpdate(Canvas3DRoot.CalcScreenPositionDelegate callback)
        {
            Vector2 pos2d = Vector2.zero;
            while (true)
            {
                pos2d = callback();
                if (pos2d != Vector2.zero)
                {
                    m_rectTr.anchoredPosition = pos2d;
                    yield return null;
                } else
                {
                    break;
                }
            }
            Destroy(gameObject);
        }


        public void AnimEvent_FinishEnterAnimation()
        {
            Destroy(gameObject);
        }
    }


}


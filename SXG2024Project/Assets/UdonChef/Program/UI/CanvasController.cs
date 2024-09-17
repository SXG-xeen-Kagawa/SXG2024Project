using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class CanvasController : MonoBehaviour
    {
        private CanvasGroup m_canvasGroup = null;

        private void Awake()
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetAlpha(float alpha)
        {
            m_canvasGroup.alpha = alpha;
        }
    }


}


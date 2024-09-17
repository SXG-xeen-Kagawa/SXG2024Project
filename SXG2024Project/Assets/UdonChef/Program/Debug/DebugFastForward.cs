using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace SXG2024
{

    public class DebugFastForward : MonoBehaviour
    {
        private int m_fastforwardLevel = 0;

        private readonly float[] m_speedRateTable = { 1.0f, 2.0f, 4.0f }; 

        [SerializeField] private CanvasGroup m_canvasGroup = null;
        [SerializeField] private TextMeshProUGUI m_text = null;


        // Start is called before the first frame update
        void Start()
        {
            ChangeLevel(0);

            GameInputManager.Instance.SetCallback(GameInputManager.Type.SpeedUp, WasPressedSpeedUp);
            GameInputManager.Instance.SetCallback(GameInputManager.Type.SpeedDown, WasPressedSpeedDown);
        }

        private void ChangeLevel(int level)
        {
            m_fastforwardLevel = level;

            if (level == 0)
            {
                m_canvasGroup.alpha = 0;
                Time.timeScale = m_speedRateTable[0];
            } else if (level < m_speedRateTable.Length)
            {
                m_canvasGroup.alpha = 1;
                Time.timeScale = m_speedRateTable[level];
                m_text.text = string.Format("早送り×{0}", Time.timeScale);
            }
        }


        private void WasPressedSpeedUp()
        {
            ChangeLevel(Mathf.Min(m_fastforwardLevel + 1, m_speedRateTable.Length - 1));
        }

        private void WasPressedSpeedDown()
        {
            ChangeLevel(Mathf.Max(m_fastforwardLevel - 1, 0));
        }

    }


}


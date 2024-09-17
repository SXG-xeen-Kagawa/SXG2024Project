using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace SXG2024
{

    public class RemainingTimeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_text = null;
        private int m_prevTime = -1;

        /// <summary>
        /// éûä‘Çê›íË 
        /// </summary>
        /// <param name="time"></param>
        public void SetTime(float time)
        {
            int time_i = (int)time;
            string newTime = string.Format("{0}:{1:D2}", time_i / 60, time_i % 60);
            if (string.Compare(m_text.text, newTime) != 0)
            {
                m_text.text = newTime;
            }
            if (time_i > 0 && time_i <= 5 && m_prevTime != time_i)
            {
                // SE
                Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Count);
            }
            m_prevTime = time_i;
        }

    }


}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SXG2024
{

    /// <summary>
    /// COMの行動
    /// </summary>
    public struct ComBehaviorData
    {
        public Vector3 m_targetPosition;
        public float m_speedRate;
        public bool m_isKick;

        public void Reset()
        {
            m_targetPosition = Vector3.zero;
            m_speedRate = 0;
            m_isKick = false;
        }
    }


    public class ComBehaviour
    {
    }

}


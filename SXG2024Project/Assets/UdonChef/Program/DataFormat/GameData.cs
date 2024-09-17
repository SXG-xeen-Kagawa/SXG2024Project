using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    [CreateAssetMenu(menuName = "SXG2024/Create GameData")]
    public class GameData : ScriptableObject
    {
        [Tooltip("食材の総数")]
        //public int m_totalCountOfFoods = 50;
        [SerializeField] private int m_totalCountOfFoods = 50;

        [Tooltip("食材の初期配置数")]
        [SerializeField] private int m_initialCountOfFoods = 20;

        [Tooltip("食材の中のうどん玉の数")]
        [SerializeField] private int m_countOfNoodles = 20;

        [Tooltip("総プレイ時間")]
        [SerializeField] private float m_totalPlayTime = 2 * 60.0f;

        [Tooltip("スコアUIを消す時間")]
        [SerializeField] private float m_hideScoreUiTime = 60.0f;


        [System.Serializable]
        public class DropSpan
        {
            public float m_time;        // 時刻
            public float m_span;        // 時間間隔 
            public int m_countOfOnce;   // 一回にドロップする数 
        }
        [SerializeField] private DropSpan[] m_dropSpanTimeTable = null;


        //-------- Accessor --------

        public int TotalCountOfFoods => m_totalCountOfFoods;

        public int InitialCountOfFoods => m_initialCountOfFoods;

        public int CountOfNoodles => m_countOfNoodles;

        public float TotalPlayTime => m_totalPlayTime;

        public float HideScoreUiTime => m_hideScoreUiTime;

        public DropSpan[] DropSpanTimeTable => m_dropSpanTimeTable;

    }


}


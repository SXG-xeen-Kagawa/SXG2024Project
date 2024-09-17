using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SXG2024
{
    public static class GameConfigSetting
    {
        /// <summary>
        /// 第何回戦か
        /// </summary>
        public static int RoundCount { get; set; } = 1;

        /// <summary>
        /// 参加プレイヤーの番号
        /// </summary>
        public static int[] Participants { get; set; } = new int[4] { -1, -1, -1, -1 };
    }
}

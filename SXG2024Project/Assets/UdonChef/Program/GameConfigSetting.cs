using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SXG2024
{
    public static class GameConfigSetting
    {
        /// <summary>
        /// �扽��킩
        /// </summary>
        public static int RoundCount { get; set; } = 1;

        /// <summary>
        /// �Q���v���C���[�̔ԍ�
        /// </summary>
        public static int[] Participants { get; set; } = new int[4] { -1, -1, -1, -1 };
    }
}

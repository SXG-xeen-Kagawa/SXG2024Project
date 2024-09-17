
namespace SXG2024
{

    public class GameConstants
    {

        // 1ゲームのプレイヤー数 
        public const int MAX_PLAYER_COUNT_IN_ONE_BATTLE = 4;

        // プレイヤーが持てる食材の最大数 
        public const int MAX_NUMBER_OF_PLAYER_CAN_HAVE = 5;


        /// <summary>
        /// プレイヤーの地面
        /// </summary>
        public enum PlayerGroundType
        {
            None,
            GameField,
            Table,
            Outside,
        }

    }


}


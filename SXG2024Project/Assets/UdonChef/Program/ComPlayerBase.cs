using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class ComPlayerBase : MonoBehaviour
    {
        // 所属組織 
        [SerializeField] private string m_yourOrganization = "Please input your organization.";
        public string Organization => m_yourOrganization;

        // 名前 
        [SerializeField] private string m_yourName = "Please input your name.";
        public string YourName => m_yourName;

        // 顔画像 
        [SerializeField] private Sprite m_faceImage = null;
        public Sprite FaceImage => m_faceImage;


        private ComBehaviorData m_comBehaviourData = new();

        private UdonChefSceneManager m_gameManager = null;


        private int m_id = 0;

        public void SetPlayerData(string organization, string name, Sprite icon)
        {
            m_yourOrganization = organization;
            m_yourName = name;
            m_faceImage = icon;
        }

        public void Setup(int id, UdonChefSceneManager gameManager)
        {
            m_id = id;
            m_comBehaviourData.Reset();

            m_gameManager = gameManager;
        }


        /// <summary>
        /// [for System] ComBehaviourDataを取得、そしてリセット 
        /// </summary>
        /// <returns></returns>
        public ComBehaviorData GetComBehaviourDataAndReset()
        {
            var retval = m_comBehaviourData;
            m_comBehaviourData.Reset();
            return retval;
        }



        #region 参照用の関数群


        /// <summary>
        /// ステージ上の食材の情報を取得
        /// </summary>
        /// <returns></returns>
        protected List<FoodNowInfo> SXG_GetFoodsInfoOnStage(FoodState[] targetFoodsState=null)
        {
            return m_gameManager.GetFoodsInfoOnStage(m_id, targetFoodsState);
        }

        /// <summary>
        /// foodIdを指定して現在の状態を取得
        /// </summary>
        /// <param name="foodId"></param>
        /// <returns></returns>
        protected FoodNowInfo SXG_GetFoodInfo(int foodId)
        {
            return m_gameManager.GetFoodInfo(m_id, foodId);
        }


        /// <summary>
        /// 次に落ちてくるFoodTypeを取得(何秒後に落ちてくるのかまでは分からないようにする)
        /// </summary>
        /// <returns></returns>
        protected FoodType    SXG_GetTheNextFallingFood()
        {
            return m_gameManager.GetNextFallingFood();
        }

        /// <summary>
        /// 次以降に落ちてくるFoodTypeをすべて取得(次以降に落ちてくる順番が分かる)
        /// </summary>
        /// <returns></returns>
        protected FoodType []   SXG_GetAllFutureFoodsDrops()
        {
            return m_gameManager.GetAllFutureFoodsDrops();
        }


        /// <summary>
        /// 目標座標を指定してそこへ移動する
        /// </summary>
        /// <param name="targetPosition">目標座標</param>
        /// <param name="speedRate">0-1:移動速度率</param>
        protected void SXG_MoveToTargetPosition(Vector3 targetPosition, float speedRate)
        {
            m_comBehaviourData.m_targetPosition = targetPosition;
            m_comBehaviourData.m_speedRate = Mathf.Clamp01(speedRate);
        }

        /// <summary>
        /// 正面方向にキックする（その後、しばらく硬直します）
        /// </summary>
        protected void SXG_Kick()
        {
            m_comBehaviourData.m_isKick = true;
        }


        /// <summary>
        /// 手に持っている食材のリストを返す 
        /// </summary>
        /// <returns></returns>
        protected IList<FoodType>    SXG_GetMyFoodsListOnHand()
        {
            return m_gameManager.GetFoodsListOnHand(m_id);
        }

        /// <summary>
        /// あと何個持てる？ 
        /// </summary>
        /// <returns></returns>
        protected int   SXG_HowManyMoreCanIHave()
        {
            return GameConstants.MAX_NUMBER_OF_PLAYER_CAN_HAVE - SXG_GetMyFoodsListOnHand().Count;
        }


        /// <summary>
        /// 自身の座標と角度を取得 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        protected void  SXG_GetPositionAndRotation(out Vector3 position, out Quaternion rotation)
        {
            m_gameManager.GetLocalPlayerPositionAndRotation(
                m_id, out position, out rotation);
        }


        /// <summary>
        /// 自身の移動速度を取得
        /// </summary>
        /// <returns></returns>
        protected Vector3 SXG_GetVelocity()
        {
            return m_gameManager.GetPlayerLocalVelocity(m_id);
        }


        /// <summary>
        /// キャラクターは地面に接地しているか？ 
        /// </summary>
        /// <returns></returns>
        protected bool  SXG_IsOnGround()
        {
            return m_gameManager.IsPlayerOnGround(m_id);
        }

        /// <summary>
        /// 食材を指定して価格を取得する
        /// </summary>
        /// <param name="foods"></param>
        /// <returns></returns>
        protected int   SXG_GetPriceOfFoods(FoodType[] foods)
        {
            return m_gameManager.GetPriceOfFoods(foods);
        }

        /// <summary>
        /// 今、手に持っている食材の出荷価格を取得する 
        /// </summary>
        /// <returns></returns>
        protected int   SXG_GetNowPriceOnHand()
        {
            var foodsList = SXG_GetMyFoodsListOnHand();
            if (0 < foodsList.Count)
            {
                FoodType[] foods = new FoodType[foodsList.Count];
                foodsList.CopyTo(foods, 0);
                return SXG_GetPriceOfFoods(foods);
            }
            return 0;
        }


        protected enum RaycastTarget
        {
            Everythings,
            Player,
            Stage,
        }

        /// <summary>
        /// Raycastによるコリジョン判定 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="maxDistance"></param>
        /// <param name="raycastTarget"></param>
        /// <returns></returns>
        protected bool SXG_RaycastFromPlayer(Vector3 origin, Vector3 direction, float maxDistance, RaycastTarget raycastTarget)
        {
            int laycastTarget = 0x7FFFFFFF;
            switch (raycastTarget)
            {
                case RaycastTarget.Player:
                    laycastTarget = 1 << SystemConstants.OBJ_LAYER_PLAYER;
                    break;
                case RaycastTarget.Stage:
                    laycastTarget = 1 << SystemConstants.OBJ_LAYER_GROUND;
                    break;
            }

            return m_gameManager.PhysicsRaycastFromPlayer(m_id, origin, direction, maxDistance, laycastTarget);
        }


        public struct PlayerInfo
        {
            public Vector3 Position;
            public Vector3 Direction;
            public int Score;   // 現在スコア 
            public IList<FoodType>  FoodsListOnHand;   // 現在の手持ちの食材 
        }

        /// <summary>
        /// 他のプレイヤー所座標やスコアの情報を取得
        /// </summary>
        /// <returns>返り値の配列の[0]は必ず自分の情報</returns>
        protected PlayerInfo [] SXG_GetPlayersInfo()
        {
            PlayerInfo[] playersInfo = new PlayerInfo[GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE];

            for (int i=0; i < GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE; ++i)
            {
                playersInfo[i] = new();
                m_gameManager.GetPlayerInfo(m_id, (m_id + i) % GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE,
                    out playersInfo[i].Position, out playersInfo[i].Direction, out playersInfo[i].Score);
                var foodsList = m_gameManager.GetFoodsListOnHand(i);
                playersInfo[i].FoodsListOnHand = m_gameManager.GetFoodsListOnHand(i);
            }

            return playersInfo;
        }

        /// <summary>
        /// 今現在接触しているプレイヤー番号のリストを返す。
        /// 接触してるプレイヤーがいないなら返り値のCount==0
        /// この番号はSXG_GetPlayersInfoで得られる配列の添え字と一致する。
        /// 0(自分自身)は返さない。
        /// </summary>
        /// <returns></returns>
        protected List<int> SXG_GetCollidedPlayersNumber()
        {
            return m_gameManager.GetCollidedPlayersNumber(m_id);
        }


        /// <summary>
        /// 硬直動作中ですか？(キックするとしばらく別の行動をできません)
        /// </summary>
        /// <returns></returns>
        protected bool SXG_IsStiffness()
        {
            return m_gameManager.IsPlayerStiffness(m_id);
        }

        /// <summary>
        /// 硬直の残り時間を取得 
        /// </summary>
        /// <returns></returns>
        protected float SXG_GetLeftTimeOfStiffness()
        {
            return m_gameManager.GetLeftTimeOfStiffness(m_id);
        }

        /// <summary>
        /// プレイヤーの足元の領域種類を取得 
        /// </summary>
        /// <returns></returns>
        protected GameConstants.PlayerGroundType    SXG_GetPlayerGroundType()
        {
            return m_gameManager.GetPlayerGroundType(m_id);
        }


        /// <summary>
        /// 試合の残り時間を取得する 
        /// </summary>
        /// <returns></returns>
        protected float SXG_GetRemainingGameTime()
        {
            return m_gameManager.GetRemainingGameTime();
        }







        #endregion





        #region 各自でオーバーライドして実装が必要な関数群

        /// <summary>
        /// [Todo:各自でオーバーライド] 食材をGETしますか？ 
        /// </summary>
        /// <param name="foodInfo"></param>
        /// <returns></returns>
        public virtual bool UDON_ShouldGetTheFoodOnStage(FoodNowInfo foodInfo)
        {
            return false;
        }


        /// <summary>
        /// 出荷報告
        /// </summary>
        /// <param name="foodsList">出荷した食材リスト</param>
        /// <param name="tableId">出荷した先のプレイヤー番号(自分の番号と異なる場合は別のテーブルに出荷してしまったという事)</param>
        /// <param name="price">出荷した価格</param>
        /// <param name="menuName">出荷したメニュー名</param>
        public virtual void UDON_ReportOnShipping(IList<FoodType> foodsList, 
            int tableId, int price, string menuName)
        {

        }

        #endregion

    }

}



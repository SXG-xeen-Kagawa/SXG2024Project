using SXG2024;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nsSample
{
    public class ComPlayerSample05 : SXG2024.ComPlayerBase
    {
        int m_lastHavingFoodCount = 0;
        int m_targetFoodId = -1;
        Vector3 m_targetPosition = Vector3.zero;
        float m_stoppedDuration = 0f;

        State m_currentState = State.None;
        Vector3 m_currentPosition = Vector3.zero;
        Quaternion m_currentRotation = Quaternion.identity;

        enum State
        {
            Collecting,
            Shipping,
            Kick,
            None,
        }

        void Start()
        {
            DecideNextState();
        }

        void Update()
        {
            SXG_GetPositionAndRotation(out m_currentPosition, out m_currentRotation);

            // キック硬直時間
            if (SXG_IsStiffness())
                return;
            else if (m_currentState == State.Kick)
                m_currentState = State.None; // キック硬直終了

            // ぶつかった相手のメニューが成立していなさそうなら蹴る
            foreach (var player in GetOtherPlayerInfo())
            {
                if (!IsFacingTarget(player.Position, 45))
                    continue;

                if (1f <= GetDistance(player.Position))
                    continue;

                var hasNoodle = false;
                var foods = player.FoodsListOnHand;
                foreach (var food in foods)
                {
                    if (food is FoodType.Noodle)
                    {
                        hasNoodle = true;
                        break;
                    }
                }
                if (!hasNoodle)
                {
                    Kick();
                    return;
                }
            }

            // その場から動けていなければ行動を変える
            if (IsCannotMoving())
            {
                // 他プレイヤーが邪魔なら蹴る
                foreach (var player in GetOtherPlayerInfo())
                {
                    if (GetDistance(player.Position) < 0.5f)
                    {
                        Kick();
                        DecideNextState();
                        return;
                    }
                }
                // ステージが邪魔なら引っかかっている
                if (SXG_RaycastFromPlayer(Vector3.up, Vector3.forward, 1f, RaycastTarget.Stage))
                {
                    DecideNextState();
                    return;
                }
            }

            // 自身と出荷テーブルの間に敵がいれば蹴る
            if (IsFacingTarget(GetShippingTablePosition()))
            {
                if (SXG_RaycastFromPlayer(Vector3.up, Vector3.forward, 1f, RaycastTarget.Player))
                {
                    Kick();
                    return;
                }
            }

            // やることないなら次の行動を決める
            if (m_currentState == State.None)
                DecideNextState();

            // 出荷していれば次の行動へ
            if (IsShipped())
                DecideNextState();

            // 狙っている食材を追いかける
            if (m_currentState == State.Collecting)
            {
                var targetFood = SXG_GetFoodInfo(m_targetFoodId);
                m_targetPosition = targetFood.m_position;

                // もうステージ上にない or 安全圏内に無ければ次の行動へ
                if (targetFood.m_state != FoodState.OnStage || !IsInSafeArea(m_targetPosition))
                    DecideNextState();
            }

            // 走る
            Run();
        }

        /// <summary>
        /// 拾う（狙う）べき食材かをチェック
        /// </summary>
        /// <param name="food">チェック対象の食材</param>
        /// <returns>true:拾う（狙う）べき</returns>
        bool CheckFood(FoodNowInfo food)
        {
            // ステージ上にない食材はNG
            if (food.m_state != FoodState.OnStage)
                return false;

            var foodType = food.m_type;

            // 「うどん玉」は最優先
            if (foodType == FoodType.Noodle)
                return true;

            // 「おろし」と「バター」は捨てる
            if (foodType == FoodType.GratedDaikon || foodType == FoodType.Butter)
                return false;

            // あと1つしか拾えないのにまだうどん玉をとっていなければ拾わない
            if (SXG_HowManyMoreCanIHave() == 1 && SXG_GetNowPriceOnHand() == 0)
                return false;

            // それ以外はとりあえず拾って置く
            return true;
        }

        /// <summary>
        /// 次の行動決め
        /// </summary>
        void DecideNextState()
        {
            m_stoppedDuration = 0f;

            // 残り時間10秒未満なら、出荷できるときに出荷
            if (SXG_GetRemainingGameTime() < 10)
            {
                if (SXG_GetNowPriceOnHand() != 0)
                {
                    ChangeShipping();
                    return;
                }
            }

            // 手持ちがいっぱいなら出荷
            if (SXG_HowManyMoreCanIHave() == 0)
            {
                ChangeShipping();
                return;
            }

            // 狙う食材を決定
            if (DecideTargetFood(out var targetFood))
            {
                ChangeCollectiong(targetFood);
                return;
            }

            // 狙える食材が無い場合、既に出荷できる状態なら出荷
            if (SXG_GetNowPriceOnHand() != 0)
            {
                ChangeShipping();
                return;
            }

            // やること無い
            ChangeNone();

            void ChangeShipping()
            {
                m_currentState = State.Shipping;
                m_targetPosition = GetShippingTablePosition();
                m_targetFoodId = -1;
            }
            void ChangeCollectiong(FoodNowInfo targetFood)
            {
                m_currentState = State.Collecting;
                m_targetPosition = targetFood.m_position;
                m_targetFoodId = targetFood.m_foodId;
            }
            void ChangeNone()
            {
                m_currentState = State.None;
                m_targetPosition = Vector3.zero;
                m_targetFoodId = -1;
            }
        }

        /// <summary>
        /// ステージ上の食材から狙うものを決定する
        /// </summary>
        /// <param name="targetFood">狙う食材</param>
        /// <returns>true:狙う食材が決まった</returns>
        bool DecideTargetFood(out FoodNowInfo targetFood)
        {
            const float FAR_THRESHOLD = 8f;
            targetFood = default;

            var foods = SXG_GetFoodsInfoOnStage();
            if (foods.Count == 0)
                return false;

            // 最優先：最短距離のうどん玉
            var noodles = foods.FindAll(_ => _.m_type == FoodType.Noodle);
            if (GetMinDistanceFood(noodles, out var minDistanceNoodle))
            {
                if (IsInSafeArea(minDistanceNoodle.m_position) && 
                    GetDistance(minDistanceNoodle.m_position) < FAR_THRESHOLD) // 安全圏内かつ遠すぎない
                {
                    targetFood = minDistanceNoodle;
                    return true;
                }
            }

            // 自分に近い食材から狙う
            while (0 < foods.Count)
            {
                if (GetMinDistanceFood(foods, out var minDistanceFood))
                {
                    if (CheckFood(minDistanceFood))
                    {
                        if (IsInSafeArea(minDistanceFood.m_position) &&
                            GetDistance(minDistanceFood.m_position) < FAR_THRESHOLD) // 安全圏内かつ遠すぎない
                        {
                            targetFood = minDistanceFood;
                            return true;
                        }
                    }
                    // 狙いたい食材じゃないので除去して残りの食材で再検討
                    foods.Remove(minDistanceFood);
                }
            }

            // 狙いたい食材が無い
            return false;
        }

        /// <summary>
        /// 走る
        /// </summary>
        void Run()
        {
            var speed = 1.0f;

            // 目標座標と違う方向を向いていたら速度を落とす
            if (!IsFacingTarget(m_targetPosition))
                speed = 0.15f;

            SXG_MoveToTargetPosition(m_targetPosition, speed);
        }

        /// <summary>
        /// 蹴る
        /// </summary>
        void Kick()
        {
            if (m_currentState != State.Kick)
            {
                SXG_Kick();
                m_currentState = State.Kick;
            }
        }

        /// <summary>
        /// 対象座標が安全圏内か
        /// </summary>
        /// <param name="targetPos">対象座標</param>
        /// <returns>true:安全圏内</returns>
        bool IsInSafeArea(Vector3 targetPos)
        {
            const float SAFE_DISTANCE = 8f;

            // ステージ中央からの距離を見る
            targetPos.y = 0;
            var distance = Vector3.Distance(Vector3.zero, targetPos);
            return distance < SAFE_DISTANCE;
        }

        /// <summary>
        /// 出荷したか
        /// </summary>
        /// <returns>true:出荷した</returns>
        bool IsShipped()
        {
            var currentHavingFoodCount = SXG_GetMyFoodsListOnHand().Count;
            var onDiff = currentHavingFoodCount != m_lastHavingFoodCount;
            m_lastHavingFoodCount = currentHavingFoodCount;

            // 前フレームで食材を持っていたのに今フレームで持っていなければ出荷したという事
            return onDiff && currentHavingFoodCount == 0;
        }

        /// <summary>
        /// 自身が対象の座標の方を向いているか
        /// </summary>
        /// <param name="targetPosition">対象の座標</param>
        /// <param name="threshold"></param>
        /// <returns>true：向いている</returns>
        bool IsFacingTarget(Vector3 targetPosition, float threshold = 5f)
        {
            var direction = targetPosition - m_currentPosition;
            direction.y = 0;

            var forward = m_currentRotation * Vector3.forward;
            forward.y = 0;

            var diff = Vector3.Angle(forward, direction);
            return diff < threshold;
        }

        /// <summary>
        /// プレイヤーが動いているかどうか
        /// </summary>
        /// <returns></returns>
        bool IsMoving()
        {
            const float THRESHOLD = 0.1f;

            var speed = SXG_GetVelocity().magnitude;
            return THRESHOLD <= speed;
        }

        /// <summary>
        /// プレイヤーが一定秒数以上動けないかどうか
        /// </summary>
        /// <returns></returns>
        bool IsCannotMoving()
        {
            if (IsMoving())
            {
                m_stoppedDuration = 0f;
                return false;
            }

            m_stoppedDuration += Time.deltaTime;

            const float THRESHOLD = 1f;

            // 1秒以上その場から動けていない
            return THRESHOLD <= m_stoppedDuration;
        }

        List<PlayerInfo> GetOtherPlayerInfo()
        {
            var players = new List<PlayerInfo>(SXG_GetPlayersInfo());
            // 0番目に自分が入っている
            players.RemoveAt(0);
            return players;
        }

        /// <summary>
        /// 出荷テーブルの座標を取得
        /// </summary>
        /// <returns></returns>
        Vector3 GetShippingTablePosition()
        {
            const float TABLE_POSITION_Z = -12.0f;
            const float TABLE_WIDTH = 5.0f;

            Vector3 goalPosition = Vector3.zero;
            goalPosition.x = Mathf.Clamp(m_currentPosition.x, -TABLE_WIDTH * 0.8f, +TABLE_WIDTH * 0.8f);
            goalPosition.z = TABLE_POSITION_Z;

            return goalPosition;
        }

        /// <summary>
        /// 対象座標と自身の距離を取得
        /// </summary>
        /// <param name="targetPosition">対象座標</param>
        /// <returns>自身との距離</returns>
        float GetDistance(Vector3 targetPosition)
        {
            var playerPosition = m_currentPosition;
            playerPosition.y = 0;
            targetPosition.y = 0;
            return Vector3.Distance(playerPosition, targetPosition);
        }

        /// <summary>
        /// 食材リストの中から、最も距離が近い食材を取得
        /// </summary>
        /// <param name="foods">食材リスト</param>
        /// <param name="minDistanceFood">最も距離が近い食材</param>
        /// <returns>false:食材が無い</returns>
        bool GetMinDistanceFood(List<FoodNowInfo> foods, out FoodNowInfo minDistanceFood)
        {
            minDistanceFood = default;

            if (foods == null || foods.Count == 0)
                return false;

            var isOnce = true;
            var tmpDistance = 0f;
            foreach(var food in foods)
            {
                if (GetDistance(food.m_position) < tmpDistance || isOnce)
                {
                    minDistanceFood = food;
                    isOnce = false;
                }
            }
            return true;
        }

        /// <summary>
        /// 食材を拾うかどうかの判断 
        /// </summary>
        /// <param name="foodInfo"></param>
        /// <returns></returns>
        public override bool UDON_ShouldGetTheFoodOnStage(FoodNowInfo foodInfo)
        {
            // 拾うべき食材か
            if (CheckFood(foodInfo))
            {
                // 次の行動を決めて拾う
                DecideNextState();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 出荷報告
        /// </summary>
        /// <param name="foodsList">出荷した食材リスト</param>
        /// <param name="tableId">出荷した先のプレイヤー番号(自分の番号と異なる場合は別のテーブルに出荷してしまったという事)</param>
        /// <param name="price">出荷した価格</param>
        /// <param name="menuName">出荷したメニュー名</param>
        public override void UDON_ReportOnShipping(IList<FoodType> foodsList, 
            int tableId, int price, string menuName)
        {

        }
    }
}


using SXG2024;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace nsSample
{

    public class ComPlayerSample02 : SXG2024.ComPlayerBase
    {
        enum State
        {
            None,
            GotoGetFood,
            GotoShippingTable,
        }
        private State m_nextState = State.None;

        private void SetNextState(State nextState)
        {
            m_nextState = nextState;
        }



        // Start is called before the first frame update
        void Start()
        {
            SetNextState(State.GotoGetFood);
        }

        // Update is called once per frame
        void Update()
        {
            switch (m_nextState)
            {
                case State.GotoGetFood:
                    StopAllCoroutines();
                    m_nextState = State.None;
                    StartCoroutine(CoGoToGetFood());
                    break;
                case State.GotoShippingTable:
                    StopAllCoroutines();
                    m_nextState = State.None;
                    StartCoroutine(CoGoToShippingTable());
                    break;
            }
        }

        private int m_targetFoodId = -1;

        const float TOO_FAR = 10.0f;

        /// <summary>
        /// 食材を取りに行く 
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoGoToGetFood()
        {
            const float GIVEUP_TIME = 10.0f;    // 諦めるまでの時間 

            List<FoodNowInfo> foodsList = SXG_GetFoodsInfoOnStage();
            if (0 < foodsList.Count)
            {
                float time = 0;

                // 麺だけを狙う
                FoodNowInfo targetFood = SelectFood(foodsList);
                m_targetFoodId = targetFood.m_foodId;
                float giveupTime = Random.Range(GIVEUP_TIME * 0.5f, GIVEUP_TIME);
                while (time < giveupTime)
                {
                    // 持ちきれなくなったら帰る 
                    int howManyMore = SXG_HowManyMoreCanIHave();
                    if (howManyMore <= 0)
                    {
                        SetNextState(State.GotoShippingTable);
                        yield break;
                    }

                    // 残り時間が一定以下になったら帰る 
                    if (SXG_GetRemainingGameTime() < 7.0f)
                    {
                        var nowFoodsList = SXG_GetMyFoodsListOnHand();
                        FoodType[] nowFoods = new FoodType[nowFoodsList.Count];
                        nowFoodsList.CopyTo(nowFoods, 0);
                        if (0 < SXG_GetPriceOfFoods(nowFoods))
                        {
                            SetNextState(State.GotoShippingTable);
                            yield break;
                        }
                    }

                    // 食材の現在の状態を取得 
                    var nowInfo = SXG_GetFoodInfo(targetFood.m_foodId);
                    if (nowInfo.m_state == FoodState.OnStage)
                    {
                        // 狙っている食材がステージ外なら考え直す
                        if (TOO_FAR* TOO_FAR < 
                            nowInfo.m_position.x* nowInfo.m_position.x+ nowInfo.m_position.z* nowInfo.m_position.z)
                        {
                            yield return null;
                            break;
                        }

                        // 進行方向と一致しているか確認
                        Vector3 myPosition = Vector3.zero;
                        Quaternion myRotation = Quaternion.identity;
                        SXG_GetPositionAndRotation(out myPosition, out myRotation);
                        Vector3 dirToFood = (nowInfo.m_position - myPosition).normalized;
                        var velocity = SXG_GetVelocity();
                        float speedRate = 1.0f;
                        if (1.0f < velocity.magnitude)
                        {
                            float dot = Vector3.Dot(dirToFood, velocity.normalized);
                            if (dot < 0)
                            {
                                speedRate = 0.1f;
                            }
                            else
                            {
                                speedRate = Mathf.Min(1.0f, dot * 2.0f);
                            }
                        }
                        // 食材の位置へ移動 (速度変更の検証)
                        SXG_MoveToTargetPosition(nowInfo.m_position, speedRate);
                    } else
                    {
                        break;
                    }

                    time += Time.deltaTime;

                    yield return null;
                }
            } 
            else
            {
                // 食材が無くなったら一度帰る 
                yield return new WaitForSeconds(0.2f);
                SetNextState(State.GotoShippingTable);
                yield break;
            }


            // 手に持っている食材を確認 
            var foodsListOnHand = SXG_GetMyFoodsListOnHand();
            if (GameConstants.MAX_NUMBER_OF_PLAYER_CAN_HAVE <= foodsListOnHand.Count+Random.Range(0,2))
            {
                // 出荷テーブルに運ぶ 
                SetNextState(State.GotoShippingTable);
            }
            else
            {
                // 繰り返し食材を集める  
                SetNextState(State.GotoGetFood);
            }
        }

        /// <summary>
        /// 狙う食材を決定 
        /// </summary>
        /// <param name="foodsList"></param>
        /// <returns></returns>
        private FoodNowInfo SelectFood(List<FoodNowInfo> foodsList)
        {
            // 自分の座標 
            Vector3 localPosition = Vector3.zero;
            Quaternion localRotation = Quaternion.identity;
            SXG_GetPositionAndRotation(out localPosition, out localRotation);

            // 食材を検索 
            FoodNowInfo targetFoodInfo = new();
            float bestScore = float.MaxValue;
            foreach (var food in foodsList)
            {
                // 遠すぎは無効 
                if (TOO_FAR * TOO_FAR < food.m_position.x*food.m_position.x + food.m_position.z*food.m_position.z)
                {
                    continue;
                }
                // 自身との距離 
                float distance = (localPosition - food.m_position).magnitude;
                float evaluation = 0;
                if (food.m_type != FoodType.Noodle)
                {
                    evaluation = distance + 10;
                }
                else
                {
                    evaluation = distance;
                }
                if (evaluation < bestScore)
                {
                    targetFoodInfo = food;
                    bestScore = evaluation;
                }
            }

            return targetFoodInfo;
        }


        /// <summary>
        /// 出荷テーブルへ運ぶ 
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoGoToShippingTable()
        {
            const float TABLE_POSITION_Z = -12.0f;
            const float TABLE_WIDTH = 5.0f;

            // ゴール座標を決定 
            Vector3 myPosition;
            Quaternion myRotation;
            SXG_GetPositionAndRotation(out myPosition, out myRotation);
            Vector3 goalPosition = Vector3.zero;
            goalPosition.x = Mathf.Clamp(myPosition.x, -TABLE_WIDTH*0.8f, +TABLE_WIDTH*0.8f);
            goalPosition.z = TABLE_POSITION_Z;

            // 走る 
            while (true)
            {
                // 手に持っている食材が無くなったら出荷したという事 
                var foodsListOnHand = SXG_GetMyFoodsListOnHand();
                if (foodsListOnHand.Count <= 0)
                {
                    break;
                }

                // ゴール座標へ向かって走る 
                SXG_MoveToTargetPosition(goalPosition, 1.0f);

                yield return null;
            }

            // 改めて食材を集めに行く 
            SetNextState(State.GotoGetFood);

        }


        /// <summary>
        /// 食材を拾うかどうかの判断 
        /// </summary>
        /// <param name="foodInfo"></param>
        /// <returns></returns>
        public override bool UDON_ShouldGetTheFoodOnStage(FoodNowInfo foodInfo)
        {
            // それを取る事でスコアアップするなら拾う 
            int nowPrice = 0;
            int futurePrice = 0;
            IList<FoodType> nowFoodsList = SXG_GetMyFoodsListOnHand();
            if (0 < nowFoodsList.Count)
            {
                FoodType[] nowFoods = new FoodType[nowFoodsList.Count];
                nowFoodsList.CopyTo(nowFoods, 0);
                nowPrice = SXG_GetPriceOfFoods(nowFoods);
            }
            FoodType[] futureFoods = new FoodType[nowFoodsList.Count + 1];
            futureFoods[0] = foodInfo.m_type;
            nowFoodsList.CopyTo(futureFoods, 1);
            futurePrice = SXG_GetPriceOfFoods(futureFoods);
            if (nowPrice < futurePrice)
            {
                return true;
            }

            // 狙っていた食材なら拾う 
            if (foodInfo.m_foodId == m_targetFoodId)
            {
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
            // 行動リセット 
            StopAllCoroutines();

            // 改めて食材を集めに行く 
            StartCoroutine(CoGoToGetFood());

        }

    }


}


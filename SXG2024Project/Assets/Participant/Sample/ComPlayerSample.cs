﻿using SXG2024;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace nsSample
{

    public class ComPlayerSample : SXG2024.ComPlayerBase
    {
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(CoGoToGetFood());
        }

        // Update is called once per frame
        void Update()
        {

        }

        private int m_targetFoodId = -1;


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
                // ランダムで食材を選択 ()
                FoodNowInfo targetFood = foodsList[Random.Range(0, foodsList.Count)];
                m_targetFoodId = targetFood.m_foodId;
                float giveupTime = Random.Range(GIVEUP_TIME * 0.5f, GIVEUP_TIME);
                while (time < giveupTime)
                {
                    // 食材の現在の状態を取得 
                    var nowInfo = SXG_GetFoodInfo(m_targetFoodId);
                    if (nowInfo.m_state == FoodState.OnStage)
                    {
                        // 食材の位置へ移動 
                        SXG_MoveToTargetPosition(nowInfo.m_position, 1.0f);
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
                StartCoroutine(CoGoToShippingTable());
                yield break;
            }


            // 手に持っている食材を確認 
            var foodsListOnHand = SXG_GetMyFoodsListOnHand();
            if (GameConstants.MAX_NUMBER_OF_PLAYER_CAN_HAVE <= foodsListOnHand.Count+Random.Range(0,2))
            {
                // 出荷テーブルに運ぶ 
                StartCoroutine(CoGoToShippingTable());
            } 
            else
            {
                // 繰り返し食材を集める  
                StartCoroutine(CoGoToGetFood());
            }
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
            StartCoroutine(CoGoToGetFood());

        }


        /// <summary>
        /// 食材を拾うかどうかの判断 
        /// </summary>
        /// <param name="foodInfo"></param>
        /// <returns></returns>
        public override bool UDON_ShouldGetTheFoodOnStage(FoodNowInfo foodInfo)
        {
            // 狙っていた食材なら拾う 
            return foodInfo.m_foodId == m_targetFoodId;
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


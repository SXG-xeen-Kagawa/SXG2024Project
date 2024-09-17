using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;



namespace SXG2024
{

    public class FoodsCreator
    {
        public class FoodEntrySheet
        {
            const int OWNER_NONE = -1;

            public FoodType m_foodType;
            public OneFood m_oneFood = null;
            public int m_serialNo = 0;
            public FoodState m_foodState;
            public int m_ownerId = OWNER_NONE;

            /// <summary>
            /// ステージから拾われる 
            /// </summary>
            /// <param name="playerSheet"></param>
            public void PickupFromStage(UdonChefSceneManager.PlayerEntrySheet playerSheet)
            {
                m_foodState = FoodState.SomeoneHas;
                m_ownerId = playerSheet.m_id;
                m_oneFood.SetPickup();

                // 持っているフードリストに追加
                playerSheet.m_foodsListOnHand.Add(m_foodType);
                playerSheet.m_foodsIdListOnHand.Add(m_serialNo);

                // うどん鉢に乗せる 
                playerSheet.m_udonBowl.RegistFood(this, m_foodType);
            }
        }

        private List<FoodEntrySheet> m_foodEntrySheetList = new();
        private Dictionary<FoodType, FoodOneData> m_foodsDictionary = new();
        private FoodsData m_foodsData = null;
        private GameData m_gameData = null;
        private RandomFoodPosition m_randomFoodPosition = new();
        private int m_foodSerialNo = 0;
        private int m_foodPopCount = 0;     // 出現数
        private float m_totalTime = 0;
        private float m_updateTime = 0;
        private int m_updateStep = 0;
        private List<FoodType> m_drawLotteryFoods = new();  // 抽選された食材の出現順序 

        /// <summary>
        /// コンストラクタ 
        /// </summary>
        /// <param name="foodsData"></param>
        public FoodsCreator(FoodsData foodsData, GameData gameData)
        {
            m_foodsData = foodsData;
            m_gameData = gameData;

            // 食材辞書作成 
            foreach (var data in m_foodsData.m_foods)
            {
                m_foodsDictionary[data.m_foodType] = data;
            }

            // シリアル番号を難読化 
            m_foodSerialNo = DateTime.Now.Millisecond;
        }


        /// <summary>
        /// 初期配置する 
        /// </summary>
        /// <param name="count"></param>
        /// <param name="rootTr"></param>
        public void CreateInitialPlacement(int count, Transform rootTr)
        {
            for (int i=0; i < count; ++i)
            {
                if (m_foodPopCount < m_drawLotteryFoods.Count)
                {
                    // 種類  
                    FoodType foodType = m_drawLotteryFoods[m_foodPopCount++];

                    // ランダム座標 
                    Vector3 randomPos = m_randomFoodPosition.Get();
                    randomPos.y = Random.Range(0, 0.5f);

                    // 生成してリストに追加 
                    CreateOneAndAddList(foodType, randomPos, rootTr);
                }
            }
        }


        /// <summary>
        /// 食材を1つ生成してリストに追加 
        /// </summary>
        /// <param name="foodType"></param>
        /// <param name="position"></param>
        private void CreateOneAndAddList(FoodType foodType, Vector3 position, Transform rootTr)
        {
            var data = m_foodsDictionary[foodType];

            FoodEntrySheet sheet = new();
            sheet.m_serialNo = m_foodSerialNo++;
            sheet.m_foodType = foodType;
            sheet.m_foodState = FoodState.OnStage;

            // 食材生成 
            sheet.m_oneFood = GameObject.Instantiate(data.m_foodPrefab, rootTr);
            sheet.m_oneFood.transform.SetPositionAndRotation(position,
                Quaternion.Euler(0, Random.Range(0, 360), 0));

            // リストで管理 
            m_foodEntrySheetList.Add(sheet);
        }


        /// <summary>
        /// IDで検索 
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public FoodEntrySheet   FindByID(int serialNo)
        {
            var foodSheet = m_foodEntrySheetList.Find((a) => a.m_serialNo == serialNo);
            return foodSheet;
        }

        /// <summary>
        /// 食材Objectで検索する 
        /// </summary>
        /// <param name="foodObj"></param>
        /// <returns></returns>
        public FoodEntrySheet FindByFoodObj(GameObject foodObj)
        {
            var foodSheet = m_foodEntrySheetList.Find((a) => a.m_oneFood.gameObject == foodObj);
            return foodSheet;
        }

        /// <summary>
        /// リストを参照する 
        /// </summary>
        /// <returns></returns>
        public IList<FoodEntrySheet>    GetList()
        {
            return m_foodEntrySheetList;
        }


        /// <summary>
        /// 出現する食材を抽選する
        /// </summary>
        /// <param name="totalCount">食材の総数</param>
        /// <param name="noodleCount">その内、うどん玉の数</param>
        public void DrawLotteryFoods(int totalCount, int noodleCount)
        {
            m_foodPopCount = 0;
            m_updateStep = 0;
            m_updateTime = 0;
            m_drawLotteryFoods.Clear();

            // うどん玉を設定 
            for (int i=0; i < noodleCount; ++i)
            {
                m_drawLotteryFoods.Add(FoodType.Noodle);
            }
            // 残りの食材をランダムで投入 
            int randamLastValue = Enum.GetValues(typeof(FoodType)).Length - 1;  // 末尾のNoneは抜く 
            for (int i=noodleCount; i < totalCount; ++i)
            {
                var next = (FoodType)Random.Range(1, randamLastValue);
                m_drawLotteryFoods.Add(next);
            }

            // 混ぜる 
            for (int i=0; i < totalCount; ++i)
            {
                int key1 = Random.Range(0, totalCount - 1);
                int key2 = (key1 + Random.Range(1, totalCount)) % totalCount;
                var temp = m_drawLotteryFoods[key1];
                m_drawLotteryFoods[key1] = m_drawLotteryFoods[key2];
                m_drawLotteryFoods[key2] = temp;
            }
        }


        /// <summary>
        /// 食材の補充更新 
        /// </summary>
        /// <param name="rootTr"></param>
        public void UpdateToPopFoods(Transform rootTr)
        {
            if (m_foodPopCount < m_drawLotteryFoods.Count)
            {
                m_totalTime += Time.deltaTime;

                // テーブルスライド確認 
                var currentDropSpan = m_gameData.DropSpanTimeTable[m_updateStep];
                if (m_updateStep+1 < m_gameData.DropSpanTimeTable.Length)
                {
                    var nextDropSpan = m_gameData.DropSpanTimeTable[m_updateStep + 1];
                    if (nextDropSpan.m_time <= m_totalTime)
                    {
                        m_updateStep++;
                        currentDropSpan = nextDropSpan;
                    }
                }
                
                // ドロップ 
                m_updateTime += Time.deltaTime;
                if (0 < currentDropSpan.m_countOfOnce)
                {
                    if (currentDropSpan.m_span <= m_updateTime)
                    {
                        m_updateTime -= currentDropSpan.m_span;

                        // 生成 
                        for (int i=0; i < currentDropSpan.m_countOfOnce; ++i)
                        {
                            if (m_foodPopCount < m_drawLotteryFoods.Count)
                            {
                                // 種類  
                                FoodType foodType = m_drawLotteryFoods[m_foodPopCount++];

                                // ランダム座標 
                                Vector3 randomPos = m_randomFoodPosition.Get();
                                randomPos.y = 12+2*i;

                                // 生成してリストに追加 
                                CreateOneAndAddList(foodType, randomPos, rootTr);
                            }
                        }
                    }
                }

            }
        }


        /// <summary>
        /// 次に落ちてくる食材を取得
        /// </summary>
        /// <returns></returns>
        public FoodType GetNextFallingFood()
        {
            if (m_foodPopCount < m_drawLotteryFoods.Count)
            {
                return m_drawLotteryFoods[m_foodPopCount];
            }
            return FoodType.None;
        }

        /// <summary>
        /// 将来落ちてくる食材をすべて取得
        /// </summary>
        /// <returns></returns>
        public FoodType [] GetAllFutureFoodsDrops()
        {
            if (m_foodPopCount < m_drawLotteryFoods.Count)
            {
                FoodType[] results = new FoodType[m_drawLotteryFoods.Count - m_foodPopCount];
                for (int i=0; i < results.Length; ++i)
                {
                    results[i] = m_drawLotteryFoods[m_foodPopCount + 1];
                }
                return results;
            } else
            {
                return null;
            }
        }


    }


}


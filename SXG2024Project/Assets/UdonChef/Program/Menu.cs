using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace SXG2024
{

    public class Menu
    {
        private MenuData m_menuData = null;

        public Menu(MenuData menuData)
        {
            m_menuData = menuData;
        }

        /// <summary>
        /// メニュー判定
        /// </summary>
        /// <param name="foods">食材(順不同)</param>
        /// <param name="price">OUT:価格</param>
        /// <param name="menuName">OUT:メニュー名</param>
        /// <param name="goodFoodsCount">OUT:倍率増加ボーナス食材の数</param>
        /// <param name="badFoodsCount">OUT:価格倍率が半減する食材の数</param>
        /// <returns>true:メニューとして成立する</returns>
        public bool Check(FoodType [] foods, out int price, out string menuName,
            out int goodFoodsCount, out int badFoodsCount)
        {
            price = 0;
            menuName = string.Empty;
            goodFoodsCount = 0;
            badFoodsCount = 0;

            if (foods == null || foods.Length == 0)
                return false;

            var foodsList = foods.ToList();
            foreach (var menu in m_menuData.MenuList)
            {
                // メニューとして成立するかチェック、成立したら残りの食材で価格計算
                if (IsMatch(foodsList, menu.RequiredFoods, out var remainingFoods))
                {
                    var namePrefix = string.Empty;
                    var nameSuffix = string.Empty;
                    var priceRatio = 1f;
                    var additionalPrice = 0;

                    if (0 < remainingFoods.Count)
                    {
                        // うどん玉の数をカウントして残りの食材から除外
                        var noodleCount = CheckMatchedCount(ref remainingFoods, new() { FoodType.Noodle });
                        // 価格をうどん玉*100円
                        additionalPrice += (noodleCount * 100);

                        // うどん玉の数に応じて名前の接尾辞
                        nameSuffix = noodleCount switch
                        {
                            1 => "中",
                            2 => "大",
                            3 => "特大",
                            4 => "ジャンボ",
                            _ => "",
                        };

                        // 残りの食材が全て倍率増加ボーナス食材か
                        if (remainingFoods.All(_ => menu.PriceRatioBonusFoods.Contains(_)))
                        {
                            goodFoodsCount = remainingFoods.Count;

                            // 食材の数だけ倍率加算
                            priceRatio += remainingFoods.Count;

                            // 仮：名前の接頭辞
                            namePrefix = remainingFoods.Count switch
                            {
                                1 => "ダブル",
                                2 => "トリプル",
                                3 => "デラックス",
                                4 => "ウルトラ",
                                _ => "",
                            };
                        }
                        else
                        {
                            // 価格倍率が半減する食材の数をカウントして残りの食材から除外
                            badFoodsCount = CheckMatchedCount(ref remainingFoods, menu.BadFoods);
                            // 食材の数だけ倍率を半減
                            for (var i = 0; i < badFoodsCount; i++)
                                priceRatio /= 2;

                            goodFoodsCount = remainingFoods.Count;

                            // 残りの食材は一律+50円
                            additionalPrice += (goodFoodsCount * 50);
                        }
                    }

                    price = (int)((menu.Price + additionalPrice) * priceRatio);
                    menuName = namePrefix + menu.Name + nameSuffix;

                    //Debug.Log($"結果：{menuName}　{price}円　( {menu.Price} + {additionalPrice}円 ) × {priceRatio}倍" 
                    //    + $"\nGOOD：{goodFoodsCount}個　BAD：{badFoodsCount}個");

                    return true;
                }
            }

            //Debug.Log("結果：なし");
            return false;
        }

        bool IsMatch(List<FoodType> list1, List<FoodType> list2, out List<FoodType> remainingElements)
        {
            remainingElements = null;

            if (list2.Count == 0)
                return false;

            var dict1 = list1.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var dict2 = list2.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

            // list1に、list2の要素がすべて含まれているかチェック
            foreach (var kvp in dict2)
            {
                if (!dict1.TryGetValue(kvp.Key, out int count) || count < kvp.Value)
                    return false;
            }

            // list1から、list2に含まれていない要素だけを抽出
            remainingElements = new();
            foreach (var element in list1)
            {
                if (dict2.TryGetValue(element, out int count) && count > 0)
                    dict2[element]--;
                else
                    remainingElements.Add(element);
            }
            return true;
        }

        int CheckMatchedCount(ref List<FoodType> list1, List<FoodType> list2)
        {
            if (list2.Count == 0)
                return 0;

            int totalCount = 0;
            foreach (var element in list2)
            {
                var elementCount1 = list1.Count(x => x == element);
                var elementCount2 = list2.Count(x => x == element);

                // list1に、list2と一致する要素がいくつ含まれているかをカウント
                totalCount += elementCount1;

                // list1から、list2に含まれる要素を捨て削除
                for (int i = 0; i < elementCount1; i++)
                {
                    list1.Remove(element);
                }
            }
            return totalCount;
        }
    }


}


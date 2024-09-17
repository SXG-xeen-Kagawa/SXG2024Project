using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SXG2024
{
    [ExecuteInEditMode]
    public class MenuCheckDebug : MonoBehaviour
    {
        [SerializeField]
        private MenuData m_menuData = null;

        private Menu m_menu = null;

        private void OnEnable()
        {
            if (m_menuData == null)
                return;
            
            m_menu = new(m_menuData);

            var startTime = DateTime.Now;

            m_menu.Check(GetDebugFoods(7), out var price, out var menuName, out int goodFoodsCount, out int badFoodsCount);

            var endTime = DateTime.Now;
            Debug.Log("所要時間：" + (endTime - startTime).TotalSeconds + " s");
        }

        private FoodType[] GetDebugFoods(int num)
        {
            return num switch
            {
                1 => new FoodType[] { FoodType.Noodle }, // かけうどん
                2 => new FoodType[] { FoodType.Noodle, FoodType.Butter, FoodType.Butter }, // かけうどん
                3 => new FoodType[] { FoodType.Noodle, FoodType.Noodle, FoodType.ShrimpTempura }, // かけうどん
                4 => new FoodType[] { FoodType.Curry, FoodType.Butter, FoodType.ShrimpTempura }, // なし
                5 => new FoodType[] { FoodType.Curry, FoodType.Noodle, FoodType.Egg, FoodType.Noodle, FoodType.GratedDaikon }, // カレーうどん
                6 => new FoodType[] { FoodType.Egg, FoodType.Noodle, FoodType.Butter }, // 窯バター
                7 => new FoodType[] { FoodType.Meat, FoodType.Meat, FoodType.Noodle }, // 肉うどん
                8 => new FoodType[] { FoodType.Noodle, FoodType.Curry, FoodType.GratedDaikon, FoodType.Noodle, FoodType.Meat }, // 肉カレーうどん
                9 => new FoodType[] { FoodType.Noodle, FoodType.Egg, FoodType.Meat, FoodType.Meat }, // 肉窯玉うどん
                10 => new FoodType[] { FoodType.Noodle, FoodType.Noodle, FoodType.Noodle, FoodType.Noodle, FoodType.Noodle }, // かけうどんジャンボ
                _ => null,
            };
        }
    }
}
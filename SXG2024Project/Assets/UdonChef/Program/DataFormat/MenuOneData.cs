using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SXG2024
{
    [CreateAssetMenu(menuName = "SXG2024/Create MenuOneData")]
    public class MenuOneData : ScriptableObject
    {
        [SerializeField, Tooltip("メニュー名")]
        private string m_name = string.Empty;

        [SerializeField, Min(0), Tooltip("価格")]
        private int m_price = 0;

        [SerializeField, Tooltip("メニューに必要な材料")]
        private List<RequiredFoodInfo> m_requiredFoods = new();

        [SerializeField, Tooltip("メニュー価格が半減する材料")]
        private List<FoodType> m_badFoods = new();

        [System.Serializable]
        private class RequiredFoodInfo
        {
            [SerializeField]
            private FoodType m_foodType;

            [SerializeField, Min(1)]
            private int m_count = 1;

            [SerializeField, Tooltip("倍率増加ボーナス")]
            private bool m_onPriceRatioBonus = false;

            public FoodType foodType => m_foodType;
            public int count => m_count;
            public bool onPriceRatioBonus => m_onPriceRatioBonus;
        }

        /// <summary>
        /// メニュー名
        /// </summary>
        public string Name => m_name;
        /// <summary>
        /// 価格
        /// </summary>
        public int Price => m_price;
        /// <summary>
        /// メニュー成立に必要な材料
        /// </summary>
        public List<FoodType> RequiredFoods { get; private set; } = null;
        /// <summary>
        /// 価格倍率が+1倍する材料
        /// </summary>
        public List<FoodType> PriceRatioBonusFoods { get; private set; } = null;
        /// <summary>
        /// メニュー価格が半減する材料
        /// </summary>
        public List<FoodType> BadFoods => m_badFoods;

        private List<FoodType> CreateRequiredFoods()
        {
            var list = new List<FoodType>();
            foreach (var food in m_requiredFoods)
            {
                for (var i = 0; i < food.count; i++)
                    list.Add(food.foodType);
            }
            return list;
        }
        private List<FoodType> CreatePriceRatioBonusFoods()
        {
            var list = new List<FoodType>();
            foreach (var food in m_requiredFoods)
            {
                if (food.onPriceRatioBonus)
                    list.Add(food.foodType);
            }
            return list;
        }

        private void OnEnable()
        {
            RequiredFoods = CreateRequiredFoods();
            PriceRatioBonusFoods = CreatePriceRatioBonusFoods();
        }
    }
}
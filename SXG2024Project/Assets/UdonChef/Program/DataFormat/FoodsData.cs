using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    [CreateAssetMenu(menuName = "SXG2024/Create FoodsData")]
    public class FoodsData : ScriptableObject
    {
        public FoodOneData[] m_foods;
    }


    [System.Serializable]
    public class FoodOneData
    {
        public string m_name;
        public FoodType m_foodType;
        public OneFood m_foodPrefab = null;
    }



}


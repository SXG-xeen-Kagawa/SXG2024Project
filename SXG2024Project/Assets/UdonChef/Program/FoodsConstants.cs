using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    /// <summary>
    /// 食材の種類 
    /// </summary>
    public enum FoodType
    {
        Noodle,             // うどん玉
        ShrimpTempura,      // 海老天
        ChikuwaTempura,     // ちくわ天
        Kitsune,            // きつね(Fried tofu pouch)
        GratedDaikon,       // 大根おろし
        Egg,                // 卵
        Meat,               // 肉
        Butter,             // バター
        Lemon,              // レモン
        Curry,              // カレー

        None,               // なし 
    }

    /// <summary>
    /// 食材の状態 
    /// </summary>
    public enum FoodState
    {
        OnStage,    // ステージ上 
        SomeoneHas, // 誰かが持っている 
        Shipped,    // 出荷済み 

        None
    }


    /// <summary>
    /// 食材の現在の状態 
    /// </summary>
    public struct FoodNowInfo
    {
        public int m_foodId;    // シリアルID 
        public FoodType m_type;
        public FoodState m_state;
        public Vector3 m_position;
    }


    public class FoodsConstants
    {



    }

}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class ResultBook
    {
        public class ResultOne
        {
            public FoodType[] m_foods = null;
            public int m_price = 0;
            public string m_menuName = null;
            public int m_madePlayerId = 0;      // 作成者 
            public int m_goodFoodsCount = 0;
            public int m_badFoodsCount = 0;

            public string m_displayMenuName = null; // 表示用のメニュー名 
        }

        private List<ResultOne> m_resultsList = new();
        private int m_totalPrice = 0;

        private System.Text.StringBuilder m_sb = new();

        /// <summary>
        /// 合計価格の取得 
        /// </summary>
        public int TotalPrice => m_totalPrice;


        /// <summary>
        /// 登録 
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="price"></param>
        /// <param name="menuName"></param>
        /// <param name="madePlayerId"></param>
        public void Regist(FoodType[] foods, int price, string menuName, int madePlayerId,
            int goodFoodsCount, int badFoodsCount)
        {
            // 表示名を決定 
            m_sb.Clear();
            m_sb.Append(menuName);
            if (0 < goodFoodsCount || 0 < badFoodsCount)
            {
                m_sb.Append(" （");
                for (int i=0; i < goodFoodsCount; ++i)
                {
                    m_sb.Append("↑");
                }
                for (int j=0; j < badFoodsCount; ++j)
                {
                    m_sb.Append("↓");
                }
                m_sb.Append("）");
            }
            string displayMenuName = m_sb.ToString();

            // リストに登録 
            m_resultsList.Add(
                new()
                {
                    m_foods = foods,
                    m_price = price,
                    m_menuName = menuName,
                    m_madePlayerId = madePlayerId,
                    m_goodFoodsCount = goodFoodsCount,
                    m_badFoodsCount = badFoodsCount,
                    m_displayMenuName = displayMenuName
                });
            m_totalPrice += price;
        }

        /// <summary>
        /// リセット 
        /// </summary>
        public void Reset()
        {
            m_resultsList = new();
            m_totalPrice = 0;
        }


        /// <summary>
        /// データ取得 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="price"></param>
        /// <param name="menuName"></param>
        /// <param name="madePlayerId"></param>
        /// <returns></returns>
        public bool GetData(int number, 
            out int price, out string menuName, out int madePlayerId, bool isSuccessOnly)
        {
            if (0 <= number && number < m_resultsList.Count)
            {
                if (!isSuccessOnly)
                {
                    var data = m_resultsList[number];
                    price = data.m_price;
                    //menuName = data.m_menuName;
                    menuName = data.m_displayMenuName;
                    madePlayerId = data.m_madePlayerId;
                    return true;
                } 
                else
                {
                    // 成功のみ
                    int count = 0;
                    foreach (var data in m_resultsList)
                    {
                        if (0 < data.m_price)
                        {
                            if (count == number)
                            {
                                price = data.m_price;
                                //menuName = data.m_menuName;
                                menuName = data.m_displayMenuName;
                                madePlayerId = data.m_madePlayerId;
                                return true;
                            } else
                            {
                                count++;
                            }
                        }
                    }
                }
            }

            price = 0;
            menuName = "";
            madePlayerId = 0;
            return false;
        }

    }


}


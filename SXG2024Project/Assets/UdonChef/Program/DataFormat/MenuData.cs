using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SXG2024
{
    [CreateAssetMenu(menuName = "SXG2024/Create MenuData")]
    public class MenuData : ScriptableObject
    {
        [SerializeField, Tooltip("優先度の高いメニューほど上に配置してください")]
        private List<MenuOneData> m_menuList = new();

        /// <summary>
        /// メニューリスト
        /// </summary>
        public List<MenuOneData> MenuList => m_menuList;
    }
}
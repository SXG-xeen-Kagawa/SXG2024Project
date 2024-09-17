using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SXG2024
{
    [CreateAssetMenu(menuName = "SXG2024/Create MenuData")]
    public class MenuData : ScriptableObject
    {
        [SerializeField, Tooltip("�D��x�̍������j���[�قǏ�ɔz�u���Ă�������")]
        private List<MenuOneData> m_menuList = new();

        /// <summary>
        /// ���j���[���X�g
        /// </summary>
        public List<MenuOneData> MenuList => m_menuList;
    }
}
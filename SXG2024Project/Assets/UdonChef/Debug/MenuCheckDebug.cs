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
            Debug.Log("���v���ԁF" + (endTime - startTime).TotalSeconds + " s");
        }

        private FoodType[] GetDebugFoods(int num)
        {
            return num switch
            {
                1 => new FoodType[] { FoodType.Noodle }, // �������ǂ�
                2 => new FoodType[] { FoodType.Noodle, FoodType.Butter, FoodType.Butter }, // �������ǂ�
                3 => new FoodType[] { FoodType.Noodle, FoodType.Noodle, FoodType.ShrimpTempura }, // �������ǂ�
                4 => new FoodType[] { FoodType.Curry, FoodType.Butter, FoodType.ShrimpTempura }, // �Ȃ�
                5 => new FoodType[] { FoodType.Curry, FoodType.Noodle, FoodType.Egg, FoodType.Noodle, FoodType.GratedDaikon }, // �J���[���ǂ�
                6 => new FoodType[] { FoodType.Egg, FoodType.Noodle, FoodType.Butter }, // �q�o�^�[
                7 => new FoodType[] { FoodType.Meat, FoodType.Meat, FoodType.Noodle }, // �����ǂ�
                8 => new FoodType[] { FoodType.Noodle, FoodType.Curry, FoodType.GratedDaikon, FoodType.Noodle, FoodType.Meat }, // ���J���[���ǂ�
                9 => new FoodType[] { FoodType.Noodle, FoodType.Egg, FoodType.Meat, FoodType.Meat }, // ���q�ʂ��ǂ�
                10 => new FoodType[] { FoodType.Noodle, FoodType.Noodle, FoodType.Noodle, FoodType.Noodle, FoodType.Noodle }, // �������ǂ�W�����{
                _ => null,
            };
        }
    }
}
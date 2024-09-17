using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class ShippingTable : MonoBehaviour
    {
        [SerializeField] private int m_tableId = 0;

        private UdonChefSceneManager m_sceneMng = null;


        public void SetUdonChefSceneManager(UdonChefSceneManager sceneMng)
        {
            m_sceneMng = sceneMng;
        }


        private void OnCollisionEnter(Collision collision)
        {
            // プレイヤーが接触したら出荷確認 
            if (collision.gameObject.layer == SystemConstants.OBJ_LAYER_PLAYER)
            {
                if (m_sceneMng != null)
                {
                    // 出荷 
                    m_sceneMng.ShipTheFoods(collision.gameObject, m_tableId, transform.position);
                }
            }
        }
    }


}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class OneFood : MonoBehaviour
    {
        private UdonChef m_ownerChef = null;
        private Rigidbody m_rigidbody = null;
        private int m_onHandLevel = 0;

        [SerializeField] private GameObject[] m_models;


        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();

            ChangeVisibleModel(0);
        }


        /// <summary>
        /// 表示モデルを変更する 
        /// </summary>
        /// <param name="modelNo"></param>
        public void ChangeVisibleModel(int modelNo)
        {
            for (int i=0; i < m_models.Length; ++i)
            {
                var model = m_models[i];
                if (model != null)
                {
                    model.SetActive(i == modelNo);
                }
            }
        }


        /// <summary>
        /// ステージから拾い上げられる 
        /// </summary>
        /// <param name="ownerChef"></param>
        public void SetPickup()
        {
            // 物理処理を止める 
            m_rigidbody.isKinematic = true;
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var coll in colliders)
            {
                coll.enabled = false;
            }

            // モデルの見た目を変更 
            ChangeVisibleModel(1);

            //// オーナー設定 
            //m_ownerChef = ownerChef;
            //m_onHandLevel = onHandLevel;

            //// オーナーの手の上 
            //transform.SetParent(ownerChef.transform);
            //transform.localPosition = new Vector3(0, 0.8f, 1.4f + onHandLevel * 0.25f);
        }

    }


}


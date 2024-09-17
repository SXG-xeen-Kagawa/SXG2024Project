using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class UdonBowl : MonoBehaviour
    {
        private Rigidbody m_rigidbody = null;
        private Collider m_collider = null;
        [SerializeField] private Transform m_bowlBaseTr = null;
        [SerializeField] private GameObject m_soupObj = null;
        [SerializeField] private GameObject m_negiObj = null;
        [SerializeField] private GameObject[] m_noodleObjs = null;

        private List<FoodsCreator.FoodEntrySheet> m_foodsOnBowlList = new();

        [SerializeField] private Vector3 m_okMenuSpeed = new Vector3(0, 3, 3.7f);
        [SerializeField] private Vector3 m_ngMenuSpeed = new Vector3(0, 6, 8);

        private bool m_invisibleSoupFlag = false;
        private bool m_containsCurry = false;        // カレーが含まれる 
        private int m_noodleState = 0;      // 0.麺なし / 1.麺あり / 2.カレー麺 


        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();
            m_collider = GetComponent<Collider>();

            // 物理挙動OFF 
            SetPhysics(false);

            // 最初はダシは見えない 
            m_soupObj.SetActive(false);
        }


        private void SetPhysics(bool isActive)
        {
            m_collider.enabled = isActive;
            m_rigidbody.isKinematic = !isActive;
        }


        /// <summary>
        /// 食材を登録 
        /// </summary>
        /// <param name="newFoodSheet"></param>
        public void RegistFood(FoodsCreator.FoodEntrySheet newFoodSheet, FoodType foodType)
        {
        //    const float FOOD_LOCAL_OFFSET = 0.3f;

            m_foodsOnBowlList.Add(newFoodSheet);

            // うどん鉢の上に置く 
            newFoodSheet.m_oneFood.transform.SetParent(this.transform);
            newFoodSheet.m_oneFood.transform.SetLocalPositionAndRotation(
                m_bowlBaseTr.localPosition, Quaternion.identity);
            //newFoodSheet.m_oneFood.transform.localPosition = m_bowlBaseTr.localPosition;
            //       Vector3.up * (FOOD_LOCAL_OFFSET * m_foodsOnBowlList.Count);

            // ダシを表示する 
            if (foodType == FoodType.Curry)
            {
                m_invisibleSoupFlag = true;     // ただし、カレーが来たらダシは消す 
                m_containsCurry = true;
                UpdateNoodleStatus(false);
            }
            if (m_soupObj.activeSelf == m_invisibleSoupFlag)
            {
                m_soupObj.SetActive(!m_invisibleSoupFlag);
            }

            // ネギを表示する 
            if (!m_negiObj.activeSelf)
            {
                m_negiObj.SetActive(true);
            }

            // 麺を表示する 
            if (foodType == FoodType.Noodle)
            {
                UpdateNoodleStatus(true);
            }
        }

        private void UpdateNoodleStatus(bool isNewComming)
        {
            int lastState = m_noodleState;

            if (isNewComming)
            {
                m_noodleState = 1;
            }
            if (0 < m_noodleState)
            {
                if (m_containsCurry)
                {
                    m_noodleState = 2;
                }
            }
            if (lastState != m_noodleState)
            {
                if (m_noodleState == 1)
                {
                    m_noodleObjs[0].SetActive(true);
                    m_noodleObjs[1].SetActive(false);
                } else if (m_noodleState == 2)
                {
                    m_noodleObjs[0].SetActive(false);
                    m_noodleObjs[1].SetActive(true);
                }
            }
        }

        /// <summary>
        /// テーブルに投げ飛ばす 
        /// </summary>
        /// <param name="dir"></param>
        public void ThrowToTable(Vector3 dir, bool isGoodMenu)
        {
            // プレイヤーから切り離す 
            transform.SetParent(null);

            // 物理有効化 
            SetPhysics(true);

            // 投げる 
            Vector3 throwForce = dir;
            if (isGoodMenu)
            {
                // メニューが成立してたらちゃんと投げる 
                throwForce *= m_okMenuSpeed.z;
                throwForce.y = m_okMenuSpeed.y;

                Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Register, Mathf.Clamp(transform.position.x / 9f, -0.4f, 0.4f));
            } else
            {
                // メニュー不成立なら投げ捨てる 
                throwForce *= m_ngMenuSpeed.z;
                throwForce.y = m_ngMenuSpeed.y;

                // 物理の拘束を外す 
                m_rigidbody.constraints = RigidbodyConstraints.None;

                // ランダム旋回 
                Quaternion randomRot = Quaternion.AngleAxis(Random.Range(-180, 180), Vector3.up);
                Vector3 randomTroque = randomRot * Vector3.right;
                m_rigidbody.AddTorque(randomTroque * Random.Range(30, 180));

                Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Bad, Mathf.Clamp(transform.position.x / 9f, -0.4f, 0.4f));
            }
            m_rigidbody.AddForce(throwForce, ForceMode.VelocityChange);
        }

    }


}


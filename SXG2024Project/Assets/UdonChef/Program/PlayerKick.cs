using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class PlayerKick : MonoBehaviour
    {
        private Animator m_animator = null;
        private bool m_isKicking = false;
        private Rigidbody m_charaRb = null;
        [SerializeField] Transform m_collisionTr = null;
        [SerializeField] GameObject m_hitEfPrefab = null;


        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            gameObject.SetActive(false);

            UdonChef chef = GetComponentInParent<UdonChef>();
            if (chef != null)
            {
                m_charaRb = chef.GetComponent<Rigidbody>();
            }
        }

        public void Kick()
        {
            gameObject.SetActive(true);
            m_isKicking = true;
            // SE
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Kick, Mathf.Clamp(transform.position.x / 9f, -1.0f, 1.0f));
        }


        public bool IsKicking()
        {
            return m_isKicking;
        }

        const float TO_PLAYER_FORCE_POWER = 12;
        const float TO_FOOD_FORCE_POWER = 8;

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == SystemConstants.OBJ_LAYER_PLAYER)
            {
                // 自分なら無効 
                if (other.attachedRigidbody == m_charaRb)
                {
                    return;
                }
                // 吹き飛ばす 
                if (other.attachedRigidbody != null)
                {
                    PutForce(other.attachedRigidbody, new Vector3(0, 0.1f, 1), TO_PLAYER_FORCE_POWER);
                    // SE
                    Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Damage, Mathf.Clamp(transform.position.x / 9f, -1.0f, 1.0f));
                    // エフェクト
                    if(m_hitEfPrefab != null)
                    {
                        var ef = Instantiate(m_hitEfPrefab);
                        ef.transform.position = other.transform.position;
                    }
                }
            } else
            {
                if (other.attachedRigidbody != null)
                {
                    PutForce(other.attachedRigidbody, new Vector3(0, 1, 1), TO_FOOD_FORCE_POWER);
                }
            }
        }

        private void PutForce(Rigidbody targetRb, Vector3 dir, float power)
        {
            Vector3 forceDir = transform.rotation * dir.normalized;
            targetRb.AddForce(forceDir * power, ForceMode.VelocityChange);
        }



        public void AnimEvent_Finish()
        {
            gameObject.SetActive(false);
            m_isKicking = false;
        }
    }

}


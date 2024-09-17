using UnityEngine;


namespace SXG2024
{

    /// <summary>
    /// 奈落の底 
    /// </summary>
    public class AbyssPlane : MonoBehaviour
    {
        [SerializeField] private float m_respawnCircleRadius = 5.0f;
        [SerializeField] private float m_respawnPositionY = 10.0f;

        private void OnCollisionEnter(Collision collision)
        {
            switch (collision.gameObject.layer)
            {
                case SystemConstants.OBJ_LAYER_FOOD:
                case SystemConstants.OBJ_LAYER_PLAYER:
                    Rigidbody rb = collision.rigidbody;
                    if (rb != null)
                    {
                        // Respawn座標へワープ 
                        Vector3 respawnPosition = Vector3.up * m_respawnPositionY;
                        float angle = Random.Range(-Mathf.PI, Mathf.PI);
                        respawnPosition.x = m_respawnCircleRadius * Mathf.Cos(angle);
                        respawnPosition.z = m_respawnCircleRadius * Mathf.Sin(angle);
                        rb.position = respawnPosition;

                        // 速度零 
                        rb.velocity = Vector3.zero;
                    }
                    break;
            }
        }
    }


}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class ComTargetCircle : MonoBehaviour
    {
        [SerializeField] private Renderer m_renderer = null;
        [SerializeField] private LineRenderer m_lineRenderer = null;

        private int m_teamNo = 0;
        private float m_rotateAngle = 0;
        private Vector3 m_targetPosition = Vector3.zero;
        private Transform m_charaTr = null;
        private Vector3 m_linePositionOffset = Vector3.up * 0.01f;
        const float CIRCLE_RADIUS = 0.9f;

        /// <summary>
        /// 番号設定 
        /// </summary>
        /// <param name="teamNo"></param>
        /// <param name="teamColor"></param>
        public void SetNo(int teamNo, Color teamColor, Transform charaTr)
        {
            m_teamNo = teamNo;
            m_charaTr = charaTr;

            // circleのマテリアル 
            m_renderer.material = new Material(m_renderer.material);
            m_renderer.material.color = teamColor;

            // lineのマテリアル 
            m_lineRenderer.material = new Material(m_lineRenderer.material);
            m_lineRenderer.material.color = teamColor;

            m_rotateAngle = 90.0f * m_teamNo / 4;
        }

        /// <summary>
        /// 目標座標設定 
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="isVisible"></param>
        public void SetTargetPosition(Vector3 worldPosition, bool isVisible)
        {
            //transform.position = worldPosition;
            m_targetPosition = worldPosition;

            m_renderer.enabled = isVisible;
            m_lineRenderer.enabled = isVisible;
        }

        private void Update()
        {
            // 旋回 
            m_rotateAngle += 180.0f * Time.deltaTime;
            m_renderer.transform.localRotation = Quaternion.Euler(90, m_rotateAngle, 0);

            // 目的地 
            Vector3 newPosition = Vector3.Lerp(transform.position, m_targetPosition, (1.0f/0.1f)*Time.deltaTime);
            transform.position = newPosition;

            // ラインの座標設定 
            if (m_charaTr != null && m_lineRenderer.enabled)
            {
                Vector3 dir = newPosition - m_charaTr.position;
                dir.y = 0;
                float distance = dir.magnitude;
                if (CIRCLE_RADIUS < distance)
                {
                    dir = dir / distance;
                    m_lineRenderer.SetPosition(0, newPosition-dir*CIRCLE_RADIUS + m_linePositionOffset);
                    m_lineRenderer.SetPosition(1, m_charaTr.position + m_linePositionOffset);
                } else
                {
                    // 近すぎる時はラインは描画しない 
                    m_lineRenderer.enabled = false;
                }
            }
        }

    }


}


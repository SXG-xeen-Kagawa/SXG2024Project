using UnityEngine;

namespace SXG2024
{

    public class CoordinateSystemController
    {
        private int m_id = 0;
        private float m_localAngle = 0;
        private Matrix4x4 m_localToWorldMat;
        private Matrix4x4 m_worldToLocalMat;


        public CoordinateSystemController(int id, float angle)
        {
            Setup(id, angle);
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        /// <param name="id">ID番号</param>
        /// <param name="angle">角度(degree)</param>
        public void Setup(int id, float angle)
        {
            m_id = id;
            m_localAngle = angle;
            m_localToWorldMat = Matrix4x4.Rotate(Quaternion.Euler(0, angle, 0));
            m_worldToLocalMat = m_localToWorldMat.inverse;
        }


        /// <summary>
        /// World座標を取得 
        /// </summary>
        /// <param name="localPosition"></param>
        /// <returns></returns>
        public Vector3 GetWorldPosition(Vector3 localPosition)
        {
            return m_localToWorldMat * localPosition;
        }

        public void GetWorldPositionAndRotation(
            Vector3 localPosition, Quaternion localRotation,
            out Vector3 worldPosition, out Quaternion worldRotation)
        {
            worldPosition = m_localToWorldMat * localPosition;
            worldRotation = m_localToWorldMat.rotation * localRotation;
        }

        public void GetLocalPositionAndRotation(
            Vector3 worldPosition, Quaternion worldRotation,
            out Vector3 localPosition, out Quaternion localRotation)
        {
            localPosition = m_worldToLocalMat * worldPosition;
            localRotation = m_worldToLocalMat.rotation * worldRotation;
        }

        /// <summary>
        /// World座標からLocal座標を取得 
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public Vector3  GetLocalPositionFromWorld(Vector3 worldPosition)
        {
            return m_worldToLocalMat * worldPosition;
        }

        public Vector3  GetLocalVectorFromWorld(Vector3 worldVector)
        {
            return m_worldToLocalMat.rotation * worldVector;
        }

    }


}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class CharaRenderCamera : MonoBehaviour
    {
        private Camera m_camera = null;
        private RenderTexture m_renderTexture = null;

        public RenderTexture Texture => m_renderTexture;

        public enum CameraMode
        {
            ChallengerIntro,        // 挑戦者紹介 
        }

        [System.Serializable]
        public class CameraData
        {
            public Vector3 m_viewPoint = Vector3.zero;  // 視点 
            public Vector3 m_viewAngle = Vector3.zero;  // カメラ角度 
        }
        [SerializeField] private CameraData[] m_cameraData;

        private CameraMode m_cameraMode = CameraMode.ChallengerIntro;
        private Transform m_targetObjTr = null;


        private void Awake()
        {
            m_camera = GetComponent<Camera>();

            // RenderTextureを複製 
            m_renderTexture = Instantiate(m_camera.targetTexture);
            m_renderTexture.name = "CharaRenderCameraTexture";
            m_camera.targetTexture = m_renderTexture;

            // 活動停止 
            gameObject.SetActive(false);
        }


        /// <summary>
        /// レンダリング開始 
        /// </summary>
        /// <param name="targetCharaTr"></param>
        /// <param name="cameraMode"></param>
        public void StartRendering(Transform targetCharaTr, CameraMode cameraMode)
        {
            gameObject.SetActive(true);
            m_cameraMode = cameraMode;
            m_targetObjTr = targetCharaTr;

            // 一回座標更新しておく 
            UpdateCameraPositionAndRotation();
        }

        public void StopRendering()
        {
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            UpdateCameraPositionAndRotation();
        }

        private void UpdateCameraPositionAndRotation()
        {
            if ((int)m_cameraMode < m_cameraData.Length && m_targetObjTr != null)
            {
                var data = m_cameraData[(int)m_cameraMode];
                Vector3 localPosition = data.m_viewPoint;
                Quaternion localRotation = Quaternion.Euler(data.m_viewAngle);
                transform.SetPositionAndRotation(
                    m_targetObjTr.TransformPoint(localPosition),
                    m_targetObjTr.rotation * localRotation);
            }
        }


    }


}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace SXG2024
{

    public class PlayerName3DUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_nameText = null;
        private RectTransform m_rectTr = null;
        private Transform m_targetObjTr = null;
        private Vector3 m_offset3d = Vector3.zero;
        private Canvas3DRoot.Calc2dPositionDelegate m_calcPositionFunction = null;

        private void Awake()
        {
            m_rectTr = GetComponent<RectTransform>();
        }

        /// <summary>
        /// セットアップ 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj3dTr"></param>
        /// <param name="offset3d"></param>
        /// <param name="outlineColor"></param>
        public void Setup(string name, Transform obj3dTr, Vector3 offset3d, Color outlineColor,
            Canvas3DRoot.Calc2dPositionDelegate calcPositionFunction)
        {
            m_nameText.text = string.Format("{0}\n▼", name);
            m_targetObjTr = obj3dTr;
            m_offset3d = offset3d;
            m_calcPositionFunction = calcPositionFunction;

            // outline
            m_nameText.fontSharedMaterial = Instantiate(m_nameText.fontSharedMaterial);
            //m_nameText.material = Instantiate(m_nameText.material);
            m_nameText.fontSharedMaterial.SetColor(Shader.PropertyToID("_OutlineColor"), outlineColor);
        }


        private void LateUpdate()
        {
            if (m_targetObjTr!=null)
            {
                m_rectTr.anchoredPosition = m_calcPositionFunction(m_targetObjTr.position + m_offset3d);
            }
        }
    }


}


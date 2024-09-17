using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class StageModel : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] m_shippingTableSheets = null;   // 出荷テーブルの上のシート

        public void Setup(Color [] colorTable)
        {
            for (int i=0; i < m_shippingTableSheets.Length; ++i)
            {
                if (i < colorTable.Length)
                {
                    var sheetRenderer = m_shippingTableSheets[i];
                    sheetRenderer.material = new Material(sheetRenderer.material);
                    sheetRenderer.material.color = colorTable[i];
                }
            }
        }
    }


}


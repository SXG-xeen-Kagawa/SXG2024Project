using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace SXG2024
{

    public class ChallengerIntroOne : MonoBehaviour
    {
        [SerializeField] private Image [] m_imagesDependOnTeamColor = null;   // チームカラーに依存する画像 

        [SerializeField] private RawImage m_renderTextureImage = null;
        [SerializeField] private NamePlateUI m_namePlate = null;


        /// <summary>
        /// セットアップ 
        /// </summary>
        /// <param name="comPlayer"></param>
        /// <param name="teamColor"></param>
        public void Setup(ComPlayerBase comPlayer, int teamNo, Color teamColor, Texture charaTexture)
        {
            m_renderTextureImage.texture = charaTexture;

            m_namePlate.Setup(comPlayer, teamNo);

            // チームカラー設定 
            foreach (var image in m_imagesDependOnTeamColor)
            {
                Color originalColor = image.color;
                originalColor.r = teamColor.r;
                originalColor.g = teamColor.g;
                originalColor.b = teamColor.b;
                image.color = originalColor;
            }
        }

    }


}


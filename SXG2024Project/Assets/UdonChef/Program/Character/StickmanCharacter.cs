

using UnityEngine;

namespace SXG2024
{

    public class StickmanCharacter : DisplayCharacterBase
    {
        [SerializeField] private SkinnedMeshRenderer m_meshRenderer = null;

        /// <summary>
        /// セットアップ 
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="baseColor"></param>
        public override void Setup(int playerId, Color baseColor)
        {
            base.Setup(playerId, baseColor);

            m_meshRenderer.material = Instantiate(m_meshRenderer.material);
            m_meshRenderer.material.color = baseColor;
        }
    }


}


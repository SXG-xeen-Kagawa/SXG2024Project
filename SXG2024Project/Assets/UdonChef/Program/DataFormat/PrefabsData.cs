using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    [CreateAssetMenu(menuName = "SXG2024/Create PrefabsData")]
    public class PrefabsData : ScriptableObject
    {
        public UdonChef m_udonChefPrefab = null;
        public UdonBowl m_udonBowlPrefab = null;

        public ComTargetCircle m_comTargetCirclePrefab = null;

        public CharaRenderCamera m_charaRenderCameraPrefab = null;

        public SimpleCapsuleCharacter m_simpleCapsuleCharacter = null;
        public MaleCitizenCharacter m_maleCitizenCharacter = null;

    }

}


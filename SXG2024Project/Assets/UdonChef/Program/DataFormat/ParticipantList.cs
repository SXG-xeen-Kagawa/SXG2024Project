using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    [CreateAssetMenu(menuName = "SXG2024/Create ParticipantList")]
    public class ParticipantList : ScriptableObject
    {
        public List<ComPlayerBase> m_comPlayers;
    }


}


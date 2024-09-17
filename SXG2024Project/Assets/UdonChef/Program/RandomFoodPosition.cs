using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class RandomFoodPosition
    {
        private Quaternion m_rotation = Quaternion.Euler(0, 45, 0);
        const float STAGE_SIZE = 5.0f;

        public Vector3  Get()
        {
            Vector3 local = Vector3.zero;
            local.x = Random.Range(-STAGE_SIZE, +STAGE_SIZE);
            local.z = Random.Range(-STAGE_SIZE, +STAGE_SIZE);
            return m_rotation * local;
        }
    }


}


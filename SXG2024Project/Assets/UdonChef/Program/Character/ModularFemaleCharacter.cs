using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class ModularFemaleCharacter : DisplayCharacterBase
    {
        [SerializeField] GameObject[] m_shirtModels = null;
        [SerializeField] GameObject[] m_pantsModels = null;
        [SerializeField] GameObject[] m_hairModels = null;
        [SerializeField] GameObject[] m_eyeModels = null;

        [SerializeField] private Color m_hairBaseColor = new Color(0.8f, 0.8f, 0.8f);


        /// <summary>
        /// セットアップ 
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="baseColor"></param>
        public override void Setup(int playerId, Color baseColor)
        {
            base.Setup(playerId, baseColor);

            // シャツを選択 
            ChoiceModel(m_shirtModels, playerId+Random.Range(0,10), true, baseColor, 0);

            // パンツを選択 
            ChoiceModel(m_pantsModels, playerId + Random.Range(0, 10), true, baseColor, 0);

            // 髪を選択
            ChoiceModel(m_hairModels, playerId + Random.Range(0, 10), true, m_hairBaseColor, 0.5f);

            // 目を選択 
            ChoiceModel(m_eyeModels, playerId + Random.Range(0, 10), false, baseColor, 0);
        }



        /// <summary>
        /// モデルを選択 
        /// </summary>
        /// <param name="modelObjs"></param>
        /// <param name="selectKey"></param>
        /// <param name="baseColor"></param>
        private void ChoiceModel(GameObject [] modelObjs, 
            int selectKey, bool isChangeMaterial, Color baseColor, float randomRange)
        {
            var selectedObj = modelObjs[selectKey%modelObjs.Length];
            foreach (var obj in modelObjs)
            {
                if (selectedObj == obj)
                {
                    obj.SetActive(true);
                    if (isChangeMaterial)
                    {
                        var renderer = obj.GetComponent<SkinnedMeshRenderer>();
                        renderer.material = Instantiate(renderer.material);
                        renderer.material.color = baseColor;
                        if (0 < randomRange)
                        {
                            //renderer.material.mainTextureOffset = Vector3.right * Random.Range(0.0f, randomRange);
                            renderer.material.mainTextureOffset =
                                new Vector2(Random.Range(0.0f, randomRange), Random.Range(0.0f, randomRange));
                        }
                    }
                } else
                {
                    obj.SetActive(false);
                }
            }
        }

    }


}


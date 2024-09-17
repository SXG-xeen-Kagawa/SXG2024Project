using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class DisplayCharacterBase : MonoBehaviour
    {
        [SerializeField] protected Animator m_animator = null;

        [SerializeField] protected Renderer[] m_bodyRenderers = null;
        [SerializeField] protected Renderer m_faceBoardRenderer = null;

        protected readonly int SHADER_PROPERTY_OUTLINE_COLOR = Shader.PropertyToID("_OutLineColor");
        protected readonly int SHADER_PROPERTY_OUTLINE_THICKNESS = Shader.PropertyToID("_OutLineThickness");

        [SerializeField] protected float m_outlineColorRate = 0.65f;  // アウトライン色をBaseColorからの倍率で指定
        [SerializeField] protected float m_outlineThickness = 0.05f;



        protected int m_playerId = 0;

        readonly int ANIM_TRIG_DANCE = Animator.StringToHash("Dance");
        readonly int ANIM_TRIG_IN_GAME = Animator.StringToHash("InGame");
        readonly int ANIM_TRIG_KICK = Animator.StringToHash("Kick");
        readonly int ANIM_TRIG_WIN = Animator.StringToHash("Win");
        readonly int ANIM_TRIG_LOSE = Animator.StringToHash("Lose");
        readonly int ANIM_BOOL_IS_RUN = Animator.StringToHash("IsRun");
        readonly int ANIM_FLOAT_RUN_SPEED = Animator.StringToHash("RunSpeed");

        public enum AnimationState
        {
            GameIdle,
            GameRun,
            Intro,
            Kick,
            Win,
            Lose,
        }



        private void Awake()
        {
            if (m_animator != null)
            {
                m_animator.SetLayerWeight(1, 0);
            }
        }

        /// <summary>
        /// セットアップ 
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="baseColor"></param>
        public virtual void Setup(int playerId, Color baseColor)
        {
            m_playerId = playerId;

            Color outlineColor =
                new Color(baseColor.r * m_outlineColorRate, baseColor.g * m_outlineColorRate, baseColor.b * m_outlineColorRate);

            // ベースカラー設定 
            foreach (var rend in m_bodyRenderers)
            {
                rend.material = Instantiate(rend.material);
                rend.material.color = baseColor;

                // アウトラインカラー設定 
                if (rend.material.HasProperty(SHADER_PROPERTY_OUTLINE_COLOR))
                {
                    rend.material.SetColor(SHADER_PROPERTY_OUTLINE_COLOR, outlineColor);
                }
                if (rend.material.HasProperty(SHADER_PROPERTY_OUTLINE_THICKNESS))
                {
                    rend.material.SetFloat(SHADER_PROPERTY_OUTLINE_THICKNESS, m_outlineThickness);
                }
            }

            // 顔画像用マテリアルを複製 
            m_faceBoardRenderer.material = Instantiate(m_faceBoardRenderer.material);
        }

        public virtual void SetupExtraData(string playerName, Sprite faceImage)
        {
            SetFaceSprite(faceImage);
        }

        /// <summary>
        /// 顔画像スプライトを設定 
        /// </summary>
        /// <param name="faceSprite"></param>
        private void SetFaceSprite(Sprite faceSprite)
        {
            m_faceBoardRenderer.material.SetTexture("_MainTex", faceSprite.texture);
        }




        public void SetAnimation(AnimationState animState)
        {
            if (m_animator == null)
            {
                return;
            }
            switch (animState)
            {
                case AnimationState.GameIdle:
                    m_animator.SetLayerWeight(1, 1);
                    m_animator.SetTrigger(ANIM_TRIG_IN_GAME);
                    m_animator.SetBool(ANIM_BOOL_IS_RUN, false);
                    break;
                case AnimationState.GameRun:
                    m_animator.SetLayerWeight(1, 1);
                    m_animator.SetTrigger(ANIM_TRIG_IN_GAME);
                    m_animator.SetBool(ANIM_BOOL_IS_RUN, true);
                    break;
                case AnimationState.Intro:
                    m_animator.SetLayerWeight(1, 0);
                    m_animator.SetTrigger(ANIM_TRIG_DANCE);
                    break;
                case AnimationState.Kick:
                    //m_animator.SetLayerWeight(1, 1);
                    m_animator.SetTrigger(ANIM_TRIG_KICK);
                    //m_animator.SetBool(ANIM_BOOL_IS_RUN, true);
                    break;
                case AnimationState.Win:
                    m_animator.SetLayerWeight(1, 0);
                    m_animator.SetTrigger(ANIM_TRIG_WIN);
                    break;
                case AnimationState.Lose:
                    m_animator.SetLayerWeight(1, 0);
                    m_animator.SetTrigger(ANIM_TRIG_LOSE);
                    break;
            }
        }

        public void SetAnimationRunSpeed(float runSpeed)
        {
            if (m_animator!=null)
            {
                m_animator.SetFloat(ANIM_FLOAT_RUN_SPEED, runSpeed);
            }
        }
    }

}


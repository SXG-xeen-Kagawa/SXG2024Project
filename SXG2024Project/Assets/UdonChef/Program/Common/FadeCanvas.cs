using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SXG2024
{

    public class FadeCanvas : MonoBehaviour
    {
        private static FadeCanvas ms_instance = null;

        const float DEFAULT_FADE_TIME = 0.5f;

        public static FadeCanvas Instance => ms_instance;

        [SerializeField] private Image m_fadeImage = null;


        [RuntimeInitializeOnLoadMethod()]
        static void OnLoadMethod()
        {
            if (ms_instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("FadeCanvas");
                if (prefab != null)
                {
                    Instantiate(prefab);
                }
            }
        }

        private void Awake()
        {
            ms_instance = this;
            DontDestroyOnLoad(this.gameObject);
        }


        /// <summary>
        /// �t�F�[�h�A�E�g 
        /// </summary>
        /// <param name="fadeTime"></param>
        public void FadeOut(float fadeTime=DEFAULT_FADE_TIME)
        {
            StopAllCoroutines();
            StartCoroutine(CoFade(1.0f, fadeTime));
        }

        /// <summary>
        /// �t�F�[�h�C�� 
        /// </summary>
        /// <param name="fadeTime"></param>
        public void FadeIn(float fadeTime=DEFAULT_FADE_TIME)
        {
            StopAllCoroutines();
            StartCoroutine(CoFade(0.0f, fadeTime));
        }


        private IEnumerator CoFade(float targetAlpha, float time)
        {
            Color fadeColor = m_fadeImage.color;
            float startAlpha = fadeColor.a;
            float animTime = Mathf.Abs(targetAlpha - fadeColor.a) * time;
            float localTime = 0;
            m_fadeImage.enabled = true;
            while (localTime < animTime)
            {
                localTime += Time.deltaTime;
                fadeColor.a = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(localTime / animTime));
                m_fadeImage.color = fadeColor;

                yield return null;
            }

            // ���� 
            fadeColor.a = targetAlpha;
            m_fadeImage.color = fadeColor;

            // ��\���H 
            if (targetAlpha <= 0)
            {
                m_fadeImage.enabled = false;
            }
        }


    }

}


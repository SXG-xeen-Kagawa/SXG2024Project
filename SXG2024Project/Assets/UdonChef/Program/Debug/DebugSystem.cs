using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class DebugSystem : MonoBehaviour
    {
        public static DebugSystem ms_instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnGameStart()
        {
            if (ms_instance == null)
            {
                // プレハブをロードしてインスタンスとして保持
                GameObject prefab = Resources.Load<GameObject>("DebugSystemCanvas");
                ms_instance = Instantiate(prefab).GetComponent<DebugSystem>();

                // シーン遷移時に破棄されないように設定
                DontDestroyOnLoad(ms_instance.gameObject);
            }
        }


        // Start is called before the first frame update
        void Start()
        {

        }
    }

}


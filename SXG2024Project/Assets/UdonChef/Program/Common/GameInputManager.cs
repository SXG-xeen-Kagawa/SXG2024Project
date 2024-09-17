using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SXG2024
{

    public class GameInputManager : MonoBehaviour
    {
        public static GameInputManager ms_instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnGameStart()
        {
            if (ms_instance == null)
            {
                // プレハブをロードしてインスタンスとして保持
                GameObject prefab = Resources.Load<GameObject>("GameInputManager");
                ms_instance = Instantiate(prefab).GetComponent<GameInputManager>();

                // シーン遷移時に破棄されないように設定
                DontDestroyOnLoad(ms_instance.gameObject);
            }
        }


        public static GameInputManager Instance => ms_instance;

        public delegate void OnPressedKeyDelegate();


        public enum Type
        {
            Decide,     // 決定 
            SpeedUp,
            SpeedDown,
        }
        private bool[] m_wasPressed;

        private OnPressedKeyDelegate OnPressedKeyCallback_Decide = null;
        private OnPressedKeyDelegate OnPressedKeyCallback_SpeedUp = null;
        private OnPressedKeyDelegate OnPressedKeyCallback_SpeedDown = null;


        // Start is called before the first frame update
        void Start()
        {
            m_wasPressed = new bool[Enum.GetValues(typeof(Type)).Length];
            ResetAll();
        }

        private void ResetAll()
        {
            for (int i = 0; i < m_wasPressed.Length; ++i)
            {
                m_wasPressed[i] = false;
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            ResetAll();
        }


        /// <summary>
        /// 押した？ 
        /// </summary>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public bool WasPressed(Type inputType)
        {
            return m_wasPressed[(int)inputType];
        }


        public void SetCallback(Type type, OnPressedKeyDelegate callback)
        {
            switch (type)
            {
                case Type.Decide:
                    OnPressedKeyCallback_Decide += callback;
                    break;
                case Type.SpeedUp:
                    OnPressedKeyCallback_SpeedUp += callback;
                    break;
                case Type.SpeedDown:
                    OnPressedKeyCallback_SpeedDown += callback;
                    break;
            }
        }



        #region Callback

        public void OnDecide(InputAction.CallbackContext context)
        {
            //Debug.Log(string.Format("OnDecide: phase={0} time={1} | T={2}", 
            //    context.phase, context.time, Time.frameCount));
            if (context.phase == InputActionPhase.Started)
            {
                m_wasPressed[(int)Type.Decide] = true;

                if (OnPressedKeyCallback_Decide != null)
                {
                    OnPressedKeyCallback_Decide.Invoke();
                }
            }
        }

        public void OnSpeedUp(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                m_wasPressed[(int)Type.SpeedUp] = true;

                if (OnPressedKeyCallback_SpeedUp != null)
                {
                    OnPressedKeyCallback_SpeedUp.Invoke();
                }
            }
        }

        public void OnSpeedDown(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                m_wasPressed[(int)Type.SpeedDown] = true;

                if (OnPressedKeyCallback_SpeedDown != null)
                {
                    OnPressedKeyCallback_SpeedDown.Invoke();
                }
            }
        }

        #endregion


    }


}


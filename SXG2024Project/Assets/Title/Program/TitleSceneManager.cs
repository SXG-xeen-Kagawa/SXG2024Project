using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


namespace SXG2024
{

    public class TitleSceneManager : MonoBehaviour
    {
        // Start is called before the first frame update
        IEnumerator Start()
        {
            // フェードイン
            FadeCanvas.Instance.FadeIn();
            yield return new WaitForSeconds(0.5f);

            // BGM再生
            Effect.SoundController.instance?.PlayBGM(Effect.SoundController.BGMType.Title);

            // キー入力待ち 
            while (!WasPressedKey())
            {
                yield return null;
            }
            // SE
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Start);

            // フェードアウト
            FadeCanvas.Instance.FadeOut();
            yield return new WaitForSeconds(0.5f);

            // BGM停止
            Effect.SoundController.instance?.FadeOutBGM();
            // シーン遷移 
            SceneManager.LoadSceneAsync("ParticipantSelection");
        }


        private bool WasPressedKey()
        {
            return GameInputManager.Instance.WasPressed(GameInputManager.Type.Decide);
        }

    }


}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SXG2024
{
    public class ParticipantSelectionSceneManager : MonoBehaviour
    {
        [SerializeField]
        ParticipantList m_participantList = null;

        [SerializeField]
        ParticipantsList m_participantsList = null;

        [SerializeField]
        ParticipantScrollView m_participantScrollView = null;

        [SerializeField]
        Button m_decideButton = null;

        IEnumerator Start()
        {
            // BGM再生
            Effect.SoundController.instance?.PlayBGM(Effect.SoundController.BGMType.Setting);
            // 前回の参加者の設定をリセット
            for (var i = 0; i < GameConfigSetting.Participants.Length; i++)
                GameConfigSetting.Participants[i] = -1;

            // 参加者一覧
            m_participantScrollView.Setup(m_participantList.m_comPlayers.ToArray(),
                index =>
                {
                    // 参加者を設定
                    var selectParticipant = m_participantList.m_comPlayers[index];
                    m_participantsList.SetPlayer(selectParticipant);
                    GameConfigSetting.Participants[m_participantsList.PlayerIndex] = index;

                    // カーソルをひとつ右に移動
                    m_participantsList.SetCursor(m_participantsList.PlayerIndex + 1);

                    // 参加者全員決まれば決定ボタンを押せる
                    var canPlayGame = true;
                    foreach (var participant in GameConfigSetting.Participants)
                    {
                        if (participant == -1)
                            canPlayGame = false;
                    }
                    m_decideButton.interactable = canPlayGame;
                });

            // 決定ボタン操作
            m_decideButton.onClick.AddListener(
                () =>
                {
                    StartCoroutine(CoGoToGame());
                });
            m_decideButton.interactable = false;

            // フェードイン
            FadeCanvas.Instance.FadeIn();
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator CoGoToGame()
        {
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Start);
            // BGM停止
            Effect.SoundController.instance?.FadeOutBGM();
            // フェードアウト
            FadeCanvas.Instance.FadeOut();
            yield return new WaitForSeconds(0.5f);

            // シーン遷移 
            SceneManager.LoadSceneAsync("Game");
        }
    }
}

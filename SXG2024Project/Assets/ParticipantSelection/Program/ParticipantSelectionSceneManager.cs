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
            // BGM�Đ�
            Effect.SoundController.instance?.PlayBGM(Effect.SoundController.BGMType.Setting);
            // �O��̎Q���҂̐ݒ�����Z�b�g
            for (var i = 0; i < GameConfigSetting.Participants.Length; i++)
                GameConfigSetting.Participants[i] = -1;

            // �Q���҈ꗗ
            m_participantScrollView.Setup(m_participantList.m_comPlayers.ToArray(),
                index =>
                {
                    // �Q���҂�ݒ�
                    var selectParticipant = m_participantList.m_comPlayers[index];
                    m_participantsList.SetPlayer(selectParticipant);
                    GameConfigSetting.Participants[m_participantsList.PlayerIndex] = index;

                    // �J�[�\�����ЂƂE�Ɉړ�
                    m_participantsList.SetCursor(m_participantsList.PlayerIndex + 1);

                    // �Q���ґS�����܂�Ό���{�^����������
                    var canPlayGame = true;
                    foreach (var participant in GameConfigSetting.Participants)
                    {
                        if (participant == -1)
                            canPlayGame = false;
                    }
                    m_decideButton.interactable = canPlayGame;
                });

            // ����{�^������
            m_decideButton.onClick.AddListener(
                () =>
                {
                    StartCoroutine(CoGoToGame());
                });
            m_decideButton.interactable = false;

            // �t�F�[�h�C��
            FadeCanvas.Instance.FadeIn();
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator CoGoToGame()
        {
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Start);
            // BGM��~
            Effect.SoundController.instance?.FadeOutBGM();
            // �t�F�[�h�A�E�g
            FadeCanvas.Instance.FadeOut();
            yield return new WaitForSeconds(0.5f);

            // �V�[���J�� 
            SceneManager.LoadSceneAsync("Game");
        }
    }
}

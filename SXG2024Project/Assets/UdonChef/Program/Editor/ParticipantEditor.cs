using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;

namespace SXG2024
{
    public class ParticipantEditor : EditorWindow
    {
        [MenuItem("SXG2024/����ҍ쐬")]
        private static void OpenWindow()
        {
            var window = GetWindow<ParticipantEditor>();
            window.titleContent = new GUIContent("����ҍ쐬");
        }

        /// <summary>
        /// �摜�t�@�C���p�X
        /// </summary>
        private string m_iconPath = "";

        private class Data : ScriptableSingleton<Data>
        {
            /// <summary>
            /// �\�����ɔ��s�����Q���ԍ��i3���j
            /// </summary>
            public int participantID = 0;
            /// <summary>
            /// ������
            /// </summary>
            public string organization = "����";
            /// <summary>
            /// ����Җ�
            /// </summary>
            public string playerName = "�����";
            /// <summary>
            /// �摜�X�v���C�g
            /// </summary>
            public Sprite sprite = null;

            public bool isRunnning = false;

            public string GetName()
                => $"Player{participantID:D3}";
        }

        private void OnGUI()
        {
            var data = Data.instance;

            GUILayout.Space(10);
            GUILayout.Label("���K�{����");

            // �Q���ԍ�
            var participantID = EditorGUILayout.IntField("�@��t�ԍ�:", data.participantID);
            data.participantID = Mathf.Clamp(participantID, 0, 999);
            GUILayout.Label("�@��connpass�G���g���[���ɔ��s���ꂽ��t�ԍ�����͂��Ă�������");

            GUILayout.Space(10);
            GUILayout.Label("���C�Ӎ��ځi�ォ��ύX�j");

            // ������
            const int MAX_NUM = 10; // �ő啶����
            data.organization = EditorGUILayout.TextField("�@������:", data.organization);
            if (MAX_NUM < data.organization.Length)
                data.organization = data.organization.Remove(MAX_NUM, data.organization.Length - MAX_NUM);
            // ����Җ�
            data.playerName = EditorGUILayout.TextField("�@����Җ�:", data.playerName);
            if (MAX_NUM < data.playerName.Length)
                data.playerName = data.playerName.Remove(MAX_NUM, data.playerName.Length - MAX_NUM);
            // �A�C�R���摜
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField("�@�A�C�R���摜�t�@�C���I��:", m_iconPath);
                if (GUILayout.Button("�Q��", GUILayout.Width(50), GUILayout.Height(18)))
                {
                    var path = EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
                    m_iconPath = path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                    GUI.FocusControl("");
                }
            }
            GUILayout.Label("�@���������E����Җ��́A�S�p���p��킸 �ő�10���� �Ƃ��Ă�������");
            GUILayout.Label("�@���A�C�R���摜�t�@�C���̍ő�T�C�Y�� 256*256 �ł�");
            GUILayout.Label("�@�������Ǒ��ɔ�����摜�▼�O�͐ݒ肵�Ȃ��ł�������");

            GUILayout.Space(10);

            if (GUILayout.Button("�쐬", GUILayout.Height(32)))
            {
                AddParticipant();
            }
        }

        private void AddParticipant()
        {
            var data = Data.instance;
            var name = data.GetName();
            
            var folderPath = $"Assets/Participant/{name}";
            
            if (Directory.Exists(folderPath))
            {
                Debug.LogError($"�Q���ԍ�:{data.participantID} �̒���҂͊��ɍ쐬�ς݂ł��B - {folderPath}");
                return;
            }
            
            // �t�H���_�쐬
            Directory.CreateDirectory(folderPath);

            // �摜�쐬
            if (File.Exists(m_iconPath))
            {
                var iconExtension = Path.GetExtension(m_iconPath);
                var iconPath = $"{folderPath}/{name}{iconExtension}";
                File.Copy(m_iconPath, iconPath, true);

                AssetDatabase.Refresh();

                // �摜�̃e�N�X�`���^�C�v�ύX
                var textureImporter = AssetImporter.GetAtPath(iconPath) as TextureImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.SaveAndReimport();

                var iconAsset = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                data.sprite = iconAsset;
            }

            // cs�t�@�C���쐬
            var csTemplatePath = "Assets/UdonChef/Program/Editor/SXGParticipant.cs.txt";
            var csPath = $"{folderPath}/{name}.cs";
            var csText = File.ReadAllText(csTemplatePath);
            csText = csText.Replace("#SCRIPTNAME#", name);
            File.WriteAllText(csPath, csText, Encoding.GetEncoding("utf-8"));

            data.isRunnning = true;

            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation();
        }

        [DidReloadScripts]
        public static void OnDidReloadScripts()
        {
            var data = Data.instance;
            var name = data.GetName();

            if (!data.isRunnning)
                return;
            data.isRunnning = false;

            // Prefab�p�Q�[���I�u�W�F�N�g�쐬
            var prefabObj = new GameObject(name);
            var type = Type.GetType($"{name}.{name}, Assembly-CSharp"); // ���O��ԂƃA�Z���u�����܂߂�
            var component = prefabObj.AddComponent(type) as ComPlayerBase;
            component.SetPlayerData(data.organization, data.playerName, data.sprite);

            // Prefab�쐬
            var folderPath = $"Assets/Participant/{name}";
            var prefabPath = $"{folderPath}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefabObj, prefabPath);
            DestroyImmediate(prefabObj);

            AssetDatabase.Refresh();

            EditorApplication.delayCall += () =>
            {
                // AI���X�g�o�^
                var participantListPath = "Assets/GameAssets/Data/ParticipantList.asset";
                var participantListAsset = AssetDatabase.LoadAssetAtPath<ParticipantList>(participantListPath);
                var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                participantListAsset.m_comPlayers.Insert(0, prefabAsset.GetComponent<ComPlayerBase>());

                EditorUtility.SetDirty(participantListAsset);
                AssetDatabase.SaveAssets();

                // Prefab�t�H�[�J�X
                Selection.activeObject = prefabAsset;
                EditorGUIUtility.PingObject(prefabAsset);

                EditorUtility.DisplayDialog("����ғo�^", $"�Q���ԍ�:{Data.instance.participantID} ��AI��o�^���܂����B\n{folderPath}", "OK");
            };
        }
    }
}

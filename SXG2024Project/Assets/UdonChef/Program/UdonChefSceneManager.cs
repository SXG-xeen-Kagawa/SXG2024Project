using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


namespace SXG2024
{

    public class UdonChefSceneManager : MonoBehaviour
    {
        [SerializeField] private PrefabsData m_prefabsData = null;

        [SerializeField] private ParticipantList m_participantList = null;      // 参加者AIリスト 

        [SerializeField] private FoodsData m_foodsData = null;      // 食材情報 

        [SerializeField] private GameData m_gameData = null;        // ゲームデータ 

        [SerializeField] private MenuData m_menuData = null;        // メニュー情報

        [SerializeField] private GameObject m_objStage = null;

        [SerializeField] private Vector3 m_startPlayerPosition = Vector3.forward * (-5);

        [SerializeField] private CountDownToStartUI m_countDownUI = null;
        [SerializeField] private RemainingTimeUI m_remainingTimeUI = null;
        [SerializeField] private PlayersScoreRootUI m_playersScoreRootUI = null;
        [SerializeField] private Canvas3DRoot m_canvas3dRoot = null;
        [SerializeField] private GameSetUI m_gameSetUI = null;
        [SerializeField] private ChallengersIntroScreen m_challengersIntroScreenUI = null;
        [SerializeField] private ResultScreen m_resultScreenUI = null;
        [SerializeField] private CanvasController m_mainCanvasController = null;

        [SerializeField] private Color[] m_gameTeamColors = new Color[4];   // チームカラー 

        [SerializeField] private Camera m_mainCamera = null;

        [SerializeField] private OutlineRenderPassFeature m_outlineRenderPassFeature = null;

        public enum CharacterType
        {
            SimpleCapsule,
            MaleCitizen,
        }
        [SerializeField] CharacterType m_displayCharacterType = CharacterType.MaleCitizen;
        [SerializeField] GameObject m_resultEffect = null;


        private Menu m_menuCheck = null;   // メニュー判定用クラス 

        private FoodsCreator m_foodsCreator = null;

        private float m_gamePlayTime = 0;

        private List<CharaRenderCamera> m_charaRenderCameraList = new();

        private static UdonChefSceneManager ms_instance = null;

        public class PlayerEntrySheet
        {
            public int m_id = 0;
            public UdonChef m_udonChef = null;  // 見た目のモデル 
            public ComPlayerBase m_comPlayer = null;    // AI処理 
            public CoordinateSystemController m_coordinateSys = null;   // 座標系管理システム 
            public List<FoodType> m_foodsListOnHand = new();    // 手に持っている食材リスト 
            public List<int> m_foodsIdListOnHand = new();
            public UdonBowl m_udonBowl = null;      // うどん鉢 
            public ResultBook m_resultBook = new();
            public ComTargetCircle m_targetCircle = null;


            /// <summary>
            /// 出荷可能判定 (メニューが成立しているかどうか)
            /// </summary>
            /// <returns></returns>
            public bool CanShip(Menu menuCheck)
            {
                if (0 < m_foodsListOnHand.Count)
                {
                    var foods = m_foodsListOnHand.ToArray();
                    int price = 0;
                    string menuName = "";
                    if (menuCheck.Check(foods, out price, out menuName, out int goodFoodsCount, out int badFoodsCount))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        private List<PlayerEntrySheet> m_playerEntrySheetList = new();



        public enum SceneFlow
        {
            Initialize,
            ChallengersIntro,
            CountDown,
            Playing,
            Result,
            Finish,

            None
        }
        private SceneFlow m_sceneFlow = SceneFlow.None;
        public SceneFlow sceneFlow => m_sceneFlow;



        //public static UdonChefSceneManager Instance => ms_instance;

        private bool m_isUpdateComs = false;    // COMの更新フラグ
        private bool m_isScoreAcceptable = false;   // スコア受付可能 


        private void Awake()
        {
            ms_instance = this;
        }



        // Start is called before the first frame update
        void Start()
        {
            Setup();

            // 開始 
            ChangeSceneFlow(SceneFlow.Initialize);
        }

        private void Setup()
        {
            m_foodsCreator = new(m_foodsData, m_gameData);
            m_menuCheck = new(m_menuData);
        }



        private void Update()
        {
            // COMの更新 
            if (m_isUpdateComs)
            {
                foreach (var playerSheet in m_playerEntrySheetList)
                {
                    // ふるまいデータを取得 
                    var behaviourData = playerSheet.m_comPlayer.GetComBehaviourDataAndReset();

                    // 硬直時間 
                    if (playerSheet.m_udonChef.IsStiffness())
                    {
                        // debug:目標座標設定 
                        playerSheet.m_targetCircle.SetTargetPosition(Vector3.zero, false);
                    }
                    // キック行動 
                    else if (behaviourData.m_isKick)
                    {
                        playerSheet.m_udonChef.Kick();

                        // debug:目標座標設定 
                        playerSheet.m_targetCircle.SetTargetPosition(Vector3.zero, false);
                    }
                    // 走る行動 
                    else if (0 < behaviourData.m_speedRate)
                    {
                        Vector3 worldPosition = playerSheet.m_coordinateSys.GetWorldPosition(behaviourData.m_targetPosition);
                        playerSheet.m_udonChef.MoveTo(worldPosition, behaviourData.m_speedRate);

                        // debug:目標座標設定 
                        Vector3 targetPosition = worldPosition;
                        targetPosition.y = playerSheet.m_udonChef.transform.position.y+0.001f;
                        playerSheet.m_targetCircle.SetTargetPosition(targetPosition, true);
                    }
                    // 行動無し 
                    else
                    {
                        playerSheet.m_udonChef.MoveTo(Vector3.zero, 0);

                        // debug:目標座標設定 
                        playerSheet.m_targetCircle.SetTargetPosition(Vector3.zero, false);
                    }
                }
            }

        }




        /// <summary>
        /// シーンフローを変更 
        /// </summary>
        /// <param name="nextSceneFlow"></param>
        private void ChangeSceneFlow(SceneFlow nextSceneFlow)
        {
            if (m_sceneFlow != nextSceneFlow)
            {
                m_sceneFlow = nextSceneFlow;
                switch (m_sceneFlow)
                {
                    case SceneFlow.Initialize:
                        StartCoroutine(CoSceneInit());
                        break;
                    case SceneFlow.ChallengersIntro:
                        StartCoroutine(CoSceneChallengersIntro());
                        break;
                    case SceneFlow.CountDown:
                        StartCoroutine(CoSceneCountDown());
                        break;
                    case SceneFlow.Playing:
                        StartCoroutine(CoSceneGamePlaying());
                        break;
                    case SceneFlow.Result:
                        StartCoroutine(CoSceneResult());
                        break;
                    case SceneFlow.Finish:
                        StartCoroutine(CoSceneFinish());
                        break;
                }
            }
        }

        /// <summary>
        /// Scene管理：初期化
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoSceneInit()
        {
            // 初期化 
            m_isUpdateComs = false;
            m_isScoreAcceptable = false;

            // 残り時間 
            m_remainingTimeUI.SetTime(m_gameData.TotalPlayTime);

            // ステージカラー設定 
            StageModel stageModel = m_objStage.GetComponent<StageModel>();
            stageModel.Setup(m_gameTeamColors);

            // プレイヤー生成 
            for (int i=0; i < GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE; ++i)
            {
                PlayerEntrySheet entrySheet = new();
                entrySheet.m_id = i;

                // 座標系管理 
                entrySheet.m_coordinateSys = new(i, 45.0f+90.0f*i);

                // 初期位置 
                Vector3 worldPosition;
                Quaternion worldRotation;
                entrySheet.m_coordinateSys.GetWorldPositionAndRotation(
                    m_startPlayerPosition, Quaternion.identity,
                    out worldPosition, out worldRotation);

                // 見た目のプレイヤー配置 
                entrySheet.m_udonChef = Instantiate(m_prefabsData.m_udonChefPrefab, this.transform);
                entrySheet.m_udonChef.SetPlayer(i, m_gameTeamColors[i]);
                entrySheet.m_udonChef.name = Utility.MakeRandomString(12);
                entrySheet.m_udonChef.transform.SetPositionAndRotation(worldPosition, worldRotation);
                entrySheet.m_udonChef.SetCallbackOnCollisionPlayerToFood(OnCollisionPlayerToFood);
                DisplayCharacterBase prefab = null;
                switch (m_displayCharacterType)
                {
                    case CharacterType.SimpleCapsule:
                        prefab = m_prefabsData.m_simpleCapsuleCharacter;
                        
                        break;
                    case CharacterType.MaleCitizen:
                        prefab = m_prefabsData.m_maleCitizenCharacter;
                        break;
                }
                if (prefab == null)
                {
                    prefab = m_prefabsData.m_simpleCapsuleCharacter;
                }
                entrySheet.m_udonChef.SetRealModel(prefab);

                // AI生成 
                int entryId = GameConfigSetting.Participants[i];
                if (entryId < 0) entryId = i;
                var comPrefab = m_participantList.m_comPlayers[entryId];
                entrySheet.m_comPlayer = Instantiate(comPrefab, this.transform);
                entrySheet.m_comPlayer.Setup(i, this);
                entrySheet.m_comPlayer.gameObject.SetActive(false);

                // うどん鉢作成 
                entrySheet.m_udonBowl = CreateNewUdonBowl(entrySheet.m_udonChef);

                // 目標座標
                entrySheet.m_targetCircle = Instantiate(m_prefabsData.m_comTargetCirclePrefab, this.transform);
                entrySheet.m_targetCircle.SetNo(i, m_gameTeamColors[i], entrySheet.m_udonChef.transform);
                entrySheet.m_targetCircle.SetTargetPosition(Vector3.zero, false);   // 非表示スタート 

                // 結果
                entrySheet.m_resultBook.Reset();

                // スコアUI設定 
                m_playersScoreRootUI.Entry(i, entrySheet.m_comPlayer, m_gameTeamColors[i]);

                // 名前UI 
                Color outlineColor = m_gameTeamColors[i];
                outlineColor.r *= 0.5f;
                outlineColor.g *= 0.5f;
                outlineColor.b *= 0.5f;
                m_canvas3dRoot.CreatePlayerName3dUI(i, entrySheet.m_comPlayer.YourName,
                    entrySheet.m_udonChef.transform, Vector3.up * 1.0f, outlineColor);

                // 表示キャラクターに追加設定 
                entrySheet.m_udonChef.SetRealModelExtraData(
                    entrySheet.m_comPlayer.YourName, entrySheet.m_comPlayer.FaceImage);

                // 登録 
                m_playerEntrySheetList.Add(entrySheet);
            }

            yield return null;

            // Stage設定 
            ShippingTable[] shippingTables = m_objStage.GetComponentsInChildren<ShippingTable>();
            foreach (var table in shippingTables)
            {
                table.SetUdonChefSceneManager(this);
            }

            yield return null;

            // CharaRenderCameraを作る 
            for (int i=0; i < GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE; ++i)
            {
                var charaCamera = Instantiate(m_prefabsData.m_charaRenderCameraPrefab, this.transform);
                m_charaRenderCameraList.Add(charaCamera);
            }

            //// アウトライン設定
            //for (int i = 0; i < m_playerEntrySheetList.Count; i++)
            //{
            //    var renderers = m_playerEntrySheetList[i].m_udonChef.GetRenderers();
            //    m_outlineRenderPassFeature.SetRenderer(m_gameTeamColors[i], renderers);
            //}

            // 食材をランダムに配置 
            m_foodsCreator.DrawLotteryFoods(m_gameData.TotalCountOfFoods, m_gameData.CountOfNoodles);
            m_foodsCreator.CreateInitialPlacement(m_gameData.InitialCountOfFoods, this.transform);

            //// キー入力待ち 
            //while (!WasPressedKey())
            //{
            //    yield return null;
            //}

            // カウントダウンへ 
            ChangeSceneFlow(SceneFlow.ChallengersIntro);
        }

        /// <summary>
        /// 挑戦者紹介画面 
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoSceneChallengersIntro()
        {
            // キャラテクスチャを描画開始 
            for (int i=0; i < GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE; ++i)
            {
                var charaTexture = m_charaRenderCameraList[i];
                var playerSheet = m_playerEntrySheetList[i];
                charaTexture.StartRendering(playerSheet.m_udonChef.transform, CharaRenderCamera.CameraMode.ChallengerIntro);
            }
            yield return null;

            // mainキャンバスを非表示に 
            m_mainCanvasController.SetAlpha(0);

            // 挑戦者紹介画面を開始 
            ComPlayerBase[] comPlayers = new ComPlayerBase[m_playerEntrySheetList.Count];
            for (int i=0; i < m_playerEntrySheetList.Count; ++i)
            {
                comPlayers[i] = m_playerEntrySheetList[i].m_comPlayer;
            }
            Texture[] charaTextures = new Texture[m_charaRenderCameraList.Count];
            for (int i=0; i < m_charaRenderCameraList.Count; ++i)
            {
                charaTextures[i] = m_charaRenderCameraList[i].Texture;
            }
            m_challengersIntroScreenUI.StartScreen(
                comPlayers, m_gameTeamColors, charaTextures);

            // 紹介アニメーション 
            foreach (var playerSheet in m_playerEntrySheetList)
            {
                playerSheet.m_udonChef.SetAnimation(StickmanCharacter.AnimationState.Intro);
            }

            yield return null;

            // フェードイン 
            FadeCanvas.Instance.FadeIn();

            // キー入力待ち 
            while (!WasPressedKey())
            {
                yield return null;
            }
            // SE
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.GameStart);

            // キャラカメラを止める 
            foreach (var charaCamera in m_charaRenderCameraList)
            {
                charaCamera.StopRendering();
            }

            // 挑戦者紹介画面を閉じる 
            m_challengersIntroScreenUI.CloseScreen();
            yield return new WaitForSeconds(0.1f);

            // カウントダウンへ 
            ChangeSceneFlow(SceneFlow.CountDown);
        }

        private bool    WasPressedKey()
        {
            return GameInputManager.Instance.WasPressed(GameInputManager.Type.Decide);
        }


        /// <summary>
        /// 新しいうどん鉢を作る 
        /// </summary>
        /// <param name="udonChef"></param>
        /// <returns></returns>
        private UdonBowl CreateNewUdonBowl(UdonChef udonChef)
        {
            UdonBowl newBowl = Instantiate(m_prefabsData.m_udonBowlPrefab, udonChef.transform);
            //newBowl.transform.localPosition = new Vector3(0, 1.0f, 0.8f);
            newBowl.transform.localPosition = new Vector3(0, 1.5f, 0.2f);
            return newBowl;
        }


        /// <summary>
        /// Scene管理：初期化
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoSceneCountDown()
        {
            // 待機アニメーション 
            foreach (var playerSheet in m_playerEntrySheetList)
            {
                playerSheet.m_udonChef.SetAnimation(StickmanCharacter.AnimationState.GameIdle);
            }
            yield return null;

            // mainキャンバスを表示に 
            m_mainCanvasController.SetAlpha(1);

            // スコア表示を入場 
            m_playersScoreRootUI.Enter();

            // キー入力待ち 
            while (!WasPressedKey())
            {
                yield return null;
            }

            // カウントダウンの表示 
            bool endCountDownFlag = false;
            m_countDownUI.StartCountDown(() =>
            {
                endCountDownFlag = true;
            });

            // カウントダウン終了待ち 
            while (!endCountDownFlag)
            {
                yield return null;
            }

            // ゲーム開始 
            ChangeSceneFlow(SceneFlow.Playing);
        }


        /// <summary>
        /// Scene管理：ゲームプレイ
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoSceneGamePlaying()
        {
            // BGM再生
            Effect.SoundController.instance?.PlayBGM(Effect.SoundController.BGMType.MainBGM);

            // COMの更新有効 
            m_isUpdateComs = true;
            m_isScoreAcceptable = true;

            // COMを起こす 
            foreach (var playerSheet in m_playerEntrySheetList)
            {
                playerSheet.m_comPlayer.gameObject.SetActive(true);
            }

            // 走るアニメーション 
            foreach (var playerSheet in m_playerEntrySheetList)
            {
                playerSheet.m_udonChef.SetAnimation(StickmanCharacter.AnimationState.GameRun);
            }

            // 制限時間管理 
            m_gamePlayTime = 0;
            bool isHidedScoreUI = false;
            while (m_gamePlayTime < m_gameData.TotalPlayTime)
            {
                m_gamePlayTime += Time.deltaTime;

                // 食材補充更新 
                m_foodsCreator.UpdateToPopFoods(this.transform);

                // 残り時間 
                m_remainingTimeUI.SetTime(Mathf.Max(0, m_gameData.TotalPlayTime - m_gamePlayTime));

                // スコアUIを消す 
                if (!isHidedScoreUI && m_gameData.HideScoreUiTime <= m_gamePlayTime)
                {
                    isHidedScoreUI = true;
                    m_playersScoreRootUI.Leave();
                }

                yield return null;
            }

            // 時間切れ 
            m_remainingTimeUI.SetTime(0);

            // BGM停止
            Effect.SoundController.instance?.StopBGM();
            // TimeUP！
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.TimeUp);

            // GAME SET UI表示 
            m_gameSetUI.Enter();

            // COMの更新停止 
            m_isUpdateComs = false;

            // ほんの少しだけ余韻
            yield return new WaitForSeconds(0.5f);

            // COMの速度を止める 
            foreach (var playerSheet in m_playerEntrySheetList)
            {
                playerSheet.m_udonChef.MoveTo(Vector3.zero, 0);
            }

            // スコア受付終了 
            m_isScoreAcceptable = false;

            // キー入力待ち 
            while (!WasPressedKey())
            {
                yield return null;
            }

            yield return null;

            // リザルト開始 
            ChangeSceneFlow(SceneFlow.Result);

        }


        /// <summary>
        /// リザルト画面 
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoSceneResult()
        {
            const float FADEOUT_TIME = 0.5f;

            // フェードアウト 
            FadeCanvas.Instance.FadeOut(FADEOUT_TIME);
            yield return new WaitForSeconds(FADEOUT_TIME);


            // プレイヤー位置をリセット
            foreach (var entrySheet in m_playerEntrySheetList)
            {
                // 初期位置 
                Vector3 worldPosition;
                Quaternion worldRotation;
                entrySheet.m_coordinateSys.GetWorldPositionAndRotation(
                    m_startPlayerPosition, Quaternion.identity,
                    out worldPosition, out worldRotation);

                // 座標リセット 
                entrySheet.m_udonChef.transform.SetPositionAndRotation(worldPosition, worldRotation);

                // 目標位置表示を止める 
                entrySheet.m_targetCircle.SetTargetPosition(Vector3.zero, false);

                // アニメーション 
                entrySheet.m_udonChef.SetAnimation(StickmanCharacter.AnimationState.Intro);
            }

            // キャラテクスチャを描画開始 
            for (int i = 0; i < GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE; ++i)
            {
                var charaTexture = m_charaRenderCameraList[i];
                var playerSheet = m_playerEntrySheetList[i];
                charaTexture.StartRendering(playerSheet.m_udonChef.transform, CharaRenderCamera.CameraMode.ChallengerIntro);
            }
            yield return null;

            // mainキャンバスを非表示に 
            m_mainCanvasController.SetAlpha(0);

            // リザルト画面を開始 
            ComPlayerBase[] comPlayers = new ComPlayerBase[m_playerEntrySheetList.Count];
            ResultBook[] resultBooks = new ResultBook[m_playerEntrySheetList.Count];
            int bestScore = 0;
            int winnerId = 0;
            for (int i = 0; i < m_playerEntrySheetList.Count; ++i)
            {
                comPlayers[i] = m_playerEntrySheetList[i].m_comPlayer;
                resultBooks[i] = m_playerEntrySheetList[i].m_resultBook;

                // 順位 
                if (bestScore < resultBooks[i].TotalPrice)
                {
                    bestScore = resultBooks[i].TotalPrice;
                    winnerId = i;
                }
            }
            Texture[] charaTextures = new Texture[m_charaRenderCameraList.Count];
            for (int i = 0; i < m_charaRenderCameraList.Count; ++i)
            {
                charaTextures[i] = m_charaRenderCameraList[i].Texture;
            }
            m_resultScreenUI.StartScreen(
                comPlayers, m_gameTeamColors, charaTextures, resultBooks, GameConfigSetting.RoundCount);

            // プレイヤーキャラの順位確定アニメーション
            m_resultScreenUI.SetOnGaugeStopDelegate((int playerId) =>
            {
                var playerSheet = m_playerEntrySheetList[playerId];
                if (playerId == winnerId)
                {
                    playerSheet.m_udonChef.SetAnimation(StickmanCharacter.AnimationState.Win);
#if ON_EFFECT_XEEN
                    if (m_resultEffect == null) return;
                    var ef = Instantiate(m_resultEffect, m_charaRenderCameraList[winnerId].transform);
                    ef.transform.localPosition = new Vector3(0, -0.33f, 11.5f);
                    ef.transform.localScale = Vector3.one * 0.4f;
#endif
                }
                else
                {
                    playerSheet.m_udonChef.SetAnimation(StickmanCharacter.AnimationState.Lose);
                }
            });

            yield return null;


            // フェードイン 
            FadeCanvas.Instance.FadeIn();

            // キー入力待ち 
            while (!WasPressedKey())
            {
                yield return null;
            }
            // SE
            Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Start);

            // キャラカメラを止める 
            foreach (var charaCamera in m_charaRenderCameraList)
            {
                charaCamera.StopRendering();
            }

            // リザルト画面を閉じる 
            m_resultScreenUI.CloseScreen();

            // 終了へ 
            ChangeSceneFlow(SceneFlow.Finish);
        }


        private IEnumerator CoSceneFinish()
        {
            // フェードアウト 
            FadeCanvas.Instance.FadeOut();
            yield return new WaitForSeconds(0.5f);

            // タイトルへ 
            SceneManager.LoadSceneAsync("Title");
        }


        /// <summary>
        /// プレイヤーがfoodに接触した 
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="foodObj"></param>
        private void OnCollisionPlayerToFood(int playerId, GameObject foodObj)
        {
            if (!m_isUpdateComs) return;

            // プレイヤーの手持ちがいっぱい
            if (GameConstants.MAX_NUMBER_OF_PLAYER_CAN_HAVE <= GetFoodsListOnHand(playerId).Count) return;

            // food検索 
            var foodSheet = m_foodsCreator.FindByFoodObj(foodObj);
            if (foodSheet != null && foodSheet.m_foodState == FoodState.OnStage)
            {
                var playerSheet = m_playerEntrySheetList[playerId];
                var coordinateSys = playerSheet.m_coordinateSys;

                FoodNowInfo info = MakeFoodNowInfo(foodSheet, coordinateSys);

                // プレイヤーに拾うかどうか問い合わせ 
                if (playerSheet.m_comPlayer.UDON_ShouldGetTheFoodOnStage(info))
                {
                    // 手に持つ 
                    foodSheet.PickupFromStage(playerSheet);
                    if (foodSheet.m_foodType == FoodType.Meat)
                    {
                        Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Cow, Mathf.Clamp(foodObj.transform.position.x / 9f, -1.0f, 1.0f));
                    }
                    else
                    {
                        Effect.SoundController.instance?.PlaySE(Effect.SoundController.SEType.Food, Mathf.Clamp(foodObj.transform.position.x / 9f, -1.0f, 1.0f));
                    }
                }
            }
        }


        /// <summary>
        /// 出荷する
        /// </summary>
        /// <param name="objPlayer"></param>
        /// <param name="tableId"></param>
        /// <param name="tableCenterPosition"></param>
        /// <returns></returns>
        public bool ShipTheFoods(GameObject objPlayer, int tableId, Vector3 tableCenterPosition)
        {
            if (!m_isScoreAcceptable)
            {
                return false;
            }

            var playerSheet = m_playerEntrySheetList.Find((a) => a.m_udonChef.gameObject == objPlayer);
            if (playerSheet != null)
            {
                // 手持ちがあるかどうか 
                if (0 < playerSheet.m_foodsListOnHand.Count)
                {
                    // メニューチェック 
                    int price = 0;
                    string menuName = "";
                    var foods = playerSheet.m_foodsListOnHand.ToArray();
                    bool isGoodMenu = m_menuCheck.Check(foods, out price, out menuName, out int goodFoodsCount, out int badFoodsCount);

                    // うどん鉢を投げる 
                    UdonBowl lastUdonBowl = playerSheet.m_udonBowl;
                    Vector3 goalPos3d = ThrowTheBowl(playerSheet, 
                        tableId, tableCenterPosition, isGoodMenu);

                    // プレイヤーに出荷伝達 
                    playerSheet.m_comPlayer.UDON_ReportOnShipping(playerSheet.m_foodsListOnHand, tableId, price, menuName);

                    // 新しいうどん鉢を持つ 
                    playerSheet.m_udonBowl = CreateNewUdonBowl(playerSheet.m_udonChef);

                    // 食材の状態変更 
                    foreach (var foodId in playerSheet.m_foodsIdListOnHand)
                    {
                        var foodSheet = m_foodsCreator.FindByID(foodId);
                        if (foodSheet != null)
                        {
                            foodSheet.m_foodState = FoodState.Shipped;  // 出荷済みにする 
                        }
                    }

                    // 出荷テーブル側に記録を登録 
                    var playerSheetByTable = m_playerEntrySheetList[tableId];
                    playerSheetByTable.m_resultBook.Regist(foods, price, menuName, playerSheet.m_id,
                        goodFoodsCount, badFoodsCount);

                    // UI表示 
                    m_playersScoreRootUI.SetNewScore(tableId, price, menuName, 
                        playerSheetByTable.m_resultBook.TotalPrice);

                    // 手持ちリストをリセット 
                    playerSheet.m_foodsListOnHand.Clear();
                    playerSheet.m_foodsIdListOnHand.Clear();

                    // UI表示
                    if (isGoodMenu)
                    {
                        m_canvas3dRoot.CreateMenuIsUp3dUI(menuName, price, goalPos3d);
                    } else
                    {
                        m_canvas3dRoot.CreateMenuIsUp3dUI(menuName, price, lastUdonBowl.transform, Vector2.up*10);
                    }

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// うどん鉢を投げる 
        /// </summary>
        /// <param name="playerSheet"></param>
        /// <param name="tableId"></param>
        /// <param name="tableCenterPosition"></param>
        /// <param name="isGoodMenu"></param>
        private Vector3 ThrowTheBowl(PlayerEntrySheet playerSheet, 
            int tableId, Vector3 tableCenterPosition, bool isGoodMenu)
        {
            // 投げ込む座標を決定 
            var playerSheetByTable = m_playerEntrySheetList[tableId];
            Vector3 tableCenterLocalPosition = 
                playerSheetByTable.m_coordinateSys.GetLocalPositionFromWorld(tableCenterPosition);
            Vector3 bowlLocalPosition =
                playerSheetByTable.m_coordinateSys.GetLocalPositionFromWorld(playerSheet.m_udonBowl.transform.position);
            Vector3 goalLocalPosition = tableCenterLocalPosition;
            goalLocalPosition.x = Mathf.Lerp(bowlLocalPosition.x, tableCenterLocalPosition.x, 0.25f);
            Vector3 goalWorldPosition =
                playerSheetByTable.m_coordinateSys.GetWorldPosition(goalLocalPosition);

            // 投げる方向 
            Vector3 throwDir = goalWorldPosition - playerSheet.m_udonBowl.transform.position;
            throwDir.y = 0;
            throwDir = throwDir.normalized;

            // うどん鉢を投げ飛ばす
            playerSheet.m_udonBowl.ThrowToTable(throwDir, isGoodMenu);

            return goalWorldPosition;

        }




#region ComPlayerBaseから呼ばれる 


        /// <summary>
        /// 現在のステージ上の食材の情報を取得 
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public List<FoodNowInfo> GetFoodsInfoOnStage(int playerId, FoodState[] targetFoodsState)
        {
            List<FoodNowInfo> foodList = new();

            // 対象のstateをbit化する 
            int targetStateBit = 0;
            if (targetFoodsState == null)
            {
                targetStateBit = (1 << (int)FoodState.OnStage);
            } else
            {
                foreach (var state in targetFoodsState)
                {
                    targetStateBit |= (1 << (int)state);
                }
            }

            var coordinateSys = m_playerEntrySheetList[playerId].m_coordinateSys;

            foreach (var foodSheet in m_foodsCreator.GetList())
            {
                // 対象のstateのものだけ抽出 
                int thisFoodStateBit = 1 << (int)foodSheet.m_foodState;
                if ((thisFoodStateBit& targetStateBit) != 0)
                {
                    FoodNowInfo info = MakeFoodNowInfo(foodSheet, coordinateSys);
                    foodList.Add(info);
                }
            }

            return foodList;
        }

        /// <summary>
        /// FoodEntrySheetからFoodNowInfoを作成 
        /// </summary>
        /// <param name="foodSheet"></param>
        /// <param name="coordinateSys"></param>
        /// <returns></returns>
        private FoodNowInfo MakeFoodNowInfo(FoodsCreator.FoodEntrySheet foodSheet, 
            CoordinateSystemController coordinateSys)
        {
            FoodNowInfo info = new();
            info.m_foodId = foodSheet.m_serialNo;
            info.m_type = foodSheet.m_foodType;
            info.m_state = foodSheet.m_foodState;
            info.m_position = coordinateSys.GetLocalPositionFromWorld(foodSheet.m_oneFood.transform.position);
            return info;
        }

        /// <summary>
        /// foodIdを指定して食材の現在の状態を取得 
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="foodId"></param>
        /// <returns></returns>
        public FoodNowInfo GetFoodInfo(int playerId, int foodId)
        {
            var foodSheet = m_foodsCreator.FindByID(foodId);
            if (foodSheet != null)
            {
                var coordinateSys = m_playerEntrySheetList[playerId].m_coordinateSys;
                FoodNowInfo info = MakeFoodNowInfo(foodSheet, coordinateSys);
                return info;
            } else
            {
                FoodNowInfo info = new();
                info.m_foodId = 0;
                info.m_state = FoodState.None;
                return info;
            }
        }


        /// <summary>
        /// 手に持っている食材リストを返す 
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public IList<FoodType> GetFoodsListOnHand(int playerId)
        {
            var playerSheet = m_playerEntrySheetList[playerId];
            return new List<FoodType>(playerSheet.m_foodsListOnHand);
        }


        /// <summary>
        /// プレイヤー座標と角度を返す(Local)
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="localPosition"></param>
        /// <param name="localRotation"></param>
        public void GetLocalPlayerPositionAndRotation(int playerId, 
            out Vector3 localPosition, out Quaternion localRotation)
        {
            var playerSheet = m_playerEntrySheetList[playerId];
            playerSheet.m_coordinateSys.GetLocalPositionAndRotation(
                playerSheet.m_udonChef.transform.position, playerSheet.m_udonChef.transform.rotation,
                out localPosition, out localRotation);
        }


        /// <summary>
        /// プレイヤーの移動速度を返す(Local)
        /// </summary>
        /// <param name="playerId"></param>
        public Vector3 GetPlayerLocalVelocity(int playerId)
        {
            var playerSheet = m_playerEntrySheetList[playerId];
            Vector3 worldVelocity = playerSheet.m_udonChef.GetVelocity();
            return playerSheet.m_coordinateSys.GetLocalVectorFromWorld(worldVelocity);
        }


        /// <summary>
        /// キャラクターは接地しているか？ 
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public bool IsPlayerOnGround(int playerId)
        {
            var playerSheet = m_playerEntrySheetList[playerId];
            return playerSheet.m_udonChef.IsOnGround();
        }

        /// <summary>
        /// 食材を指定して価格を取得する
        /// </summary>
        /// <param name="foods"></param>
        /// <returns></returns>
        public int GetPriceOfFoods(FoodType[] foods)
        {
            int price = 0;
            string menuName = "";
            bool isGoodMenu = m_menuCheck.Check(foods, out price, out menuName, out int goodFoodsCount, out int badFoodsCount);
            return price;
        }


        /// <summary>
        /// Raycastによるコリジョン判定
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="maxDistance"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public bool PhysicsRaycastFromPlayer(int playerId, Vector3 origin, Vector3 direction, float maxDistance, int layerMask)
        {
            var playerSheet = m_playerEntrySheetList[playerId];

            // originとdirectionをWorldに変換
            Vector3 worldOrigin = playerSheet.m_udonChef.transform.TransformPoint(origin);
            Vector3 worldDirection = playerSheet.m_udonChef.transform.TransformDirection(direction);

            // Raycast
            bool result = Physics.Raycast(worldOrigin, worldDirection, maxDistance, layerMask);
            return result;
        }


        /// <summary>
        /// 他のプレイヤーの情報を取得する 
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="targetPlayerId"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="score"></param>
        public void GetPlayerInfo(int playerId, int targetPlayerId, 
            out Vector3 position, out Vector3 direction, out int score)
        {
            var playerSheet = m_playerEntrySheetList[playerId];
            var baseCoorinateSys = playerSheet.m_coordinateSys;
            var targetPlayerSheet = m_playerEntrySheetList[targetPlayerId];
            var targetPlayerChef = targetPlayerSheet.m_udonChef;

            // 座標 
            Vector3 playerPos = Vector3.zero;
            Quaternion playerRot = Quaternion.identity;
            baseCoorinateSys.GetLocalPositionAndRotation(
                targetPlayerChef.transform.position, targetPlayerChef.transform.rotation,
                out playerPos, out playerRot);

            // 結果 
            position = playerPos;
            direction = playerRot * Vector3.forward;
            score = targetPlayerSheet.m_resultBook.TotalPrice;

        }



        /// <summary>
        /// 接触しているプレイヤー番号のリストを取得 
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public List<int>    GetCollidedPlayersNumber(int playerId)
        {
            var playerSheet = m_playerEntrySheetList[playerId];
            List<int> collidedIdList = playerSheet.m_udonChef.GetCollidedPlayerIdList();
            List<int> resultNumberList = new();
            if (0 < collidedIdList.Count)
            {
                foreach (var id in collidedIdList)
                {
                    if (id != playerId)
                    {
                        resultNumberList.Add(
                            (id + GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE - playerId) % GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE);
                    }
                }
            }
            return resultNumberList;
        }



        public FoodType GetNextFallingFood()
        {
            return m_foodsCreator.GetNextFallingFood();
        }

        public FoodType [] GetAllFutureFoodsDrops()
        {
            return m_foodsCreator.GetAllFutureFoodsDrops();
        }


        /// <summary>
        /// プレイヤーの硬直状態を返す 
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public bool IsPlayerStiffness(int playerId)
        {
            var playerSheet = m_playerEntrySheetList[playerId];

            return playerSheet.m_udonChef.IsStiffness();
        }

        /// <summary>
        /// 残りの硬直時間を取得 
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public float GetLeftTimeOfStiffness(int playerId)
        {
            var playerSheet = m_playerEntrySheetList[playerId];

            return playerSheet.m_udonChef.StiffnessTime;
        }

        /// <summary>
        /// 地面の種類を取得 
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public GameConstants.PlayerGroundType GetPlayerGroundType(int playerId)
        {
            var playerSheet = m_playerEntrySheetList[playerId];

            return playerSheet.m_udonChef.GroundTypeUnderFeet;
        }


        /// <summary>
        /// 試合の残り時間を取得 
        /// </summary>
        /// <returns></returns>
        public float GetRemainingGameTime()
        {
            return Mathf.Max(0, m_gameData.TotalPlayTime - m_gamePlayTime);
        }



#endregion

    }


}


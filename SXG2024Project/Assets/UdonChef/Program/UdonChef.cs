using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SXG2024
{

    public delegate void OnCollisionDelegate(int playerId, GameObject targetObj);


    public class UdonChef : MonoBehaviour
    {
        const float STIFFNESS_TIME_KICK = 2.0f;     // キックの硬直時間 

        private int m_playerNo = 0;     // プレイヤー番号 

        private Rigidbody m_rigidbody = null;
        private float m_moveSpeedRate = 0;
        private float m_stiffnessTime = 0;  // 硬直時間 

        private OnCollisionDelegate m_onCollisionPlayerToFood;  // プレイヤーがFoodに接触した時 

        private List<GameObject> m_touchedGroundObjList = new();    // 接触している地面のリスト 
        private List<List<GameObject>> m_touchedPlayerObjList = new();  // 接触しているプレイヤーのリスト 

        private Color m_teamColor = Color.white;
        private DisplayCharacterBase m_realChara = null;

        [SerializeField] private MeshRenderer m_capsuleRenderer = null;
        [SerializeField] private PlayerKick m_playerKick = null;

        [SerializeField] private CapsuleCollider m_footCollider = null;    // 足元のコライダー 

        private PhysicMaterial m_physicMaterial = null;
        private float m_physicBaseDynamicFriction = 0;
        private float m_physicBaseStaticFriction = 0;
        private float m_physicBaseBounciness = 0;

        [SerializeField] private AnimationCurve m_physicParamCurve = new();
        [SerializeField] private float m_frictionWhenBraking = 1;
        [SerializeField] private float m_bouncinessWhenBraking = 0;

        private GameConstants.PlayerGroundType m_playerGroundType = GameConstants.PlayerGroundType.None;


        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();

            // 物理パラメータの初期化 
            m_physicMaterial = m_footCollider.material;
            m_physicBaseDynamicFriction = m_physicMaterial.dynamicFriction;
            m_physicBaseStaticFriction = m_physicMaterial.staticFriction;
            m_physicBaseBounciness = m_physicMaterial.bounciness;
        }

        // Start is called before the first frame update
        void Start()
        {
            for (int i=0; i < GameConstants.MAX_PLAYER_COUNT_IN_ONE_BATTLE; ++i)
            {
                m_touchedPlayerObjList.Add(new List<GameObject>());
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (0 < m_stiffnessTime)
            {
                m_stiffnessTime -= Time.deltaTime;
            }
        }

        public bool IsStiffness()
        {
            return 0 < m_stiffnessTime;
        }

        public float StiffnessTime => m_stiffnessTime;



        [SerializeField] private float m_maxMoveAcc = 100;

        private void FixedUpdate()
        {
            if (0 < m_moveSpeedRate)
            {
                // 設置してる場合のみ意図した方向へ加速可能 
                if (IsOnGround() && m_stiffnessTime <= 0)
                {
                    m_rigidbody.AddForce(transform.forward * (m_maxMoveAcc * m_moveSpeedRate));
                }
            }
        }


        /// <summary>
        /// プレイヤー番号に応じてマテリアルカラーを設定 
        /// </summary>
        private void SetMaterialColor(Color teamColor)
        {
            m_teamColor = teamColor;
            Renderer rend = GetComponentInChildren<Renderer>();
            rend.material = new Material(rend.material);
            rend.material.color = teamColor;
        }


        public void SetPlayer(int playerNo, Color teamColor)
        {
            m_playerNo = playerNo;
            SetMaterialColor(teamColor);
        }


        /// <summary>
        /// 目標座標に向かって移動 
        /// </summary>
        /// <param name="targetWorldPosition"></param>
        /// <param name="speedRate"></param>
        public void MoveTo(Vector3 targetWorldPosition, float speedRate)
        {
            const float MAX_ROT_SPEED = 180.0f;  // 最大旋回速度 

            if (0 < speedRate)
            {
                // 目標の方向を向く 
                Vector3 dir = targetWorldPosition - transform.position;
                dir.y = 0;
                if (0.01f < dir.sqrMagnitude)
                {
                    Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                    Quaternion newRotation = Quaternion.RotateTowards(transform.rotation,
                        lookRot, MAX_ROT_SPEED * Time.deltaTime);
                    transform.rotation = newRotation;
                }
            }

            // アニメーション 
            if (m_realChara!=null)
            {
                m_realChara.SetAnimationRunSpeed(speedRate);
            }

            // 移動速度 
            m_moveSpeedRate = speedRate;

            // マテリアル設定 
            UpdatePhysicsMaterialParameter(speedRate);

        }

        /// <summary>
        /// 物理マテリアルの摩擦力などを調整 
        /// </summary>
        /// <param name="rate"></param>
        private void UpdatePhysicsMaterialParameter(float rate)
        {
            float k = Mathf.Clamp01(m_physicParamCurve.Evaluate(rate));
            m_physicMaterial.dynamicFriction =
                Mathf.Lerp(m_physicBaseDynamicFriction, m_frictionWhenBraking, k);
            m_physicMaterial.staticFriction =
                Mathf.Lerp(m_physicBaseStaticFriction, m_frictionWhenBraking, k);
            m_physicMaterial.bounciness =
                Mathf.Lerp(m_physicBaseBounciness, m_bouncinessWhenBraking, k);
        }


        /// <summary>
        /// キック行動 
        /// </summary>
        public void Kick()
        {
            // 攻撃判定 
            m_playerKick.Kick();

            // アニメーション 
            if (m_realChara!=null)
            {
                m_realChara.SetAnimation(StickmanCharacter.AnimationState.Kick);
            }

            // 硬直時間 
            m_stiffnessTime = STIFFNESS_TIME_KICK;

            // 移動速度 
            m_moveSpeedRate = 0;
        }


        /// <summary>
        /// Player to Food コリジョンコールバックの設定
        /// </summary>
        /// <param name="callback"></param>
        public void SetCallbackOnCollisionPlayerToFood(OnCollisionDelegate callback)
        {
            m_onCollisionPlayerToFood += callback;
        }


        /// <summary>
        /// Ground(Stage)の上に接地しているか？ 
        /// </summary>
        /// <returns></returns>
        public bool IsOnGround()
        {
            return 0 < m_touchedGroundObjList.Count;
        }

        /// <summary>
        /// 足元の地面の種類を取得 
        /// </summary>
        public GameConstants.PlayerGroundType GroundTypeUnderFeet => m_playerGroundType;


        /// <summary>
        /// リアルモデルを設定
        /// </summary>
        /// <param name="realCharacter">プレハブ</param>
        public void SetRealModel(DisplayCharacterBase realCharacter)
        {
            if (realCharacter != null)
            {
                // Stickman生成 
                m_realChara = Instantiate(realCharacter, this.transform);
                m_realChara.Setup(m_playerNo, m_teamColor);

                // カプセルは消す 
                m_capsuleRenderer.enabled = false;
            }
        }

        public Renderer[] GetRenderers()
        {
            if (m_realChara == null)
                return new Renderer[] { m_capsuleRenderer };

            return m_realChara.GetComponentsInChildren<Renderer>();
        }


        /// <summary>
        /// 拡張データを設定 
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="faceImage"></param>
        public void SetRealModelExtraData(string playerName, Sprite faceImage)
        {
            if (m_realChara != null)
            {
                m_realChara.SetupExtraData(playerName, faceImage);
            }
        }

        public void SetAnimation(StickmanCharacter.AnimationState animState)
        {
            if (m_realChara != null)
            {
                m_realChara.SetAnimation(animState);
            }
        }


        /// <summary>
        /// Rigidbodyの速度を返す 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetVelocity()
        {
            return m_rigidbody.velocity;
        }


        /// <summary>
        /// 現在接触しているプレイヤーIDのリストを返す 
        /// </summary>
        /// <returns></returns>
        public List<int> GetCollidedPlayerIdList()
        {
            List<int> resultList = new();
            for (int i=0; i < m_touchedPlayerObjList.Count; ++i)
            {
                if (0 < m_touchedPlayerObjList[i].Count)
                {
                    resultList.Add(i);
                }
            }
            return resultList;
        }




        private void OnCollisionEnter(Collision collision)
        {
            // 食材 
            if (collision.gameObject.layer == SystemConstants.OBJ_LAYER_FOOD)
            {
                m_onCollisionPlayerToFood.Invoke(m_playerNo, collision.gameObject);
            }
            // 地面 
            else if (collision.gameObject.layer == SystemConstants.OBJ_LAYER_GROUND)
            {
                if (!m_touchedGroundObjList.Contains(collision.gameObject))
                {
                    m_touchedGroundObjList.Add(collision.gameObject);
                }
                // 地面の種類を更新 
                UpdateGroundTypeUnderFeet();
            }
            // プレイヤー 
            else if (collision.gameObject.layer == SystemConstants.OBJ_LAYER_PLAYER)
            {
                if (collision.rigidbody != null)
                {
                    UdonChef otherChef = collision.rigidbody.GetComponent<UdonChef>();
                    if (otherChef != null)
                    {
                        var otherList = m_touchedPlayerObjList[otherChef.m_playerNo];
                        if (!otherList.Contains(collision.gameObject))
                        {
                            otherList.Add(collision.gameObject);
                        }
                    }
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            // 地面 
            if (collision.gameObject.layer == SystemConstants.OBJ_LAYER_GROUND)
            {
                if (m_touchedGroundObjList.Contains(collision.gameObject))
                {
                    m_touchedGroundObjList.Remove(collision.gameObject);
                }
                // 地面の種類を更新 
                UpdateGroundTypeUnderFeet();
            }
            // プレイヤー 
            else if (collision.gameObject.layer == SystemConstants.OBJ_LAYER_PLAYER)
            {
                if (collision.rigidbody != null)
                {
                    UdonChef otherChef = collision.rigidbody.GetComponent<UdonChef>();
                    if (otherChef != null)
                    {
                        var otherList = m_touchedPlayerObjList[otherChef.m_playerNo];
                        if (otherList.Contains(collision.gameObject))
                        {
                            otherList.Remove(collision.gameObject);
                        }
                    }
                }
            }
        }


        const string TAG_GAME_FIELD = "GameField";
        const string TAG_TABLE = "Table";
        const string TAG_OUTSIDE = "Outside";

        /// <summary>
        /// プレイヤーの足元の種類を取得 
        /// </summary>
        private void UpdateGroundTypeUnderFeet()
        {
            if (0 < m_touchedGroundObjList.Count)
            {
                foreach (var obj in m_touchedGroundObjList)
                {
                    if (obj.CompareTag(TAG_GAME_FIELD))
                    {
                        m_playerGroundType = GameConstants.PlayerGroundType.GameField;
                        break;      // GameField優先
                    } else if (obj.CompareTag(TAG_TABLE))
                    {
                        m_playerGroundType = GameConstants.PlayerGroundType.Table;
                    } else if (obj.CompareTag(TAG_OUTSIDE))
                    {
                        m_playerGroundType = GameConstants.PlayerGroundType.Outside;
                        break;      // Outside優先
                    }
                }
            } else
            {
                m_playerGroundType = GameConstants.PlayerGroundType.None;
            }
        }

    }


}

using System.Collections.Generic;
using UnityEngine;
using SXG2024;

namespace nsSample
{
    public class ComPlayerSample04 : ComPlayerBase
    {
        const float TABLE_POSITION_Z = -12.0f;
        const float TABLE_WIDTH = 5.0f;

        const float SAFE_RANGE = 3f;
        const float OUT_STAGE_RANGE = 9.0f;

        const int BUY_PRICE = 300;

        private Vector3 GoalPos
        {
            get
            {
                // ゴール座標を決定 
                Vector3 goalPos = m_pos;
                goalPos.x = Mathf.Clamp(m_pos.x, -TABLE_WIDTH * 0.8f, +TABLE_WIDTH * 0.8f);
                goalPos.z = TABLE_POSITION_Z;
                return goalPos;
            }
        }

        private Vector3 m_pos = Vector3.zero;
        private Quaternion m_rot = Quaternion.identity;

        [SerializeField]
        private float m_kickAngle = 0.1f;

        [SerializeField]
        private float m_kickRange = 2.0f;

        [SerializeField]
        private float m_eneRange = 0.1f;
        private float m_confTime = 0.0f;

        [System.Serializable]
        private struct AdvantageData {
            public FoodType _type;
            public float _factor;
        }

        [SerializeField]
        private List<AdvantageData> _advantageDatas = new List<AdvantageData>();

        private State m_state = State.Start;

        private Vector3 m_targetPos = Vector3.zero;
        private float m_speed = 1.0f;
        private void SetMovePosSpeed(Vector3 pos, float spe)
        {
            m_targetPos = pos;
            m_speed = spe;
        }

        // 硬直
        private bool _isStiffness = false;

        private enum State { 
            Start,
            Run,
            Buy,
            Confusion,
            Wait,
        }

        private FoodNowInfo _target;

        private void Start()
        {
            SXG_GetPositionAndRotation(out m_pos, out m_rot);
        }

        private void Update()
        {
            // 座標更新
            SXG_GetPositionAndRotation(out m_pos, out m_rot);
            if (SXG_IsOnGround() == false)
            {
                // 蹴とばされてる？？
                // ホームか中央に戻る
                if (m_state == State.Buy)
                {
                    // ホームに戻る
                    SXG_MoveToTargetPosition(GoalPos, 1.0f);
                }
                else
                {
                    // 中央に戻る
                    SXG_MoveToTargetPosition(Vector3.zero, 1.0f);
                }
                return;
            }
            if(SXG_GetPlayerGroundType() == GameConstants.PlayerGroundType.Outside)
            {
                // ステージ外にいるので落ちよう
                SXG_MoveToTargetPosition(m_pos - m_pos.normalized, 1.0f);
                return;
            }

            if (m_state != State.Confusion && SXG_GetNowPriceOnHand() >= BUY_PRICE)
            {
                // 売りに行く
                m_state = State.Buy;
            }

            // ステートごとの行動
            switch (m_state)
            {
                case State.Start:
                    Search();
                    break;
                case State.Run:
                    Run();
                    break;
                case State.Buy:
                    Buy();
                    break;
                case State.Confusion:
                    Kick();
                    break;
                case State.Wait:
                    Wait();
                    break;
            }

            SXG_MoveToTargetPosition(m_targetPos, m_speed);
        }

        private void Wait()
        {
            if (SXG_GetFoodsInfoOnStage().Count == 0)
            {
                var pos = new Vector3(Mathf.Sin(Time.time), 0, Mathf.Cos(Time.time)) * SAFE_RANGE;
                SetMovePosSpeed(pos, 1f);
            }
            else
            {
                SetState(State.Start);
            }
        }

        private void Buy()
        {
            if (SXG_IsStiffness()) return;
            // 手に持っている食材が無くなったら出荷したという事 
            if (SXG_GetMyFoodsListOnHand().Count <= 0)
            {
                SetState(State.Start);
                return;
            }

            // ゴール座標へ向かって走る 
            if (!Kick())
            {
                SetMovePosSpeed(GoalPos, 1.0f);
            }
        }

        private void Run()
        {
            if (SXG_IsStiffness())
            {
                _isStiffness = true;
                return;
            }
            if (_isStiffness)
            {
                // 硬直明け
                _isStiffness = false;
                // もっかい探そう
                SetState(State.Start);
                return;
            }

            _target = SXG_GetFoodInfo(_target.m_foodId);
            if ((SXG_HowManyMoreCanIHave() == 0 || _target.m_state != FoodState.OnStage || Mathf.Abs(_target.m_position.magnitude) > OUT_STAGE_RANGE))
            {
                SetState(State.Start);
                return;
            }
            var len = Vector3.Distance(_target.m_position, m_pos) / SAFE_RANGE;
            if (!Kick())
            {
                // 目的に移動する
                SetMovePosSpeed(_target.m_position, Mathf.Clamp(len, 0.9f, 1.0f));
            }

            if (_target.m_type != FoodType.Noodle)
            {
                var have = SXG_GetMyFoodsListOnHand();
                foreach(FoodType foodType in have)
                {
                    if(foodType == FoodType.Noodle)
                    {
                        return;
                    }
                }
                // 麺持ってない！
                // ステージ上に麺ある？
                var foodState = new FoodState[] { FoodState.OnStage };
                var foods = SXG_GetFoodsInfoOnStage(foodState);
                foreach (FoodNowInfo food in foods)
                {
                    if(food.m_type == FoodType.Noodle)
                    {
                        SetState(State.Start);
                        return;
                    }
                }
            }
        }

        private void Search()
        {
            var foodState = new FoodState[] { FoodState.OnStage };
            var foods = SXG_GetFoodsInfoOnStage(foodState);
            var haveFoods = SXG_GetMyFoodsListOnHand();
            var money = SXG_GetNowPriceOnHand();

            if (foods.Count == 0)
            {
                if (money > 0)
                {
                    SetState(State.Buy);
                }
                else
                {
                    SetState(State.Wait);
                }
                return;
            }
            else
            {
                var Pos = m_pos;
                Pos.y = 0;
                var targets = new List<(int id, float length)>();

                for (int i = 0; i < foods.Count; ++i)
                {
                    var foodPos = foods[i].m_position;
                    foodPos.y = 0;
                    if(foodPos.magnitude > OUT_STAGE_RANGE)
                    {
                        // 場外の物はカウントしない
                        continue;
                    }
                    var length = Vector3.Distance(foodPos, Pos);

                    // 優先的に取得したいものの計算
                    var data = _advantageDatas.FindAll(_ => _._type == foods[i].m_type);
                    if (data != null && data.Count > 0)
                    {
                        length *= data[0]._factor;
                    }
                    // 自分からの距離をリスト化
                    targets.Add(new (i, length));
                }
                if(targets.Count > 0)
                {
                    targets.Sort((a, b) => a.length.CompareTo(b.length));
                    _target = foods[targets[0].id];
                }
                else
                {
                    if (money > 0)
                    {
                        SetState(State.Buy);
                    }
                    else
                    {
                        SetState(State.Wait);
                    }
                    return;
                }
            }

            // 食材追いかける
            SetState(State.Run);
        }

        private bool Kick()
        {
            // 他プレイヤーの位置を取得
            var enemys = SXG_GetPlayersInfo();
            for (int i = 1; i < enemys.Length; ++i)
            {
                var enePos = enemys[i].Position;
                var dis = Vector3.Distance(enePos, m_pos);
                if (dis < m_eneRange)
                {
                    // 待つ
                    m_confTime += Time.deltaTime;
                    if (m_confTime > 1.0f)
                    {
                        // 目の前のやつを蹴る
                        SXG_Kick();
                        m_confTime = 0.0f;
                        SetState(State.Run);
                    }
                    else
                    {
                        if (m_state != State.Confusion)
                            m_targetPos = enePos + Vector3.forward;
                        SetState(State.Confusion);
                    }
                    return false;
                }
            }

            // conf状態を抜けた
            if (m_state == State.Confusion)
            {
                SetState(State.Run);
                m_confTime = 0.0f;
            }

            var goalVec = (GoalPos - m_pos).normalized;
            Vector3 forward = m_rot * Vector3.forward;
            if (Vector3.Dot(goalVec, forward) > 0.5f 
                && SXG_RaycastFromPlayer(Vector3.up, Vector3.forward, m_kickRange, RaycastTarget.Player))
            {
                // 目の前のやつを蹴る
                SXG_Kick();
                return true;
            }
            return false;
        }

        private void SetState(State state)
        {
            if (SXG_HowManyMoreCanIHave() == Random.Range(0, 2) || SXG_GetNowPriceOnHand() >= BUY_PRICE)
            {
                // 売りに行く
                m_state = State.Buy;
            }
            else
            {
                m_state = state;
            }
        }

        /// <summary>
        /// 食材Get判定
        /// </summary>
        /// <param name="foodInfo"></param>
        /// <returns></returns>
        public override bool UDON_ShouldGetTheFoodOnStage(FoodNowInfo foodInfo)
        {
            // それを取る事でスコアアップするなら拾う 
            int nowPrice = 0;
            int futurePrice = 0;
            IList<FoodType> nowFoodsList = SXG_GetMyFoodsListOnHand();
            if (nowFoodsList.Count > 0)
            {
                FoodType[] nowFoods = new FoodType[nowFoodsList.Count];
                nowFoodsList.CopyTo(nowFoods, 0);
                nowPrice = SXG_GetPriceOfFoods(nowFoods);
            }
            FoodType[] futureFoods = new FoodType[nowFoodsList.Count + 1];
            futureFoods[0] = foodInfo.m_type;
            nowFoodsList.CopyTo(futureFoods, 1);
            futurePrice = SXG_GetPriceOfFoods(futureFoods);
            if (nowPrice > futurePrice)
            {
                return false;
            }

            return true;
        }
    }
}


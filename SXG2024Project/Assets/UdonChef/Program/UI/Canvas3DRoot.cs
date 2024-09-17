using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SXG2024
{

    public class Canvas3DRoot : MonoBehaviour
    {
        [SerializeField] private PlayerName3DUI m_playerName3DPrefab = null;
        [SerializeField] private MenuIsUp3DUI m_menuIsUp3DPrefab = null;


        public delegate Vector2 Calc2dPositionDelegate(Vector3 pos3d);


        private RectTransform m_rectTr = null;
        private Camera m_mainCamera = null;
        private List<PlayerName3DUI> m_playerName3dList = new();

        private void Awake()
        {
            m_rectTr = GetComponent<RectTransform>();
        }

        private void Start()
        {
            m_mainCamera = Camera.main;
        }


        /// <summary>
        /// 名前UIを生成 
        /// </summary>
        /// <param name="playerNo"></param>
        /// <param name="playerName"></param>
        /// <param name="baseColor"></param>
        public void CreatePlayerName3dUI(int playerNo, string playerName, 
            Transform targetTr, Vector3 offset, Color baseColor)
        {
            var name3d = Instantiate(m_playerName3DPrefab, this.transform);
            name3d.Setup(playerName, targetTr, offset, baseColor, Calc2DPosition);

            m_playerName3dList.Add(name3d);
        }


        /// <summary>
        /// メニュー納品完了UIを生成 
        /// </summary>
        /// <param name="menuName"></param>
        /// <param name="price"></param>
        /// <param name="posdd"></param>
        public void CreateMenuIsUp3dUI(string menuName, int price, Vector3 pos3d)
        {
            var menuIsUp = Instantiate(m_menuIsUp3DPrefab, this.transform);
            Vector2 pos2d = Calc2DPosition(pos3d);
            menuIsUp.Setup(menuName, price, pos2d);
        }


        public delegate Vector2 CalcScreenPositionDelegate();

        public void CreateMenuIsUp3dUI(string menuName, int price, Transform ownerTr, Vector2 offset)
        {
            var menuIsUp = Instantiate(m_menuIsUp3DPrefab, this.transform);
            menuIsUp.Setup(menuName, price, ()=>
                {
                    if (ownerTr != null)
                    {
                        return Calc2DPosition(ownerTr.position) + offset;
                    } else
                    {
                        return Vector2.zero;
                    }
                });
        }


        /// <summary>
        /// 3D→2D座標変換 
        /// </summary>
        /// <param name="pos3d"></param>
        /// <returns></returns>
        public Vector2 Calc2DPosition(Vector3 pos3d)
        {
            if (m_mainCamera != null)
            {
                Vector2 viewp = m_mainCamera.WorldToViewportPoint(pos3d);
                return new Vector2(viewp.x * m_rectTr.rect.width, viewp.y * m_rectTr.rect.height);
            }
            return Vector2.zero;
        }
    }


}


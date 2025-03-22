using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Solution.Task2
{
    public class WaitingCellQueue : MonoBehaviour
    {
        public WaitingCell[] Queue { get; private set; }

        private GameSettings m_settings;

        private Transform m_root;

        public GameManager GameManager { get; private set; }

        private bool m_isDragging;

        private Camera m_cam;
        
        private Collider2D m_hitCollider;

        public bool IsBusy { get; set; }

        public int TotalItemsOnQueue { get; set; }

        private const int MAX_QUEUE_SIZE = 5;

        private void Awake()
        {
            m_cam = Camera.main;
        }

        public void SetUp(GameSettings gameSettings, GameManager gameManager)
        {
            Queue = new WaitingCell[MAX_QUEUE_SIZE];

            m_settings = gameSettings;
            
            GameManager = gameManager;
            
            CreateQueue();
        }

        private void CreateQueue()
        {
            Vector3 origin = new Vector3(-2f, -m_settings.BoardSizeY * 0.5f - 1f, 0f);
            
            if(m_root == null)
                m_root = new GameObject("QueueRoot").transform;
                
            m_root.position = origin;
            
            GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_WAITING_CELL);

            for (int i = 0; i < MAX_QUEUE_SIZE; i++)
            {
                GameObject go = Instantiate(prefabBG);
                go.transform.position = origin +new Vector3(i, 0, 0);
                go.transform.SetParent(go.transform);
                
                WaitingCell waitingCell = go.GetComponent<WaitingCell>();
                waitingCell.Setup(i, this);
                
                Queue[i] = waitingCell;
            }
        }

        public IEnumerator OnItemSelected(Item item, int x, int y)
        {
            if(TotalItemsOnQueue < MAX_QUEUE_SIZE)
            {
                yield return MoveToBottom(item, x, y);

                yield return new WaitForSeconds(0.2f);

                yield return CheckCollapse();

                if (GameManager.LevelMode != GameManager.eLevelMode.TIMER && TotalItemsOnQueue == MAX_QUEUE_SIZE)
                {
                    GameManager.SetState(GameManager.eStateGame.GAME_OVER);
                }

                GameManager.BoardController.CheckWin();
            }
        }

        private IEnumerator MoveToBottom(Item item, int x, int y)
        {
            item.SetViewRoot(m_root);
            item.LastPosition = (x, y);

            int insertedIdx = FindSimilarTypeFirstIndex(item);

            for (int i = TotalItemsOnQueue; i > insertedIdx; i--)
            {
                Queue[i].Assign(Queue[i - 1].Item);
                Queue[i].ApplyItemMoveToPosition();
                Queue[i - 1].Free();
                yield return new WaitForSeconds(0.1f);
            }

            Queue[insertedIdx].Assign(item);
            Queue[insertedIdx].ApplyItemMoveToPosition();
            TotalItemsOnQueue++;

            yield return new WaitForSeconds(0.1f);
        }

        private int FindSimilarTypeFirstIndex(Item item)
        {
            NormalItem normalItem = item as NormalItem;
            for (int i = 0; i < TotalItemsOnQueue; i++)
            {
                NormalItem currItem = Queue[i].Item as NormalItem;
                if (currItem.IsSameType(normalItem))
                    return i;
            }

            return TotalItemsOnQueue;
        }

        private IEnumerator CheckCollapse()
        {
            for (int i = 0; i <= TotalItemsOnQueue - 3; i++)
            {
                if (Queue[i].IsSameType(Queue[i + 1]) && Queue[i].IsSameType(Queue[i + 2]))
                {
                    TotalItemsOnQueue -= 3;
                    
                    Queue[i].ExplodeItem();
                    Queue[i+1].ExplodeItem();
                    Queue[i+2].ExplodeItem();

                    if (i <= MAX_QUEUE_SIZE - 3 - 1)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (Queue[i + 3].Item != null)
                            {
                                Queue[i].Assign(Queue[i + 3].Item);
                                Queue[i].ApplyItemMoveToPosition();
                                Queue[i + 3].Free();
                            
                                yield return new WaitForSeconds(0.1f);
                            }
                            else break;
                        }
                    }

                    
                }
            }
        }

        public int CountType(Item item)
        {
            int cnt = 0;
            
            for (int i = 0; i < TotalItemsOnQueue; i++)
            {
                if (Queue[i].Item.IsSameType(item))
                    cnt++;
            }

            return cnt;
        }

        public void Clear()
        {
            Destroy(m_root.gameObject);
            for (int i = 0; i < MAX_QUEUE_SIZE; i++)
            {
                Queue[i].Clear();
                Destroy(Queue[i].gameObject);
                Queue[i] = null;
            }
        }
    }
}
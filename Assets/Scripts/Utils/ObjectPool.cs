using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CyberSpeed.UI;

namespace CyberSpeed.Utils
{
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;
        [SerializeField] private bool allowGrowth = true;
        [SerializeField] Transform parent;

        private Queue<GameObject> pool = new Queue<GameObject>();
        private void Awake()
        {
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Instantiate(prefab, parent);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// Get an object from the pool, or create one if none available.
        /// </summary>
        public GameObject Get()
        {
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }

            if (allowGrowth)
                return Instantiate(prefab, parent);

            return null;
        }

        /// <summary>
        /// Return an object back to the pool.
        /// </summary>
        public void Release(GameObject obj)
        {
            // Reset GameCard component if present
            var gameCard = obj.GetComponent<GameCard>();
            if (gameCard != null)
            {
                gameCard.ResetCard();
            }

            // Reset transform parent and position
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}

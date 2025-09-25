using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CyberSpeed.UI;

namespace CyberSpeed.Utils
{
    public class ObjectPool : MonoBehaviour
    {
        public bool incrementalPool = true;

        private GameObject mPrefab;

        private int poolLength = 4;
        private Transform pooledObjectParent;
        private List<GameObject> pooledObjectList;
        private GameObject mObject;

        // Init Obj Pool
        public void InitializePool(GameObject inPrefab, int inPoolSize = 4)
        {
            poolLength = inPoolSize;
            mPrefab = inPrefab;

            pooledObjectParent = this.transform;

            pooledObjectList = new List<GameObject>();
            for (int index = 0; index < poolLength; index++)
            {
                mObject = GameObject.Instantiate(mPrefab, pooledObjectParent) as GameObject;
                pooledObjectList.Add(mObject);
                mObject.SetActive(false);
            }

            mObject = null;
        }

        ///  <summary>Get the curretly deactivate object from the pool</summary>
        public GameObject GetObjectFromPool()
        {
            for (int index = 0; index < pooledObjectList.Count; index++)
            {
                if (!pooledObjectList[index].activeInHierarchy)
                    return pooledObjectList[index];
            }

            if (incrementalPool)
            {
                mObject = GameObject.Instantiate(mPrefab, pooledObjectParent) as GameObject;
                pooledObjectList.Add(mObject);
                return mObject;
            }

            return null;
        }

        /// <summary>Reset and Deactivate all pooled objects</summary>
        public void ReleaseAll()
        {
            if (pooledObjectList == null) return;

            for (int index = 0; index < pooledObjectList.Count; index++)
            {
                if (pooledObjectList[index] != null)
                {
                    pooledObjectList[index].SetActive(false);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{

    public class MixedObjectPoolExample : MonoBehaviour
    {
        public GameObject template;

        private IMixedObjectPool<GameObject> pool;
        private Dictionary<string, List<GameObject>> dict;

        private void Start()
        {
            CubeMixedObjectFactory factory = new CubeMixedObjectFactory(template, transform);
            pool = new MixedObjectPool<GameObject>(factory, 5);
            dict = new Dictionary<string, List<GameObject>>();
        }

        private void OnDestroy()
        {
            if (pool != null)
            {
                pool.Dispose();
                pool = null;
            }
        }

        void OnGUI()
        {
            int x = 50;
            int y = 50;
            int width = 100;
            int height = 60;
            int i = 0;
            int padding = 10;

            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), "Add(red)"))
            {
                Add("red", 1);
            }
            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), "Add(green)"))
            {
                Add("green", 1);
            }
            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), "Add(blue)"))
            {
                Add("blue", 1);
            }

            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), "Delete(red)"))
            {
                Delete("red", 1);
            }
            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), "Delete(green)"))
            {
                Delete("green", 1);
            }
            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), "Delete(blue)"))
            {
                Delete("blue", 1);
            }
        }

        protected void Add(string typeName, int count)
        {
            if (!dict.TryGetValue(typeName, out var list))
            {
                list = new List<GameObject>();
                dict.Add(typeName, list);
            }

            for (int i = 0; i < count; i++)
            {
                GameObject go = pool.Allocate(typeName);
                go.transform.position = GetPosition();
                go.name = $"Cube {typeName}-{list.Count}";
                go.SetActive(true);
                list.Add(go);
            }
        }

        protected void Delete(string typeName, int count)
        {
            if (!dict.TryGetValue(typeName, out var list) || list.Count <= 0)
                return;

            for (int i = 0; i < count; i++)
            {
                if (list.Count <= 0)
                    return;

                int index = list.Count - 1;
                GameObject go = list[index];
                list.RemoveAt(index);

                //this.pool.Free(go);
                //or
                IPooledObject freeable = go.GetComponent<IPooledObject>();
                freeable.Free();
            }
        }

        protected Vector3 GetPosition()
        {
            float x = UnityEngine.Random.Range(-10, 10);
            float y = UnityEngine.Random.Range(-5, 5);
            float z = UnityEngine.Random.Range(-10, 10);
            return new Vector3(x, y, z);
        }

        public class CubeMixedObjectFactory : UnityMixedGameObjectFactoryBase
        {
            private readonly GameObject template;
            private readonly Transform parent;
            public CubeMixedObjectFactory(GameObject template, Transform parent)
            {
                this.template = template;
                this.parent = parent;
            }

            protected override GameObject Create(string typeName)
            {
                Debug.LogFormat("Create a cube.");
                GameObject go = Instantiate(template, parent);
                go.GetComponent<MeshRenderer>().material.color = GetColor(typeName);
                return go;
            }

            protected Color GetColor(string typeName)
            {
                if (typeName.Equals("red"))
                    return Color.red;
                if (typeName.Equals("green"))
                    return Color.green;
                if (typeName.Equals("blue"))
                    return Color.blue;

                throw new NotSupportedException("Unsupported type:" + typeName);
            }

            public override void Reset(string typeName, GameObject obj)
            {
                obj.SetActive(false);
                obj.name = $"Cube {typeName}-Idle";
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.Euler(Vector3.zero);
            }

            public override void Destroy(string typeName, GameObject obj)
            {
                base.Destroy(typeName, obj);
                Debug.LogFormat("Destroy a cube.");
            }
        }
    }
}
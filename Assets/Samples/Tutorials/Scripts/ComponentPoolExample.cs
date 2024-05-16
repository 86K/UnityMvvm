using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class ComponentPoolExample : MonoBehaviour
    {
        public GameObject template;

        private IObjectPool<MeshRenderer> pool;
        private List<MeshRenderer> list;
        private List<Color> colors;

        private void Start()
        {
            CubeObjectFactory2 factory = new CubeObjectFactory2(template, transform);
            pool = new ObjectPool<MeshRenderer>(factory, 10, 20);

            list = new List<MeshRenderer>();
            colors = new List<Color>()
            {
                Color.black,
                Color.blue,
                Color.red,
                Color.yellow,
                Color.white,
                Color.green
            };

            Add(10);
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

            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), "Add"))
            {
                Add(1);
            }

            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), "Delete"))
            {
                Delete(1);
            }
        }

        protected void Add(int count)
        {
            for (int i = 0; i < count; i++)
            {
                MeshRenderer go = pool.Allocate();
                go.material.color = GetColor();
                go.transform.position = GetPosition();
                go.name = $"Cube {list.Count}";
                go.gameObject.SetActive(true);
                list.Add(go);
            }
        }

        protected void Delete(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (list.Count <= 0)
                    return;

                int index = list.Count - 1;
                MeshRenderer go = list[index];
                list.RemoveAt(index);

                //this.pool.Free(go);
                //or
                IPooledObject freeable = go.GetComponent<IPooledObject>();
                freeable.Free();
            }
        }

        protected Color GetColor()
        {
            int index = Random.Range(0, colors.Count);
            return colors[index];
        }

        protected Vector3 GetPosition()
        {
            float x = Random.Range(-10, 10);
            float y = Random.Range(-5, 5);
            float z = Random.Range(-10, 10);
            return new Vector3(x, y, z);
        }
    }
}
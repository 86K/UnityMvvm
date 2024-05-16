using UnityEngine;

namespace Fusion.Mvvm
{
    public class CubeObjectFactory : UnityGameObjectFactoryBase
    {
        private readonly GameObject template;
        private readonly Transform parent;
        public CubeObjectFactory(GameObject template, Transform parent)
        {
            this.template = template;
            this.parent = parent;
        }

        protected override GameObject Create()
        {
            Debug.LogFormat("Create a cube.");
            return Object.Instantiate(template, parent);
        }

        public override void Reset(GameObject obj)
        {
            obj.SetActive(false);
            obj.name = "Cube Idle";
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.Euler(Vector3.zero);
        }

        public override void Destroy(GameObject obj)
        {
            base.Destroy(obj);
            Debug.LogFormat("Destroy a cube.");
        }
    }
}

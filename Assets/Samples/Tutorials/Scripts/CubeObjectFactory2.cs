using UnityEngine;

namespace Fusion.Mvvm
{
    public class CubeObjectFactory2 : UnityComponentFactoryBase<MeshRenderer>
    {
        private readonly GameObject template;
        private readonly Transform parent;
        public CubeObjectFactory2(GameObject template, Transform parent)
        {
            this.template = template;
            this.parent = parent;
        }

        protected override MeshRenderer Create()
        {
            Debug.LogFormat("Create a cube.");
            GameObject go = Object.Instantiate(template, parent);
            return go.GetComponent<MeshRenderer>();
        }

        public override void Reset(MeshRenderer obj)
        {
            obj.gameObject.SetActive(false);
            obj.gameObject.name = "Cube Idle";
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.Euler(Vector3.zero);
        }

        public override void Destroy(MeshRenderer obj)
        {
            base.Destroy(obj);
            Debug.LogFormat("Destroy a cube.");
        }
    }
}

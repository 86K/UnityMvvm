

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    [RequireComponent(typeof(Image))]
    public class AsyncSpriteLoader : MonoBehaviour
    {
        private Image target;
        private string spriteName;
        public Sprite defaultSprite;
        public Material defaultMaterial;
        public string spritePath;

        public string SpriteName
        {
            get => spriteName;
            set
            {
                if (spriteName == value)
                    return;

                spriteName = value;
                if (target != null)
                    OnSpriteChanged();
            }
        }

        protected virtual void OnEnable()
        {
            target = GetComponent<Image>();
        }

        protected virtual void OnSpriteChanged()
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                target.sprite = null;
                target.material = null;
                return;
            }

            target.sprite = defaultSprite;
            target.material = defaultMaterial;

            StartCoroutine(LoadSprite());
        }

        /// <summary>
        /// Simulate the way asynchronous loading
        /// </summary>
        /// <returns></returns>
        IEnumerator LoadSprite()
        {
            yield return new WaitForSeconds(1f); 

            Sprite[] sprites = Resources.LoadAll<Sprite>(spritePath);
            foreach(var sprite in sprites)
            {
                if(sprite.name.Equals(spriteName))
                {
                    target.sprite = sprite;
                    target.material = null;
                }
            }
        }
    }
}

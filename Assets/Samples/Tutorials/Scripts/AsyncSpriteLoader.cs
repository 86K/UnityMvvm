/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Loxodon.Framework.Tutorials
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

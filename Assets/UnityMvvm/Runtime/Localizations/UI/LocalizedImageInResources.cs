

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    [AddComponentMenu("Loxodon/Localization/LocalizedImageInResources")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public class LocalizedImageInResources : AbstractLocalized<Image>
    {
        protected override void OnValueChanged(object sender, EventArgs e)
        {
            object v = value.Value;
            if (v is Sprite)
            {
                target.sprite = (Sprite)v;
            }
            else if (v is string)
            {
                string path = (string)v;
                StartCoroutine(DoLoad(path));
            }
            else if (v != null)
            {
                Debug.LogWarning(string.Format("There is an invalid localization value \"{0}\" on the GameObject named \"{1}\".", v, name));
            }
        }

        protected virtual IEnumerator DoLoad(string path)
        {
            var result = Resources.LoadAsync<Sprite>(path);
            yield return result;
            target.sprite = (Sprite)result.asset;
        }
    }
}

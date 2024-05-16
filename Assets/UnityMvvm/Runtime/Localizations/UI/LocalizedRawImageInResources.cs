

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    [AddComponentMenu("Loxodon/Localization/LocalizedRawImageInResources")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RawImage))]
    public class LocalizedRawImageInResources : AbstractLocalized<RawImage>
    {
        protected override void OnValueChanged(object sender, EventArgs e)
        {
            object v = value.Value;
            if (v is Texture2D)
            {
                target.texture = (Texture2D)v;
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
            var result = Resources.LoadAsync<Texture2D>(path);
            yield return result;
            target.texture = (Texture2D)result.asset;
        }
    }
}

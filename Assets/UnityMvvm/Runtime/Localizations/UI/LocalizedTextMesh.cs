

using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    [AddComponentMenu("Loxodon/Localization/LocalizedTextMesh")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMesh))]
    public class LocalizedTextMesh : AbstractLocalized<TextMesh>
    {
        protected override void OnValueChanged(object sender, EventArgs e)
        {
            target.text = (string)Convert.ChangeType(value.Value, typeof(string));
        }
    }
}

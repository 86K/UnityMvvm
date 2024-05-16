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

using Loxodon.Framework.Binding;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Loxodon.Framework.Localizations
{
    [AddComponentMenu("Loxodon/Localization/LocalizedDataBinder")]
    [DisallowMultipleComponent]
    [AllowedMembers(typeof(RectTransform), "offsetMax", "offsetMin", "pivot", "sizeDelta", "anchoredPosition", "anchorMax", "anchoredPosition3D", "rect", "anchorMin")]
    [AllowedMembers(typeof(Image), "sprite", "material", "color")]
    [AllowedMembers(typeof(RawImage), "texture", "material", "color")]
    [AllowedMembers(typeof(SpriteRenderer), "sprite", "color", "drawMode")]
    [AllowedMembers(typeof(Text), "text", "font", "fontStyle", "fontSize", "color")]
    [AllowedMembers(typeof(TextMesh), "text", "font", "fontStyle", "fontSize", "color")]
    [AllowedMembers(typeof(AudioSource), "clip")]
    [AllowedMembers(typeof(VideoPlayer), "clip", "url")]
    public class LocalizedDataBinder : MonoBehaviour
    {
        [SerializeField]
        protected LocalizedBindingDescriptionSet data = new LocalizedBindingDescriptionSet();

        protected virtual void Start()
        {
            var localization = Localization.Current;
            var bindingSet = this.CreateSimpleBindingSet();
            foreach (var description in data.descriptions)
            {
                string typeName = description.TypeName;
                var target = GetComponentByName(typeName);
                if (target == null)
                    throw new MissingComponentException($"Not found the \"{typeName}\" component.");

                string propertyName = description.PropertyName;
                string key = description.Key;
                BindingMode mode = description.Mode;
                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogWarning(string.Format("The key is null or empty.Please check the binding \"{0}\" in the GameObject \"{1}\"", description.ToString(), name));

                    continue;
                }

                var value = localization.GetValue(key);
                var builder = bindingSet.Bind(target).For(propertyName).ToValue(value);
                switch (mode)
                {
                    case BindingMode.OneTime:
                        builder.OneTime();
                        break;
                    default:
                        builder.OneWay();
                        break;
                }
            }
            bindingSet.Build();
        }


        protected virtual Component GetComponentByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            foreach (AllowedMembersAttribute attribute in GetType().GetCustomAttributes(typeof(AllowedMembersAttribute), true))
            {
                Type type = attribute.Type;
                if (!typeName.Equals(type.FullName))
                    continue;

                Component component = GetComponent(type);
                if (component != null)
                    return component;
                break;
            }

            return GetComponent(typeName);
        }
    }
}

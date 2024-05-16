using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class SpriteConverter : IConverter
    {
        private readonly Dictionary<string, Sprite> sprites;

        public SpriteConverter(Dictionary<string, Sprite> sprites)
        {
            this.sprites = sprites;
        }

        public object Convert(object value)
        {
            Sprite sprite = null;
            if (value != null)
                sprites.TryGetValue((string)value, out sprite);
            return sprite;
        }

        public object ConvertBack(object value)
        {
            throw new NotImplementedException();
        }
    }
}

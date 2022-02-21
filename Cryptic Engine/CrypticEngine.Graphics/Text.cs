using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CrypticEngine.Graphics
{
    public sealed class Text
    {
        private string text;
        private SpriteFont font;
        private Vector2 size;
        private Color color;
        private CrypticDraw.SpriteAnchor anchor;

        public Text(string text, SpriteFont font, Color color = default, CrypticDraw.SpriteAnchor anchor = CrypticDraw.SpriteAnchor.MiddleLeft)
        {
            if (color == default)
                color = Color.Black;

            this.text = text;
            this.font = font;
            this.color = color;
            this.anchor = anchor;
            size = font.MeasureString(text);
        }

        /// <summary> Sets the Text string value and recalculates the pixel size </summary>
        public void setText(string text)
        {
            this.text = text;
            size = font.MeasureString(text);
        }

        public string getText()
        {
            return text;
        }

        public Vector2 getSize()
        {
            return size;
        }

        public void setColor(Color color)
        {
            this.color = color;
        }

        public Color getColor()
        {
            return color;
        }

        /// <summary> Sets the Text font and recalculates the pixel size </summary>
        public void setFont(SpriteFont font)
        {
            this.font = font;
            size = font.MeasureString(text);
        }

        public SpriteFont getFont()
        {
            return font;
        }

        public void setAnchor(CrypticDraw.SpriteAnchor anchor)
        {
            this.anchor = anchor;
        }

        public CrypticDraw.SpriteAnchor getAnchor()
        {
            return anchor;
        }
    }
}

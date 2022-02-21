using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CrypticEngine.Graphics
{
    public sealed class CrypticDraw
    {
        private SpriteBatch spriteBatch;
        private Camera2D camera;

        public enum SpriteAnchor
        {
            Center,
            MiddleTop,
            MiddleBottom,
            MiddleLeft,
            MiddleRight,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }

        public CrypticDraw(SpriteBatch batch, Camera2D camera)
        {
            spriteBatch = batch;
            this.camera = camera;
        }

        /// <summary> The draw function for sprites. Includes optional layer, scale, rotation, color, anchor, and sprite effect </summary>
        public void drawSprite(float x, float y, Texture2D source, float layer = 0, float scale = 1, float rotation = 0, Color color = default, SpriteAnchor anchor = SpriteAnchor.Center, SpriteEffects effect = SpriteEffects.None)
        {
            if (color == default)
                color = Color.White;

            Vector2 origin = getAnchorVector(source.Width, source.Height, anchor);
            spriteBatch.Draw(source, new Vector2(x * camera.getWorldZoom(), y * camera.getWorldZoom()), new Rectangle(0, 0, source.Width, source.Height), color, rotation, origin, scale * camera.getWorldZoom(), effect, layer);
        }


        /// <summary> The animation draw function that takes a source rectangle. Includes optional layer, scale, rotation, color, anchor, and sprite effect </summary>
        public void drawSprite(float x, float y, Texture2D source, Rectangle sourceRect, float layer = 0, float scale = 1, float rotation = 0, Color color = default, SpriteAnchor anchor = SpriteAnchor.Center, SpriteEffects effect = SpriteEffects.None)
        {
            if (color == default)
                color = Color.White;

            Vector2 origin = getAnchorVector(source.Width, source.Height, anchor);
            spriteBatch.Draw(source, new Vector2(x * camera.getWorldZoom(), y * camera.getWorldZoom()), sourceRect, color, rotation, origin, scale * camera.getWorldZoom(), effect, layer);
        }

        /// <summary> The draw string function. Includes optional scale, layer, rotation, draw space, and sprite effect </summary>
        public void drawString(Text text, float x, float y, float scale = 0.5f, float layer = 0f, float rotation = 0, bool screenSpace = true, SpriteEffects effect = SpriteEffects.None)
        {
            Vector2 origin;
            Vector2 pos;
            float cameraScale;

            origin = getAnchorVector((int)text.getSize().X, (int)text.getSize().Y, text.getAnchor());
            pos = (screenSpace ? camera.screenSpace(x, y) : new Vector2(x, y));
            cameraScale = (screenSpace ? 1 : camera.getWorldZoom()); //Screen space UI does not scale with the camera world zoom
       
            spriteBatch.DrawString(text.getFont(), text.getText(), new Vector2(pos.X, pos.Y) * cameraScale, text.getColor(), rotation, origin, new Vector2(scale, scale) * cameraScale, effect, layer);
        }

        /// <summary> The draw string function for color formatted text. Includes optional scale, layer, rotation, draw space, and sprite effect </summary>
        public void drawString(ColorText colorText, float x, float y, float scale = 0.5f, float layer = 0f, float rotation = 0, bool screenSpace = true, SpriteEffects effect = SpriteEffects.None)
        {
            Vector2 origin;
            Vector2 pos;
            float cameraScale;

            origin = getAnchorVector((int)colorText.getSize().X, (int)colorText.getSize().Y, colorText.getAnchor());
            pos = (screenSpace ? camera.screenSpace(x, y) : new Vector2(x, y));
            cameraScale = (screenSpace ? 1 : camera.getWorldZoom()); //Screen space UI does not scale with the camera world zoom

            for (int i = 0; i < colorText.getTextPieces().Count; i++)
                spriteBatch.DrawString(colorText.getFont(), colorText.getTextPieces()[i].getText(), new Vector2(pos.X + colorText.getOffsets()[i], pos.Y) * cameraScale, colorText.getTextPieces()[i].getColor(), rotation, origin, new Vector2(scale, scale) * cameraScale, effect, layer);
        }

        /// <summary> Returns the vector associated with the given sprite anchor enum </summary>
        private Vector2 getAnchorVector(int width, int height, SpriteAnchor anchor)
        {
            switch (anchor)
            {
                case SpriteAnchor.Center:
                    return new Vector2(width / 2, height / 2);
                case SpriteAnchor.MiddleTop:
                    return new Vector2(width / 2, 0);
                case SpriteAnchor.MiddleBottom:
                    return new Vector2(width / 2, height);
                case SpriteAnchor.MiddleLeft:
                    return new Vector2(0, height / 2);
                case SpriteAnchor.MiddleRight:
                    return new Vector2(width, height / 2);
                case SpriteAnchor.TopLeft:
                    return new Vector2(0, 0);
                case SpriteAnchor.TopRight:
                    return new Vector2(width, 0);
                case SpriteAnchor.BottomLeft:
                    return new Vector2(0, height);
                case SpriteAnchor.BottomRight:
                    return new Vector2(width, height);
                default:
                    return Vector2.Zero;
            }
        }
    }
}

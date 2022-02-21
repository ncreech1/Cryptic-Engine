using System;
using System.Collections.Generic;
using System.Text;
using CrypticEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CrypticEngine.Scripts
{
    public class TextRenderer : CrypticScript
    {
        public string text;
        public string fontPath;
        public Color color;
        public bool worldSpace;
        public CrypticDraw.SpriteAnchor anchor;
        public bool richText;
        private Text textObj;
        private ColorText colorText;
        private SpriteFont font;
        private string textOld;

        public TextRenderer()
        {
            text = "";
            fontPath = "";
            color = Color.Black;
            anchor = CrypticDraw.SpriteAnchor.MiddleLeft;
            richText = false;
        }

        public TextRenderer(string text, SpriteFont font, Color color = default, CrypticDraw.SpriteAnchor anchor = CrypticDraw.SpriteAnchor.MiddleLeft, bool richText = false)
        {
            this.text = text;
            this.font = font;
            this.color = color;
            this.anchor = anchor;
            this.richText = richText;

            if (!richText)
                textObj = new Text(text, font, color, anchor);
            else
                colorText = new ColorText(text, font, color, anchor);
        }

        public override void start(CrypticGame game) 
        {
            if (fontPath != null)
            {
                try { font = game.getContent().Load<SpriteFont>(fontPath); }
                catch (ContentLoadException) { CrypticGame.printError($"Unable to load font \"{fontPath}\""); }
            }

            if (font != null)
            {
                if (!richText)
                    textObj = new Text(text, font, color, anchor);
                else
                    colorText = new ColorText(text, font, color, anchor);
            }

            else
                CrypticGame.printWarning($"No font set for TextRenderer script on CrypticObject \"{parent.getName()}\"");
        }

        public override void update(CrypticGame game, GameTime gameTime) { }

        public override void draw(CrypticGame game, GameTime gameTime)
        {
            if (font != null)
            {
                //Ensure text is rebuilt if text is changed outside of setText()
                if(text != textOld)
                    setText(text);

                //Ensure both text objects are built if rich text setting is changed outside of setRichText()
                if (!richText && textObj == null)
                    textObj = new Text(text, font, color, anchor);
                else if (richText && colorText == null)
                    colorText = new ColorText(text, font, color, anchor);

                if(!richText)
                    game.getDraw().drawString(textObj, transform.position.X, transform.position.Y, transform.scale, transform.position.Z, transform.rotation, !worldSpace);
                else
                    game.getDraw().drawString(colorText, transform.position.X, transform.position.Y, transform.scale, transform.position.Z, transform.rotation, !worldSpace);
            }
        }

        /// <summary> Sets the text string of the TextRenderer. Rebuilds the Text or ColorText object </summary>
        public void setText(string text)
        {
            this.text = text;
            textOld = text;

            if (!richText)
                textObj.setText(text);
            else
                colorText.setText(text);
        }

        /// <summary> Sets the color of the TextRenderer. Rebuilds the Text or ColorText object </summary>
        public void setColor(Color color)
        {
            this.color = color;

            if (!richText)
                textObj.setColor(color);
            else
                colorText.setColor(color);
        }

        /// <summary> Sets the font of the TextRenderer. Rebuilds the Text or ColorText object </summary>
        public void setFont(SpriteFont font)
        {
            this.font = font;

            if (!richText)
                textObj.setFont(font);
            else
                colorText.setFont(font);
        }

        /// <summary> Sets the font of the TextRenderer. Rebuilds the Text or ColorText object </summary>
        public void setAnchor(CrypticDraw.SpriteAnchor anchor)
        {
            this.anchor = anchor;

            if (!richText)
                textObj.setAnchor(anchor);
            else
                colorText.setAnchor(anchor);
        }

        /// <summary> Enables or disables rich text for the TextRenderer. Rebuilds the Text or ColorText object </summary>
        public void setRichText(bool richText)
        {
            this.richText = richText;

            if (!richText)
                textObj = new Text(text, font, color, anchor);
            else
                colorText = new ColorText(text, font, color, anchor);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrypticEngine.Graphics
{
    public sealed class ColorText
    {
        private List<Text> textPieces;
        private List<int> offsets;
        private SpriteFont font;
        private Color color;
        private CrypticDraw.SpriteAnchor anchor;
        private string text;
        private string plainText;
        private Vector2 size;

        public ColorText(string text, SpriteFont font, Color color = default, CrypticDraw.SpriteAnchor anchor = CrypticDraw.SpriteAnchor.MiddleLeft)
        {
            if (color == default)
                color = Color.Black;

            this.font = font;
            this.color = color;
            this.anchor = anchor;

            textPieces = new List<Text>();
            offsets = new List<int>();
            setText(text, font, color, anchor);
        }

        /// <summary> Builds the ColorText object from a formatted color string. Returns true on success or false on failure </summary>
        /// <exception cref="NullReferenceException"> text or font is null</exception>
        public bool setText(string text, SpriteFont font, Color color = default, CrypticDraw.SpriteAnchor anchor = CrypticDraw.SpriteAnchor.MiddleLeft)
        {
            string unformattedPiece = "";
            int offset = 0;

            textPieces.Clear();
            offsets.Clear();
            plainText = "";
            this.text = text;

            if (font == null || text == null)
                throw new NullReferenceException("Formatted string and font must not be null");

            for(int x = 0; x < text.Length; x++)
            {
                //Found start of color identifier (e.g. #FF0000[red])
                if (text[x] == '#' && (x == 0 || text[x - 1] != '\\'))
                {
                    int r, g, b;
                    Color pieceColor;
                    string piece = "";
                    bool endFound = false;

                    //Add the piece up to the current color indentifier
                    if (unformattedPiece != "")
                    {
                        Text textPiece = new Text(unformattedPiece, font, color, anchor);
                        textPieces.Add(textPiece);
                        offsets.Add(offset);
                        offset += (int)textPiece.getSize().X;
                        plainText += unformattedPiece;
                        unformattedPiece = "";
                    }

                    //End bracket not found
                    if (x + 8 >= text.Length)
                    {
                        CrypticGame.printWarning($"String Color Format Exception for string \"{text}\": Missing closing bracket ']'");
                        break;
                    }

                    //Start bracket not found
                    if (text[x + 7] != '[')
                    {
                        CrypticGame.printWarning($"String Color Format Exception for string \"{text}\": Missing opening bracket '['");
                        break;
                    }

                    //Convert color identifier from hex string to Color object
                    try
                    {
                        string hexColor = text.Substring(x + 1, 6);
                        r = int.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                        g = int.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        b = int.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                        pieceColor = new Color(r, g, b);
                    }

                    catch (FormatException) { CrypticGame.printWarning($"String Color Format Exception for string \"{text}\": 6 digit hex string expected after '#'"); break; }

                    //Get the text piece wrapped in the current identifier
                    for (int i = x + 8; i < text.Length; i++)
                    {
                        if (text[i] == ']' && text[i - 1] != '\\')
                        {
                            Text textPiece = new Text(piece, font, pieceColor, anchor);
                            textPieces.Add(textPiece);
                            offsets.Add(offset);
                            offset += (int)textPiece.getSize().X;
                            plainText += piece;
                            endFound = true;
                            x = i;
                            break;
                        }

                        else
                            piece += text[i];
                    }

                    //End bracket not found
                    if (!endFound)
                    {
                        CrypticGame.printWarning($"String Color Format Exception for string \"{text}\": Missing closing bracket ']'");
                        break;
                    }
                }

                else
                {
                    unformattedPiece += text[x];

                    //Successfully parsed color formatted string
                    if (x == text.Length - 1)
                    {
                        //Add the piece up to the current color indentifier
                        if (unformattedPiece != "")
                        {
                            Text textPiece = new Text(unformattedPiece, font, color, anchor);
                            textPieces.Add(textPiece);
                            offsets.Add(offset);
                            plainText += unformattedPiece;
                        }

                        size = font.MeasureString(plainText);

                        return true;
                    }
                }
            }

            //Failed to parse color formatted string
            textPieces.Add(new Text(text, font, color, anchor));
            offsets.Add(0);
            return false;
        }

        /// <summary> Sets the ColorText string value. Rebuilds the ColorText object. </summary>
        public bool setText(string text)
        {
            return setText(text, font, color, anchor);
        }

        public List<Text> getTextPieces()
        {
            return textPieces;
        }

        public List<int> getOffsets()
        {
            return offsets;
        }

        /// <summary> Sets the ColorText font. Rebuilds the ColorText object. </summary>
        public void setFont(SpriteFont font)
        {
            this.font = font;
            setText(text, font, color, anchor);
        }

        public SpriteFont getFont()
        {
            return font;
        }

        /// <summary> Sets the ColorText color. Rebuilds the ColorText object. </summary>
        public void setColor(Color color)
        {
            this.color = color;
            setText(text, font, color, anchor);
        }

        public Color getColor()
        {
            return color;
        }

        /// <summary> Sets the ColorText anchor. Rebuilds the ColorText object. </summary>
        public void setAnchor(CrypticDraw.SpriteAnchor anchor)
        {
            this.anchor = anchor;
            setText(text, font, color, anchor);
        }

        public CrypticDraw.SpriteAnchor getAnchor()
        {
            return anchor;
        }

        /// <summary> Returns the formatted color string </summary>
        public string getText()
        {
            return plainText;
        }

        /// <summary> Returns the unformatted color string </summary>
        public string getPlainText()
        {
            return plainText;
        }

        /// <summary> Returns the size of the plain text string </summary>
        public Vector2 getSize()
        {
            return size;
        }
    }
}

using Microsoft.Xna.Framework;
using System;

namespace CrypticEngine.Graphics
{
    public abstract class TextHandler
    {
        /// <summary> Loads the game fonts from the content manager </summary>
        public abstract void loadFonts(Game game);

        /// <summary> Color formats the provided string with a Color object </summary>
        public static string colorString(string piece, Color color)
        {
            //Ex: This a string with a #FF0000[red] piece!
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + "[" + piece + "]";
        }

        /// <summary> Color formats the provided string with the RGB components of a Color object </summary>
        public static string colorString(string piece, int r, int g, int b)
        {
            //Ex: This a string with a #FF0000[red] piece!
            return "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2") + "[" + piece + "]";
        }

        /// <summary> Attempts to parse a Vector from a string with the given number of axes (2 to 4). </summary>
        /// <exception cref="FormatException"> vstr does not respresent a Vector in a valid format for the given axes. </exception>
        private static Vector4 parseVectorString(string vstr, int axes)
        {
            Vector4 result;
            string currentVal = "";
            bool startFound = false;
            bool endFound = false;
            int axis = 0;

            if(axes < 2 || axes > 4)
                throw new FormatException($"Cannot parse a vector string with {axes} axes");

            result = new Vector4();

            for (int x = 0; x < vstr.Length; x++)
            {
                switch (vstr[x])
                {
                    case '(':
                        if (startFound || endFound)
                            throw new FormatException($"Unexpected character '(' at character {x}");
                        startFound = true;
                        break;
                    case ')':
                        if (startFound && !endFound && axis == axes - 1)
                        {
                            endFound = true;
                            try 
                            {
                                if (axis == 1)
                                    result.Y = float.Parse(currentVal);
                                if (axis == 2)
                                    result.Z = float.Parse(currentVal);
                                if (axis == 3)
                                    result.W = float.Parse(currentVal);
                            }
                            catch (FormatException) { throw new FormatException($"\"{currentVal}\" is not a valid float"); }
                            break;
                        }
                        throw new FormatException($"Unexpected character ')' at character {x}");
                    case ',':
                        if (startFound && !endFound && axis < axes)
                        {
                            try
                            {
                                if (axis == 0)
                                    result.X = float.Parse(currentVal);
                                if (axis == 1)
                                    result.Y = float.Parse(currentVal);
                                if (axis == 2)
                                    result.Z = float.Parse(currentVal);
                            }

                            catch (FormatException) { throw new FormatException($"\"{currentVal}\" is not a valid float"); }

                            axis++;
                            currentVal = "";
                            break;
                        }
                        throw new FormatException($"Unexpected character ',' at character {x}");
                    case ' ':
                        break;
                    default:
                        if (startFound && !endFound)
                        {
                            currentVal += vstr[x];
                            break;
                        }
                        throw new FormatException($"Unexpected character '{vstr[x]}' at column {x}");
                }
            }

            if (!endFound)
                throw new FormatException("Unexpected end of vector string");

            return result;
        }

        /// <summary> Attempts to parse a Vector4 from a string. </summary>
        /// <exception cref="FormatException"> vstr does not respresent a Vector4 in a valid format. </exception>
        public static Vector4 parseVector4(string vstr)
        {
            Vector4 result;

            try { result = parseVectorString(vstr, 4); }
            catch (FormatException e) { throw e; }

            return result;
        }

        /// <summary> Attempts to parse a Vector4 from a string. Returns false and sets result to Vecor4.Zero on failure. </summary>
        public static bool tryParseVector4(string vstr, out Vector4 result)
        {
            try { result = parseVector4(vstr); }
            catch (FormatException e)
            {
                CrypticGame.printWarning($"Unable to parse Vector4 from string \"{vstr}\"; {e.Message}");
                result = Vector4.Zero;
                return false;
            }

            return true;
        }

        /// <summary> Attempts to parse a Vector3 from a string. </summary>
        /// <exception cref="FormatException"> vstr does not respresent a Vector3 in a valid format. </exception>
        public static Vector3 parseVector3(string vstr)
        {
            Vector3 result;

            try 
            {
                Vector4 parsedVector;
                parsedVector = parseVectorString(vstr, 3);
                result = new Vector3(parsedVector.X, parsedVector.Y, parsedVector.Z);
            }

            catch(FormatException e) { throw e; }

            return result;
        }

        /// <summary> Attempts to parse a Vector3 from a string. Returns false and sets result to Vecor3.Zero on failure. </summary>
        public static bool tryParseVector3(string vstr, out Vector3 result)
        {
            try { result = parseVector3(vstr); }
            catch (FormatException e)
            {
                CrypticGame.printWarning($"Unable to parse Vector3 from string \"{vstr}\"; {e.Message}");
                result = Vector3.Zero;
                return false;
            }

            return true;
        }

        /// <summary> Attempts to parse a Vector2 from a string. </summary>
        /// <exception cref="FormatException"> vstr does not respresent a Vector2 in a valid format. </exception>
        public static Vector2 parseVector2(string vstr)
        {
            Vector2 result;

            try
            {
                Vector4 parsedVector;
                parsedVector = parseVectorString(vstr, 2);
                result = new Vector2(parsedVector.X, parsedVector.Y);
            }

            catch (FormatException e) { throw e; }

            return result;
        }

        /// <summary> Attempts to parse a Vector2 from a string. Returns false and sets result to Vecor2.Zero on failure. </summary>
        public static bool tryParseVector2(string vstr, out Vector2 result)
        {
            try { result = parseVector2(vstr); }
            catch (FormatException e)
            {
                CrypticGame.printWarning($"Unable to parse Vector2 from string \"{vstr}\"; {e.Message}");
                result = Vector2.Zero;
                return false;
            }

            return true;
        }

        /// <summary> Attempts to parse a Color from a string. Returns false and sets result to Color.White on failure. </summary>
        public static bool tryParseColor(string vstr, out Color result)
        {
            Vector3 parsedColor;
            Vector4 parsedAlphaColor;
            bool parsedVector3 = true;
            bool parsedVector4 = true;
            string execptMsg = "";

            parsedColor = Vector3.Zero;
            parsedAlphaColor = Vector4.Zero;

            try { parsedColor = parseVector3(vstr); }
            catch (FormatException e) { parsedVector3 = false; execptMsg = e.Message; }

            try { parsedAlphaColor = parseVector4(vstr); }
            catch (FormatException) { parsedVector4 = false; }

            if (!parsedVector3 && !parsedVector4)
            {
                CrypticGame.printWarning($"Unable to parse Vector3 or Vector4 from string \"{vstr}\"; {execptMsg}");
                result = Color.White;
                return false;
            }

            else
            {
                if (parsedVector3)
                {
                    if (parsedColor.X > 1 || parsedColor.Y > 1 || parsedColor.Z > 1)
                        result = new Color((int)parsedColor.X, (int)parsedColor.Y, (int)parsedColor.Z);
                    else
                        result = new Color(parsedColor);
                }

                else
                {
                    if (parsedColor.X > 1 || parsedColor.Y > 1 || parsedColor.Z > 1)
                        result = new Color((int)parsedAlphaColor.X, (int)parsedAlphaColor.Y, (int)parsedAlphaColor.Z, (int)parsedAlphaColor.W);
                    else
                        result = new Color(parsedAlphaColor);
                }
            }

            return true;
        }
    }
}

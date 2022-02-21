using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace CrypticEngine.Input
{
    public static class KeyboardExtension
    {
        private static Dictionary<string, Keys> keyBindings = new Dictionary<string, Keys>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<Keys, bool> keysPressed = new Dictionary<Keys, bool>();

        /// <summary> Returns true if a key is being held down (true until key is released) </summary>
        public static bool getKeyHeld(Keys key)
        {
            bool pressed = Keyboard.GetState().IsKeyDown(key);

            keysPressed[key] = pressed;
            return pressed;
        }

        /// <summary> Returns true if a key is pressed (only true on the inital press; binding name is not case sensitive) </summary>
        public static bool getKeyDown(Keys key)
        {
            bool pressed = Keyboard.GetState().IsKeyDown(key);
            bool storedValue;

            keysPressed.TryGetValue(key, out storedValue);

            if (pressed && storedValue == true)
                return false;

            keysPressed[key] = pressed;
            return pressed;
        }

        /// <summary> Returns true if a key is being held down (true until key is released) </summary>
        public static bool getKeyHeld(string bindingName)
        {
            Keys key = getKey(bindingName);
            bool pressed = Keyboard.GetState().IsKeyDown(key);

            if (key == Keys.None)
                return false;

            keysPressed[key] = pressed;
            return pressed;
        }

        /// <summary> Returns true if a key is pressed (only true on the inital press; binding name is not case sensitive) </summary>
        public static bool getKeyDown(string bindingName)
        {
            Keys key = getKey(bindingName);
            bool pressed = Keyboard.GetState().IsKeyDown(key);
            bool storedValue;

            if (key == Keys.None)
                return false;

            keysPressed.TryGetValue(key, out storedValue);

            if (pressed && storedValue == true)
                return false;

            keysPressed[key] = pressed;
            return pressed;
        }

        /// <summary> Adds a new key binding to the game </summary>
        public static void addKeyBinding(string bindingName, Keys key)
        {
            Keys keyCheck;
            if (keyBindings.TryGetValue(bindingName, out keyCheck))
                CrypticGame.printWarning("Adding binding name '" + bindingName + "' for key '" + key.ToString() + "' ovewrites existing binding to key '" + keyCheck.ToString() + "'");

            keyBindings[bindingName] = key;
        }

        /// <summary> Converts a key binding name to its associated key (not case sensitive) </summary>
        private static Keys getKey(string bindingName)
        {
            Keys keyResult;

            if (keyBindings.TryGetValue(bindingName, out keyResult))
                return keyResult;
            else
                return Keys.None;
        }
    }
}

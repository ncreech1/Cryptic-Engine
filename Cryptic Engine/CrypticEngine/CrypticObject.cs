using CrypticEngine.Exception;
using CrypticEngine.Scripts;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CrypticEngine
{
    public sealed class CrypticObject
    {
        private bool enabled;
        private string name;
        private List<CrypticScript> scripts;
        private Transform transform;

        public CrypticObject()
        {
            enabled = true;
            scripts = new List<CrypticScript>();

            //Every CrypticObject has one and only one Transform Script
            transform = new Transform();
            addScript(transform);

            name = null;
        }

        public CrypticObject(string name) : this()
        {
            this.name = name;
        }

        /// <summary> Starts the CrypticObject and starts each attached script </summary>
        public void start(CrypticGame game)
        {
            foreach (CrypticScript script in scripts)
                script.start(game);
        }

        /// <summary> Updates the CrypticObject and updates each attached script </summary>
        public void update(CrypticGame game, GameTime gameTime)
        {
            foreach (CrypticScript script in scripts)
            {
                if(script.getEnabled())
                    script.update(game, gameTime);
            }
        }

        /// <summary> Calls the draw() function of each attached script </summary>
        public void draw(CrypticGame game, GameTime gameTime)
        {
            foreach (CrypticScript script in scripts)
            {
                if (script.getEnabled())
                    script.draw(game, gameTime);
            }
        }

        public void setEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        public bool getEnabled()
        {
            return enabled;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public string getName()
        {
            return name;
        }

        public void addScript(CrypticScript script)
        {
            foreach(CrypticScript s in scripts)
            {
                if (s.GetType() == script.GetType() && !script.getDuplicatesAllowed())
                    throw new DuplicateCrypticObjectException("Cannot add more than one [" + script.GetType().ToString() + "] script to CrypticObject '" + name + "'");
            }

            script.setParent(this);
            script.setTransform(transform);
            scripts.Add(script);
        }

        /// <summary> Gets the first CrypticScript of type T on this CrypticObject </summary>
        public T getScript<T>() where T : CrypticScript
        {
            foreach (CrypticScript s in scripts)
            {
                if (s.GetType() == typeof(T))
                    return s as T;
            }

            return null;
        }

        /// <summary> Gets all of the CrypticScripts of type T on this CrypticObject </summary>
        public List<T> getScriptsOfType<T>() where T : CrypticScript
        {
            List<T> result = new List<T>();

            foreach (CrypticScript s in scripts)
            {
                if (s.GetType() == typeof(T))
                    result.Add(s as T);
            }

            return result;
        }

        public Transform getTransform()
        {
            return transform;
        }
    }
}

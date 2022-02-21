using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrypticEngine
{
    public sealed class CrypticScene
    { 
        /// <summary> Keeps track of the CrypticObjects in this scene </summary>
        private Dictionary<string, List<CrypticObject>> crypticObjectStorage;

        private string name;
        private string subPath;
        private string fullPath;
        
        public CrypticScene(string name, string subPath = "")
        {
            if (name.EndsWith(".txt"))
                name = name.Substring(0, name.Length - 4);

            this.name = name;
            this.subPath = subPath;
            fullPath = CrypticGame.scenePath + '/' + subPath + '/' + name + ".txt";
            crypticObjectStorage = new Dictionary<string, List<CrypticObject>>();

            //Create directory if necessary
            if (!Directory.Exists(CrypticGame.scenePath + subPath))
                Directory.CreateDirectory(CrypticGame.scenePath + subPath);

            //Create scene file if it does not yet exist
            if (!File.Exists(fullPath))
                File.WriteAllText(fullPath, "# Define CrypticObjects here");
        }

        /// <summary> Called once when the scene is loaded. Starts all CrypticObjects and their scripts </summary>
        public void start(CrypticGame game)
        {
            foreach (KeyValuePair<string, List<CrypticObject>> pair in crypticObjectStorage)
            {
                foreach (CrypticObject obj in pair.Value)
                    obj.start(game);
            }
        }

        /// <summary> Called every frame. Updates CrypticObjects and their scripts </summary>
        public void update(CrypticGame game, GameTime gameTime)
        {
            foreach (KeyValuePair<string, List<CrypticObject>> pair in crypticObjectStorage)
            {
                foreach (CrypticObject obj in pair.Value)
                {
                    if(obj.getEnabled())
                        obj.update(game, gameTime);
                }
            }
        }

        /// <summary> Called every frame. Calls the draw() functions of CrypticObject scripts </summary>
        public void draw(CrypticGame game, GameTime gameTime)
        {
            game.getSpriteBatch().Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, game.getCamera().getViewTransformationMatrix());

            foreach (KeyValuePair<string, List<CrypticObject>> pair in crypticObjectStorage)
            {
                foreach (CrypticObject obj in pair.Value)
                {
                    if (obj.getEnabled())
                        obj.draw(game, gameTime);
                }
            }

            game.getSpriteBatch().End();
        }

        /// <summary> Adds a new CrypticObject to the scene </summary>
        public void addCrypticObject(CrypticObject crypticObject)
        {
            List<CrypticObject> objs;

            if(!crypticObjectStorage.TryGetValue(crypticObject.getName(), out objs))
            {
                objs = new List<CrypticObject>();
                objs.Add(crypticObject);

                crypticObjectStorage.Add(crypticObject.getName(), objs);
            }

            else
                objs.Add(crypticObject);
        }

        public Dictionary<string, List<CrypticObject>> getCrypticObjectStorage()
        {
            return crypticObjectStorage;
        }

        /// <summary> Frees the memory of the CrypticObjects loaded into the scene </summary>
        public void unloadCrypticObjects()
        {
            crypticObjectStorage.Clear();
        }

        /// <summary> Returns the first CrypticObject with the given name </summary>
        public CrypticObject find(string name)
        {
            List<CrypticObject> objs;

            if (crypticObjectStorage.TryGetValue(name, out objs))
                return objs[0];

            return null;
        }

        /// <summary> Removes the CrypticObject from the scene. Returns true if successful </summary>
        public bool destroy(CrypticObject obj)
        {
            List<CrypticObject> objs;

            if (crypticObjectStorage.TryGetValue(obj.getName(), out objs))
            {
                objs.Remove(obj);

                if(objs.Count == 0)
                    crypticObjectStorage.Remove(obj.getName());

                return true;
            }

            return false;
        }

        public string getName()
        {
            return name;
        }

        /// <summary> Returns the full path to the scene (.txt) file </summary>
        public string getFullPath()
        {
            return fullPath;
        }

        /// <summary> Returns the directory of the scene (.txt) file relative to '[Build Output]/[Scene Path]/' </summary>
        public string getSubPath()
        {
            return subPath;
        }
    }
}

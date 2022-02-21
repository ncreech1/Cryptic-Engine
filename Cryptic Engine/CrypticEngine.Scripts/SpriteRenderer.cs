using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CrypticEngine.Scripts
{
    public class SpriteRenderer : CrypticScript
    {
        public string spritePath;
        public Color color;
        private Texture2D sprite;

        public SpriteRenderer()
        {
            spritePath = "";
            duplicatesAllowed = false;
            color = Color.White;
        }

        public SpriteRenderer(string spritePath)
        {
            this.spritePath = spritePath;
            color = Color.White;
            duplicatesAllowed = false;
        }

        public override void start(CrypticGame game)
        {
            if (spritePath != "")
            {
                try { sprite = game.getContent().Load<Texture2D>(spritePath); }
                catch (ContentLoadException) { CrypticGame.printError($"Unable to load sprite \"{spritePath}\""); }
            }
        }

        public override void update(CrypticGame game, GameTime gameTime) {}

        public override void draw(CrypticGame game, GameTime gameTime)
        {
            if (sprite != null)
                game.getDraw().drawSprite(transform.position.X, transform.position.Y, sprite, transform.position.Z, transform.scale, transform.rotation, color: color);
        }
    }
}

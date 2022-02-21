using Microsoft.Xna.Framework;

namespace CrypticEngine.Scripts
{
    public sealed class Transform : CrypticScript
    {
        public Vector3 position;
        public float scale;
        public float rotation;

        public Transform()
        {
            position = new Vector3(0, 0, 1);
            scale = 1;
            rotation = 0;
            duplicatesAllowed = false;
        }

        public override void start(CrypticGame game) {}
        public override void update(CrypticGame game, GameTime gameTime) {}
        public override void draw(CrypticGame game, GameTime gameTime) {}

        public void setPosition(float x, float y, float z)
        {
            if (z > 1)
                z = 1;
            else if (z < 0)
                z = 0;

            position = new Vector3(x, y, z);
        }
    }
}

using Microsoft.Xna.Framework;

namespace CrypticEngine.Scripts
{
    public abstract class CrypticScript
    {
        protected CrypticObject parent;
        protected Transform transform;
        protected bool enabled = true;
        protected bool duplicatesAllowed = true;

        public abstract void start(CrypticGame game);
        public abstract void update(CrypticGame game, GameTime gameTime);
        public abstract void draw(CrypticGame game, GameTime gameTime);
 
        public void setParent(CrypticObject parent)
        {
            this.parent = parent;
        }

        public CrypticObject getParent()
        {
            return parent;
        }

        public void setTransform(Transform transform)
        {
            this.transform = transform;
        }

        public Transform getTransform()
        {
            return transform;
        }

        public void setEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        public bool getEnabled()
        {
            return enabled;
        }

        public bool getDuplicatesAllowed()
        {
            return duplicatesAllowed;
        }
    }
}

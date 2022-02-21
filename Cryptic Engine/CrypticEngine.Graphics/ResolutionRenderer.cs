using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CrypticEngine.Graphics
{
    public sealed class ResolutionRenderer
    {
        public int screenWidth;
        public int screenHeight;
        public int virtualWidth;
        public int virtualHeight;

        private GraphicsDeviceManager gdm;
        private Matrix scaleMatrix;
        private bool dirtyMatrix = true;
        private Color borderColor;

        public ResolutionRenderer(GraphicsDeviceManager device, Color borderColor, Vector2 virtualResolution)
        {
            screenWidth = device.PreferredBackBufferWidth;
            screenHeight = device.PreferredBackBufferHeight;
            virtualWidth = (int)virtualResolution.X;
            virtualHeight = (int)virtualResolution.Y;
            gdm = device;
            dirtyMatrix = true;
            this.borderColor = borderColor;
        }


        public Matrix getTransformationMatrix()
        {
            if (dirtyMatrix)
                recreateScaleMatrix();

            return scaleMatrix;
        }

        public void beginDraw()
        {
            // Start by reseting viewport to (0,0,1,1)
            fullViewport();

            // Clear to Black
            gdm.GraphicsDevice.Clear(borderColor);

            // Calculate Proper Viewport according to Aspect Ratio
            resetViewport();
        }

        //Scales the resolution matrix as the aspect ratio changes
        private void recreateScaleMatrix()
        {
            dirtyMatrix = false;
            scaleMatrix = Matrix.CreateScale((float)gdm.GraphicsDevice.Viewport.Width / virtualWidth, (float)gdm.GraphicsDevice.Viewport.Width / virtualWidth, 1f);
        }

        //Reset viewport before calculation
        public void fullViewport()
        {
            Viewport vp = new Viewport();
            vp.X = vp.Y = 0;
            vp.Width = screenWidth;
            vp.Height = screenHeight;
            gdm.GraphicsDevice.Viewport = vp;
        }

        public float getVirtualAspectRatio()
        {
            return virtualWidth / (float)virtualHeight;
        }

        public void resetViewport()
        {
            float targetAspectRatio = getVirtualAspectRatio();

            //Figure out the largest area that fits in this resolution at the desired aspect ratio
            int width = gdm.PreferredBackBufferWidth;
            int height = (int)(width / targetAspectRatio + .5f);

            if (height > gdm.PreferredBackBufferHeight)
            {
                height = gdm.PreferredBackBufferHeight;

                // PillarBox
                width = (int)(height * targetAspectRatio + .5f);
            }

            //Set up the new viewport centered in the backbuffer
            Viewport viewport = new Viewport();

            viewport.X = (gdm.PreferredBackBufferWidth / 2) - (width / 2);
            viewport.Y = (gdm.PreferredBackBufferHeight / 2) - (height / 2);
            viewport.Width = width;
            viewport.Height = height;

            dirtyMatrix = true;
            gdm.GraphicsDevice.Viewport = viewport;

            //Convert mouse position
            Vector2 rawMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            Vector2 translate = new Vector2(rawMousePos.X - viewport.X, rawMousePos.Y - viewport.Y);
            Vector2 virtualMousePos = Vector2.Transform(translate, Matrix.Invert(scaleMatrix));

            //Only update virtual mouse position if mouse is within game viewport
            if (virtualMousePos.X < virtualWidth && virtualMousePos.X >= 0 && virtualMousePos.Y < virtualHeight && virtualMousePos.Y >= 0)
                CrypticGame.mousePos = virtualMousePos;
        }

        public void setBorderColor(Color color)
        {
            borderColor = color;
        }
    }
}

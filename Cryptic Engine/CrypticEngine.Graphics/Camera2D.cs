using Microsoft.Xna.Framework;
using System;

namespace CrypticEngine.Graphics
{
    public sealed class Camera2D
    {
        private float zoom;
        private float worldZoom;
        private float rotation;
        private float step;
        private Vector2 position;
        private Matrix transform = Matrix.Identity;
        private bool viewTransformationDirty = true;
        private Matrix camTranslationMatrix = Matrix.Identity;
        private Matrix camRotationMatrix = Matrix.Identity;
        private Matrix camScaleMatrix = Matrix.Identity;
        private Matrix resTranslationMatrix = Matrix.Identity;

        private ResolutionRenderer resolutionRenderer;
        private Vector3 camTranslationVector = Vector3.Zero;
        private Vector3 camScaleVector = Vector3.Zero;
        private Vector3 resTranslationVector = Vector3.Zero;

        public Camera2D(ResolutionRenderer resolutionRenderer)
        {
            this.resolutionRenderer = resolutionRenderer;

            zoom = 1f;
            worldZoom = 1f;
            rotation = 0.0f;
            position = Vector2.Zero;
        }

        public void setPosition(Vector2 pos)
        {
            position = pos;
            viewTransformationDirty = true;
        }

        public Vector2 getPosition()
        {
            return position; 
        }

        public void move(Vector2 amount)
        {
            position += amount;
            viewTransformationDirty = true;
        }

        /// <summary> Sets the matrix zoom that scales all draws. Zoom has a value of 15f by default and 1f is the minimum value. Note: Zooming beyond 15f can cause problems with the matrix scaling formula. Consider using Camera2D.setWorldZoom() instead </summary>
        public void setMatrixZoom(float zoom)
        {
            this.zoom = zoom;

            if (this.zoom < 1f)
                this.zoom = 1f;

            viewTransformationDirty = true;
        }

        /// <summary> Sets the world zoom that scales the non-UI sprites in the world. World zoom has a value of 1f by default and 0.01f is the minimum value </summary>
        public void setWorldZoom(float zoom)
        {
            worldZoom = zoom;

            if (worldZoom < 0.01f)
                worldZoom = 0.01f;
        }

       /// <summary> Returns the matrix zoom that scales all draws </summary>
        public float getMatrixZoom()
        {
            return zoom;
        }

        /// <summary> Returns the world zoom that scales the non-UI sprites in the world </summary>
        public float getWorldZoom()
        {
            return worldZoom;
        }

        /// <summary> Sets the camera movement step. Cannot be less than 0.1f </summary>
        public void setStep(float step)
        {
            if (step < 0.1f)
            {
                Console.WriteLine("[Cryptic Engine] WARNING: Camera step cannot be less than 0.1f");
                step = 0.1f;
            }

            this.step = step;
        }

        public float getStep()
        {
            return step;
        }

        public void setRotation(float rot)
        {
            rotation = rot;
            viewTransformationDirty = true;
        }

        public float getRotation()
        {
            return rotation;
        }

        /// <summary> Returns the camera matrix after scaling, translating, rotating, and applying the resolution change </summary>
        public Matrix getViewTransformationMatrix()
        {
            if (viewTransformationDirty)
            {
                camTranslationVector.X = -position.X;
                camTranslationVector.Y = -position.Y;

                Matrix.CreateTranslation(ref camTranslationVector, out camTranslationMatrix);
                Matrix.CreateRotationZ(rotation, out camRotationMatrix);

                camScaleVector.X = zoom;
                camScaleVector.Y = zoom;
                camScaleVector.Z = 1;

                Matrix.CreateScale(ref camScaleVector, out camScaleMatrix);

                resTranslationVector.X = resolutionRenderer.virtualWidth * 0.5f;
                resTranslationVector.Y = resolutionRenderer.virtualHeight * 0.5f;
                resTranslationVector.Z = 0;

                Matrix.CreateTranslation(ref resTranslationVector, out resTranslationMatrix);

                transform = camTranslationMatrix * camRotationMatrix * camScaleMatrix * resTranslationMatrix * resolutionRenderer.getTransformationMatrix();

                viewTransformationDirty = false;
            }

            return transform;
        }

        /// <summary> Recreates the camera matrix with the new screen width and height. Call after window change to dynamically adjust aspect ratio </summary>
        public void recalculateTransformationMatrices(int screenWidth, int screenHeight)
        {
            resolutionRenderer.screenWidth = screenWidth;
            resolutionRenderer.screenHeight = screenHeight;
            viewTransformationDirty = true;
        }

        /// <summary> Returns the position relative to the camera. (0, 0) is the upper left of the camera bounds. </summary>
        public Vector2 screenSpace(float x,  float y)
        {
            return new Vector2(position.X - resolutionRenderer.virtualWidth / 2 / zoom + x, position.Y - resolutionRenderer.virtualHeight / 2 / zoom  + y);
        }
    }
}

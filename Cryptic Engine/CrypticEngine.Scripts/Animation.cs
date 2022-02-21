using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CrypticEngine.Scripts
{
    public class Animation : CrypticScript
    {
        public string spriteSource;
        public Vector2 frameSize;
        public Vector2 sheetOrigin;
        public uint frameCount;
        public uint fps;
        protected bool playing;
        protected Texture2D spriteSheet;
        protected List<Rectangle> frames;
        protected int currentFrame;
        protected float timeUntilNextFrame;    

        public enum FetchDirection
        {
            LeftToRight,
            RightToLeft
        }

        public Animation()
        {
            frames = new List<Rectangle>();
            frameSize = Vector2.Zero;
            sheetOrigin = Vector2.Zero;
            frameCount = 1;
            spriteSource = "";
            fps = 15;
        }

        public override void start(CrypticGame game)
        {        
            spriteSheet = game.getContent().Load<Texture2D>(spriteSource);

            //Fetch the animation frames
            if (frameSize.X > 0 && frameSize.Y > 0)
                fetchFrames(new Rectangle((int)sheetOrigin.X, (int)sheetOrigin.Y, (int)frameSize.X, (int)frameSize.Y), frameCount);
        }

        public override void update(CrypticGame game, GameTime gameTime)
        {
            if (playing && frames != null && frames.Count > 0 && fps != 0)
            {
                float frameStep = 1f / fps; //Duration of animation frame
                float gameFrameTime = (float)gameTime.ElapsedGameTime.TotalSeconds; //Elapsed time of current game frame

                timeUntilNextFrame -= gameFrameTime;

                if (timeUntilNextFrame <= 0)
                {
                    currentFrame++;
                    timeUntilNextFrame += frameStep;

                    if (currentFrame >= frames.Count)
                        currentFrame = 0;
                }
            }
        }

        public override void draw(CrypticGame game, GameTime gameTime) {}

        /// <summary> Adds a new frame to the animation </summary>
        public void addFrame(Rectangle frame)
        {
            frames.Add(frame);
        }

        /// <summary> Adds a series of frames to the animation spliced as a grid. The fetch direction is the direction to read the frames </summary>
        public void fetchFrames(Rectangle grid, uint frameCount = 1, FetchDirection dir = FetchDirection.LeftToRight)
        {
            int totalAdded = 0;

            for (int y = 0; y < (spriteSheet.Height - grid.Y) / grid.Height; y++)
            {
                for (int x = 0; x < (spriteSheet.Width - grid.X) / grid.Width; x++)
                {
                    if(dir == FetchDirection.LeftToRight)
                        frames.Add(new Rectangle(grid.X + (x * grid.Width), grid.Y + (y * grid.Y), grid.Width, grid.Height));
                    else
                        frames.Add(new Rectangle(grid.X - (x * grid.Width), grid.Y + (y * grid.Y), grid.Width, grid.Height));

                    totalAdded++;

                    if (totalAdded == frameCount)
                        return;
                }
            }
        }

        /// <summary> Starts the animation </summary>
        public void play()
        {
            playing = true;
            timeUntilNextFrame = 1f / fps;
        }

        /// <summary> Stops the animation </summary>
        public void stop()
        {
            playing = false;
        }

        /// <summary> Returns the current frame of the animation in its rectangle representation </summary>
        public Rectangle getCurrentRect()
        {
            return frames[currentFrame];
        }

        public bool getPlaying()
        {
            return playing;
        }

        /// <summary> Returns the sprite sheet for this animation </summary>
        public Texture2D getSpriteSheet()
        {
            return spriteSheet;
        }
    }
}

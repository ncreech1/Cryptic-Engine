using CrypticEngine.Graphics;
using CrypticEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace CrypticEngine
{
    public abstract class CrypticGame : Game
    {
        /// <summary> The virtual position of the mouse, scaled with the aspect ratio </summary>
        public static Vector2 mousePos;

        /// <summary> The position of the mouse in world coordinates (SpriteBatch draw pixels) </summary>
        public static Vector2 mouseWorldPos;

        /// <summary> The unaveraged frames per second of the game </summary>
        public static int fps;

        /// <summary> The path to the scene (.txt) files </summary>
        public static string scenePath = "Content/Scenes/";

        protected CrypticDraw crypticDraw;
        protected SpriteBatch spriteBatch;
        protected GraphicsDeviceManager graphics;
        protected Camera2D camera;
        protected string scriptNamespace;
        protected string scriptAssembly;
        private CrypticScene currentScene;
        private Dictionary<string, CrypticScene> scenes;
        private ResolutionRenderer resolutionRenderer;
        private Texture2D viewportRect;
        private int windowedWidth, windowedHeight;
        private Vector2 windowedPos;
        private bool editMode;

        /// <summary> Prints an error message to the console </summary>
        public static void printError(string error)
        {
            Console.WriteLine($"[Cryptic Engine] Error: {error}");
        }

        /// <summary> Prints a warning message to the console </summary>
        public static void printWarning(string warning)
        {
            Console.WriteLine($"[Cryptic Engine] Warning: {warning}");
        }

        /// <summary> Prints a message to the console </summary>
        public static void printMsg(string msg)
        {
            Console.WriteLine($"[Cryptic Engine] {msg}");
        }

        /// <summary> Initializes Cryptic Engine. </summary>
        protected abstract void initialize();

        /// <summary> Loads content from the local content pipeline. Load the first scene or the scene to edit here </summary>
        protected abstract void loadContent();

        /// <summary> Main update loop for Cryptic Engine </summary>
        protected abstract void update(GameTime gameTime);

        /// <summary> Main draw loop for Cryptic Engine </summary>
        protected abstract void draw(GameTime gameTime);

        /// <summary> Initializes the default key bindings for the game </summary>
        protected virtual void initializeDefaultKeyBindings() { }

        /// <summary> Called when Monogame exits </summary>
        protected virtual void onExit() { }

        public CrypticGame(float virtualResolutionX, float virtualResolutionY, string scenePath = "Content/Scenes")
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            CrypticGame.scenePath = scenePath;
            scriptAssembly = "";
            scriptNamespace = "";

            scenes = new Dictionary<string, CrypticScene>();

            //Create directory for reading scene files
            if (!Directory.Exists(scenePath))
                Directory.CreateDirectory(scenePath);

            //Load all scenes in the given scene directory
            string[] sceneFiles = Directory.GetFiles(scenePath, "*.txt");

            //Create CrypticScene objects from the scene files
            foreach (string file in sceneFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string subPath = Path.GetDirectoryName(file).Remove(0, scenePath.Length);
                CrypticScene readScene = new CrypticScene(fileName, subPath);
                scenes.Add(readScene.getName(), readScene);
            }

            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            windowedWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2;
            windowedHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2;
            windowedPos = new Vector2(0, 0);
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();
            Window.AllowUserResizing = true;

            IsMouseVisible = true;
            mousePos = new Vector2(0, 0);
            mouseWorldPos = new Vector2(0, 0);
            editMode = true;

            //Add default editor keys
            KeyboardExtension.addKeyBinding("Camera Up", Keys.W);
            KeyboardExtension.addKeyBinding("Camera Down", Keys.S);
            KeyboardExtension.addKeyBinding("Camera Left", Keys.A);
            KeyboardExtension.addKeyBinding("Camera Right", Keys.D);
            KeyboardExtension.addKeyBinding("Pause/Play", Keys.P);
            KeyboardExtension.addKeyBinding("Exit", Keys.Escape);
            KeyboardExtension.addKeyBinding("Fullscreen", Keys.F11);

            //Setup resolution indepence renderer and camera
            initializeResolutionIndependence(new Vector2(0, 0), Color.Black, new Vector2(virtualResolutionX, virtualResolutionY));

            //Initialize viewport rect
            viewportRect = new Texture2D(graphics.GraphicsDevice, 1, 1);
            viewportRect.SetData(new Color[] { Color.CornflowerBlue });
        }

        /// <summary> Monogame's initialize function </summary>
        protected sealed override void Initialize()
        {
            base.Initialize();

            initializeDefaultKeyBindings();
            initialize();
        }

        /// <summary> Monogame's load function </summary>
        protected sealed override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            crypticDraw = new CrypticDraw(spriteBatch, camera);

            //Attempt to load configuration file
            if (!ConfigReader.loadConfig(this))
                ConfigReader.loadDefaultConfig(this);

            //Config file loaded successfully but field "last_scene" was not found or was invalid
            if (currentScene == null)
            {
                CrypticScene defaultScene = new CrypticScene("Default");
                addScene(defaultScene);
                openScene(defaultScene);
            }

            loadContent();
        }

        /// <summary> Monogame's unload function </summary>
        protected sealed override void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary> Monogame's draw function </summary>
        protected sealed override void Draw(GameTime gameTime)
        {
            try
            {
                recalculateAspectRatio();
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.getViewTransformationMatrix());
                drawViewportRect();
                spriteBatch.End();

                draw(gameTime);
                currentScene.draw(this, gameTime);
                base.Draw(gameTime);
            }

            catch (System.Exception e)
            {
                Console.WriteLine("[Cryptic Engine] Fatal error occurred while drawing to the screen: " + e.Message);
                Exit();
            }
        }

        /// <summary> Monogame's update function </summary>
        protected sealed override void Update(GameTime gameTime)
        {
            //Update the current scene
            if (!editMode)
            {
                update(gameTime);
                currentScene.update(this, gameTime);

                if (KeyboardExtension.getKeyDown("Pause/Play"))
                    editMode = true;
            }

            else
            {
                //Camera movement
                if (KeyboardExtension.getKeyHeld("Camera Left"))
                    camera.move(new Vector2(-camera.getStep(), 0));
                if (KeyboardExtension.getKeyHeld("Camera Right"))
                    camera.move(new Vector2(camera.getStep(), 0));
                if (KeyboardExtension.getKeyHeld("Camera Up"))
                    camera.move(new Vector2(0, -camera.getStep()));
                if (KeyboardExtension.getKeyHeld("Camera Down"))
                    camera.move(new Vector2(0, camera.getStep()));
                if (KeyboardExtension.getKeyHeld("Camera Zoom In"))
                    camera.setWorldZoom(camera.getWorldZoom() + 0.01f);
                else if (KeyboardExtension.getKeyHeld("Camera Zoom Out"))
                    camera.setWorldZoom(camera.getWorldZoom() - 0.01f);
                if (KeyboardExtension.getKeyDown("Pause/Play") && currentScene != null)
                    editMode = false;
            }

            //Save last windowed size and position to return to when fullscreen is toggled off
            if (windowedWidth != graphics.PreferredBackBufferWidth || windowedHeight != graphics.PreferredBackBufferHeight || windowedPos != Window.Position.ToVector2())
            {
                if (!graphics.IsFullScreen)
                {
                    windowedPos = Window.Position.ToVector2();
                    windowedWidth = graphics.PreferredBackBufferWidth;
                    windowedHeight = graphics.PreferredBackBufferHeight;
                }
            }

            if (KeyboardExtension.getKeyDown("Exit"))
                Exit();

            //Enable/disable fullscreen
            if (KeyboardExtension.getKeyDown("Fullscreen"))
                toggleFullscreen();

            //Calculate world mouse position from screen position and viewport
            resolveWorldMousePos();

            //Calculate frames per second (not averaged)
            fps = (int)Math.Round(1 / (float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        /// <summary> Monogame's exit function </summary>
        protected sealed override void OnExiting(object sender, EventArgs args)
        {
            //Update config file
            File.WriteAllText(scenePath + "/../ce_config.txt", $"# The Cryptic Engine Configuration File{Environment.NewLine}{Environment.NewLine}last_scene: \"{currentScene.getName()}.txt\"");
            onExit();

            base.OnExiting(sender, args);
        }

        /// <summary> Returns the game camera </summary>
        public Camera2D getCamera()
        {
            return camera;
        }

        /// <summary> Sets the color of the letterboxing and pillarboxing </summary>
        public void setBorderColor(Color color)
        {
            resolutionRenderer.setBorderColor(color);
        }

        /// <summary> Sets the color of the game viewport </summary>
        public void setViewportColor(Color color)
        {
            viewportRect.SetData(new Color[] { color });
        }

        /// <summary> Toggles full screen resolution </summary>
        protected void toggleFullscreen()
        {
            if (!graphics.IsFullScreen)
            {
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }

            else
            {
                graphics.PreferredBackBufferWidth = windowedWidth;
                graphics.PreferredBackBufferHeight = windowedHeight;
                Window.Position = windowedPos.ToPoint();
            }

            graphics.IsFullScreen = !graphics.IsFullScreen;
            graphics.ApplyChanges();
        }     

        /// <summary> Loads the given scene from the scene list </summary>
        public bool openScene(string name)
        {
            CrypticScene s = getScene(name);

            if (s != null)
            {
                //Attempt to load the scene from its scene file
                if(!SceneReader.loadScene(this, s))
                    return false;

                currentScene = s;
                s.start(this);
                return true;
            }

            return false;
        }

        /// <summary> Loads the given scene </summary>
        public bool openScene(CrypticScene scene)
        {
            return openScene(scene.getName());
        }

        /// <summary> Adds a scene to the scene list </summary>
        public void addScene(CrypticScene scene)
        {
            if(getScene(scene.getName()) == null)
                scenes.Add(scene.getName(), scene);
        }

        /// <summary> Finds the scene in the scene list from its name. Accepts name with ".txt" extension as well </summary>
        public CrypticScene getScene(string name)
        {
            CrypticScene s;

            if (name.EndsWith(".txt"))
                name = Path.GetFileNameWithoutExtension(name);

            scenes.TryGetValue(name, out s);

            return s;
        }

        public CrypticScene getCurrentScene()
        {
            return currentScene;
        }

        /// <summary> Returns the ContentManager instance for the game </summary>
        public ContentManager getContent()
        {
            return Content;
        }

        /// <summary> Returns the CrypticDraw instance for the game </summary>
        public CrypticDraw getDraw()
        {
            return crypticDraw;
        }

        /// <summary> Returns the SpriteBatch instance for the game </summary>
        public SpriteBatch getSpriteBatch()
        {
            return spriteBatch;
        }

        /// <summary> Returns the user-defined namespace where scripts are stored </summary>
        public string getScriptNamespace()
        {
            return scriptNamespace;
        }

        /// <summary> Returns the user-defined assembly where scripts are stored </summary>
        public string getScriptAssembly()
        {
            return scriptAssembly;
        }

        /// <summary> Switches the Cryptic Engine to edit mode </summary>
        private void play()
        {
            editMode = false;
        }

        /// <summary> Switches the Cryptic Engine to play mode </summary>
        private void pause()
        {
            editMode = true;
        }

        /// <summary> Converts the virtual mouse position to the mouse location in world space </summary>
        private void resolveWorldMousePos()
        {
            mouseWorldPos = new Vector2((camera.getPosition().X + (mousePos.X - resolutionRenderer.virtualWidth / 2) / camera.getMatrixZoom()) / camera.getWorldZoom(), (camera.getPosition().Y + (mousePos.Y - resolutionRenderer.virtualHeight / 2) / camera.getMatrixZoom()) / camera.getWorldZoom());
            mouseWorldPos = new Vector2((int)mouseWorldPos.X, (int)mouseWorldPos.Y);
        }

        ///<summary> Sets up the camera and resolution indepence system. The camera's zoom and initial position are defined here </summary>
        private void initializeResolutionIndependence(Vector2 cameraPos, Color borderColor, Vector2 virtualResolution, float matrixZoom = 15f)
        {
            resolutionRenderer = new ResolutionRenderer(graphics, borderColor, virtualResolution);
            camera = new Camera2D(resolutionRenderer);

            camera.setPosition(cameraPos);
            camera.setMatrixZoom(matrixZoom);
            camera.setStep(2);

            camera.recalculateTransformationMatrices(resolutionRenderer.screenWidth, resolutionRenderer.screenHeight);
        }

        /// <summary> Updates the camera and resolution matrices to account for window changes. Call in the game draw loop before the first SpriteBatch.Begin() call  </summary>
        private void recalculateAspectRatio()
        {
            //Update resolution on window change
            if (resolutionRenderer.screenWidth != graphics.PreferredBackBufferWidth || resolutionRenderer.screenHeight != graphics.PreferredBackBufferHeight)
                camera.recalculateTransformationMatrices(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            //Create resolution matrix
            resolutionRenderer.beginDraw();
        }

        /// <summary> Creates and updates the viewport background rectangle to account for window changes. Call in the game draw loop after the first SpriteBatch.Begin() call  </summary>
        private void drawViewportRect()
        {
            spriteBatch.Draw(viewportRect, new Rectangle((int)camera.screenSpace(0, 0).X, (int)camera.screenSpace(0, 0).Y, (int)(resolutionRenderer.virtualWidth / camera.getMatrixZoom()), (int)(resolutionRenderer.virtualHeight / camera.getMatrixZoom())), Color.White);
        }
    }
}

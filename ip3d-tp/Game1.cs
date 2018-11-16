using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ip3d_tp
{

    public class Game1 : Game
    {
        // flag indicating if the mouse is captured or not 
        bool captureMouse = true;

        // flag to indicate if help should be displayed
        public bool showHelp = true;

        // an utility to measure framerates
        FrameRate FrameRate = new FrameRate();
        
        // the graphics device
        GraphicsDeviceManager graphics;
        
        // I'm keeping the spritebatch because there will
        // be a gui string showing the controls
        SpriteBatch spriteBatch;

        // this will be the font we will use for text rendering
        SpriteFont font;

        /*
         * world objects
         */ 

        // the world axis render object, to be a reference in space
        Axis3D worldAxis;

        // the plane that will be the terrain
        Plane plane;

        // texture to be applied to the terrain
        Texture2D terrainHeightMap;
        
        // the tank
        Tank tank1;
        
        // the second tank
        Tank tank2;

        /*
         * cameras
         */

        // holds the current active camera
        Camera currentCamera;
         
        // the cameras that will be attached to the first tank
        ThirdPersonCamera ThirdPersonCamera1;

        // the camera that will be attached to the second tank
        ThirdPersonCamera ThirdPersonCamera2;
        
        // acts as a free camera, meaning it is possible to fly around
        FreeCamera freeCamera;

        // follows the surface, at an offset from the ground
        SurfaceFollowCamera surfaceFollowCamera;

        /*
         * lights
         */

        // direction light properties
        // indicates the direction that the infinity light comes from
        public Vector4 LightDirection = new Vector4(10, 5, 0, 0);

        // the color of the light
        public Vector4 LightColor = Color.White.ToVector4();

        // the intensity of said light
        public float LightIntensity = 1.0f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // prepare for anti aliasing
            // reference: http://community.monogame.net/t/solved-anti-aliasing/10561
            // state that we will use a HiDef profile, check here the difs:
            // https://blogs.msdn.microsoft.com/shawnhar/2010/03/12/reach-vs-hidef/

            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            //graphics.IsFullScreen = true;

            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
            graphics.ApplyChanges();
        }

        // callback for preparing device settings, see link above for more info
        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            graphics.PreferMultiSampling = true;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;  // samples count
        }

        protected override void Initialize()
        {
            Window.Title = $"EP3D-TP - JORGE NORO - 15705 {graphics.GraphicsProfile}, Sampling: {graphics.PreferMultiSampling}, Samples: {GraphicsDevice.PresentationParameters.MultiSampleCount}";

            // set mouse cursor state
            IsMouseVisible = IsMouseVisible;
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load font for debugging
            font = Content.Load<SpriteFont>("font");

            // load a texture to modify the surface vertices
            terrainHeightMap = Content.Load<Texture2D>("lh3d1");

            // initialize the axis and add it to the components manager
            worldAxis = new Axis3D(this, Vector3.Zero, 200f);
            Components.Add(worldAxis);

            // initialize the plane with the prefered settings
            plane = new Plane(this, "ground_texture", 128*2, 128*2, terrainHeightMap.Width - 1, terrainHeightMap.Height - 1, 0.5f);
            Components.Add(plane);

            // dispace the vertices of the plane, based on the given heightmap, and adjust by a scale
            plane.SetHeightFromTexture(terrainHeightMap, 0.08f);
            
            // create the tanks 
            tank1 = new Tank(this);
            tank1.Position.X = 4f;  // offset a bit so the two don't overlap

            tank2 = new Tank(this);
            tank2.Position.X = -4f;
            tank2.TankID = 1;  // identify this tank as ID 1, used for the controls

            /*
             * cameras
             * 
             */ 

            // create the various cameras
            ThirdPersonCamera1 = new ThirdPersonCamera(this, tank1, plane, new Vector3(0, 15f, -15f));  // see definition for an understanding

            ThirdPersonCamera2 = new ThirdPersonCamera(this, tank2, plane, new Vector3(0, 15f, -15f));
            
            freeCamera = new FreeCamera(this, 45f);
            freeCamera.Position.Y = 10;
            freeCamera.Position.Z = 10;

            surfaceFollowCamera = new SurfaceFollowCamera(this, 45f, plane, 1.76f); // my height 
            surfaceFollowCamera.MaxVelocity = 2.0f;
            surfaceFollowCamera.Acceleration = new Vector3(0.1f);
            
            // set the default camera
            currentCamera = ThirdPersonCamera1;

            // init controls
            Controls.Initilalize();

        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Controls.IsKeyDown(Keys.Escape))
                Exit();

            #region UtilsUpdate
            
            // frame delta value, for calculations based on time
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // update the framerate utility
            FrameRate.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            // set the current keyboard state
            Controls.UpdateCurrentStates();

            #endregion

            #region UserInput

            // switch between cameras
            if (Controls.IsKeyPressed(Keys.F1))
            {
                currentCamera = ThirdPersonCamera1;

            } else if(Controls.IsKeyPressed(Keys.F2))
            {
                currentCamera = ThirdPersonCamera2;

            } else if (Controls.IsKeyPressed(Keys.F3))
            {
                currentCamera = surfaceFollowCamera;

            } else if (Controls.IsKeyPressed(Keys.F4))
            {
                currentCamera = freeCamera;

            }
            
            // toggle wireframe
            if (Controls.IsKeyPressed(Keys.F) && showHelp)
            {
                plane.ShowWireframe = !plane.ShowWireframe;
            }

            // toggle normals
            if (Controls.IsKeyPressed(Keys.N) && showHelp)
            {
                plane.ShowNormals = !plane.ShowNormals;
            }

            // toogle mouse capture
            if(Controls.IsKeyPressed(Keys.M))
            {
                IsMouseVisible = captureMouse;
                captureMouse = !captureMouse;
            }

            // toogle help
            if (Controls.IsKeyPressed(Keys.H))
            {
                showHelp = !showHelp;
            }

            #endregion

            // locking the mouse only after the components are updated
            if (captureMouse)
            {
                Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
            }

            // every component will be updated after base update
            base.Update(gameTime);

            // updates the state of the current camera
            currentCamera.Update(gameTime);

            // we update the tanks manually so we can more flexibility
            // they are not part of the component system.
            // Note: all this internal component system is a bit week.
            //       I hope in the future of this project to develop our own
            //       node based object structure. Let's see how that goes. If I have the time.
            tank1.Update(gameTime, currentCamera, plane);
            tank2.Update(gameTime, currentCamera, plane);
            
            // here we update the object shader(effect) matrices
            // so it can perform the space calculations on the vertices
            plane.UpdateShaderMatrices(currentCamera.ViewTransform, currentCamera.ProjectionTransform);
            worldAxis.UpdateShaderMatrices(currentCamera.ViewTransform, currentCamera.ProjectionTransform);
            
            // update the last keyboard state
            Controls.UpdateLastStates();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.20f, 0.20f, 0.20f));

            // we need to call the draw manually for the plane
            // it extends component, and not drawable
            plane.DrawCustomShader(gameTime, currentCamera, LightDirection, LightColor, LightIntensity);

            // draw the tanks with the light created
            tank1.Draw(gameTime, currentCamera, LightDirection, LightColor, LightIntensity);
            tank2.Draw(gameTime, currentCamera, LightDirection, LightColor, LightIntensity);

            // render the gui text
            // notive the DepthStencilState, without default set in, depth will not 
            // be calculated when drawing wireframes.
            // more investigation needs to be done in order to understand why Monogame
            // is doing it this way.
            //
            // update 18/10/2018:
            // so we know that the graphics card is a state machine. so when we dont specify the stencil or sampler states
            // the spritebatch will use some defaults of his. because in the plane class we never change them back,
            // we get strange results. so, for now, the solution is to just maintain the same ones in the spritebatch.
            // in the future, we must create stencil and sampler states for the meshs render.
            //
            // update 22/10/2018
            // the blendstate messes up with custom shader.
            // render targets might be the solution
            //
            // update 16/11/2018
            // no. just changing the blend state is the solution.
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, DepthStencilState.Default, null, null, null);

            if(showHelp)
            {

                spriteBatch.DrawString(font, $"{Math.Round(FrameRate.AverageFramesPerSecond)}", new Vector2(10f, 10f), new Color(0f, 1f, 0f));
                spriteBatch.DrawString(font, $"Help (H, Toogle): {showHelp}\nWireframe (F, Toogle): {plane.ShowWireframe}\nNormals (N, Toogle): {plane.ShowNormals}", new Vector2(10f, 26f), new Color(0f, 1f, 0f));
                spriteBatch.DrawString(font, tank1.GetDebugInfo(), new Vector2(10f, graphics.PreferredBackBufferHeight - 5 * 26f), new Color(0f, 1f, 0f));
                spriteBatch.DrawString(font, $"Cycle between cameras in F1-F12\nAbout the camera:\n{currentCamera.About()}", new Vector2(graphics.PreferredBackBufferWidth / 2, 10f), new Color(0f, 1f, 0f));

            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

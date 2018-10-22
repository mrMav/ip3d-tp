using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ip3d_tp
{

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;

        // I'm keeping the spritebatch because there will
        // be a gui string showing the controls
        SpriteBatch spriteBatch;

        // this will be the font we will use for text rendering
        SpriteFont font;

        // the world axis render object, to be a reference in space
        Axis3D worldAxis;

        // the plane that will be the terrain
        Plane plane;

        // texture to be applied to the terrain
        Texture2D terrainHeightMap;

        // cameras:
        // holds the current active camera
        Camera currentCamera;

        // basic camera rotates around the center of the landscape
        BasicCamera basicCamera;
        
        // acts as a free camera, meaning it is possible to fly around
        FreeCamera freeCamera;

        // follows the surface, at an offset from the ground
        SurfaceFollowCamera surfaceFollowCamera;

        // put the cameras in an array for easy cycling
        Camera[] camerasArray;
        int currCam;

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
            IsMouseVisible = false;

            // ensure that the culling is happening for counterclockwise polygons
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.RasterizerState = rasterizerState;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load font
            font = Content.Load<SpriteFont>("font");

            // laod texture
            terrainHeightMap = Content.Load<Texture2D>("lh3d1");

            // initialize the axis and add it to the componetens manager
            worldAxis = new Axis3D(this, Vector3.Zero, 200f);
            Components.Add(worldAxis);

            // initialize the plane with the prefered settings
            plane = new Plane(this, "ground_texture", 128*2, 128*2, terrainHeightMap.Width - 1, terrainHeightMap.Height - 1, 0.5f);
            //plane = new Plane(this, "grey", 20, 30, 4, 10);
            Components.Add(plane);

            // dispace the vertices of the plane, based on the given heightmap, and adjust by a scale
            plane.SetHeightFromTexture(terrainHeightMap, 0.09f);

            // toogle wireframe out of the box
            plane.ShowWireframe = true;

            // create the various cameras
            basicCamera = new BasicCamera(this, 45f, 128);

            freeCamera = new FreeCamera(this, 45f);
            freeCamera.Position.Y = 10;
            freeCamera.Position.Z = 10;

            surfaceFollowCamera = new SurfaceFollowCamera(this, 45f, plane, 1.76f); // my height 
            surfaceFollowCamera.MaxVelocity = 2.0f;
            surfaceFollowCamera.Acceleration = new Vector3(0.1f);

            // initialize the array of cameras
            camerasArray = new Camera[3];
            camerasArray[0] = surfaceFollowCamera;
            camerasArray[1] = freeCamera;
            camerasArray[2] = basicCamera;
            currCam = 1;

            // set the default camera
            currentCamera = camerasArray[currCam];

            // init controls
            Controls.Initilalize();

        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            // set the current keyboard state
            Controls.UpdateCurrentStates();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Controls.IsKeyDown(Keys.Escape))
                Exit();
            
            // switch between cameras
            if(Controls.IsKeyPressed(Keys.Space))
            {
                currCam = (currCam + 1) % 3;

                Console.WriteLine(currCam);

                currentCamera = camerasArray[currCam];
            }

            // toggle wireframe
            if (Controls.IsKeyPressed(Keys.F))
            {
                plane.ShowWireframe = !plane.ShowWireframe;
            }

            currentCamera.Update(gameTime);

            // every component will be updated after base update
            base.Update(gameTime);
            
            // locking the mouse only after the components are updated
            Mouse.SetPosition(Window.Position.X + (graphics.PreferredBackBufferWidth / 2), Window.Position.Y + (graphics.PreferredBackBufferHeight / 2));
            
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
            plane.DrawCustomShader(gameTime, currentCamera);
            
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
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.Default, null, null, null);
            spriteBatch.DrawString(font, $"Camera (SPACE, Cycle): {camerasArray[currCam].GetType().Name}\nWireframe (F, Toogle): {plane.ShowWireframe}", new Vector2(10f, 10f), new Color(0f, 1f, 0f));
            spriteBatch.DrawString(font, $"{camerasArray[currCam].About()}", new Vector2(graphics.PreferredBackBufferWidth / 2, 10f), new Color(0f, 1f, 0f));
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

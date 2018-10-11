using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ip3d_tp
{

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Axis3D worldAxis;
        Plane plane;
        FreeCamera camera;
        Texture2D terrainHeightMap;


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
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
        }

        protected override void Initialize()
        {
            Window.Title = $"EP3D-TP - JORGE NORO - 15705 {graphics.GraphicsProfile}, Sampling: {graphics.PreferMultiSampling}, Samples: {GraphicsDevice.PresentationParameters.MultiSampleCount}";
            IsMouseVisible = false;
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            terrainHeightMap = Content.Load<Texture2D>("lh3d1");

            worldAxis = new Axis3D(this, camera, Vector3.Zero, 200f);
            Components.Add(worldAxis);

            plane = new Plane(this, 128, 128, terrainHeightMap.Width - 1, terrainHeightMap.Height - 1);
            plane.SetHeightFromTexture(terrainHeightMap);
            plane.ShowWireframe = true;
            Components.Add(plane);

            //camera = new SurfaceFollowCamera(this, 45f, plane);
            camera = new FreeCamera(this, 45f);
            camera.Position.Y = 20;
            Components.Add(camera);

        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // every component will be updated after base update
            base.Update(gameTime);

            // locking the mouse
            Mouse.SetPosition(Window.Position.X + (graphics.PreferredBackBufferWidth / 2), Window.Position.Y + (graphics.PreferredBackBufferHeight / 2));

            // here we update the object shader(effect) matrices
            // so it can perform the space calculations on the vertices
            plane.UpdateShaderMatrices(camera.ViewTransform, camera.ProjectionTransform);
            worldAxis.UpdateShaderMatrices(camera.ViewTransform, camera.ProjectionTransform);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.20f, 0.20f, 0.20f));

            plane.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}

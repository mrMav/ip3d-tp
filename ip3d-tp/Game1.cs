using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ip3d_tp
{

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Plane plane;
        BasicCamera camera;
        Texture2D terrainHeightMap;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // prepare for anti aliasing
            // reference: http://community.monogame.net/t/solved-anti-aliasing/10561
            // state that we will use a HiDef profile, check here the difs:
            // https://blogs.msdn.microsoft.com/shawnhar/2010/03/12/reach-vs-hidef/
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
            IsMouseVisible = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            terrainHeightMap = Content.Load<Texture2D>("lh3d1");

            plane = new Plane(this, 100, 100, terrainHeightMap.Width - 1, terrainHeightMap.Height - 1);
            plane.SetHeightFromTexture(terrainHeightMap);
            plane.ShowWireframe = true;
            Components.Add(plane);

            camera = new BasicCamera(this, 45f, 50);
            camera.Position.Y = 25;
            camera.Target.Y = 0;
            camera.RotateCamera = true;
            Components.Add(camera);

        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            base.Update(gameTime);

            // here we update the object shader(effect) matrices
            // so it can perform the space calculations on the vertices
            plane.UpdateShaderMatrices(camera.ViewTransform, camera.ProjectionTransform);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.20f, 0.20f, 0.20f));

            plane.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}

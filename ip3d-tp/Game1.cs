﻿using Microsoft.Xna.Framework;
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
            plane = new Plane(this, "ground_texture", 128*2, 128*2, terrainHeightMap.Width - 1, terrainHeightMap.Height - 1);
            Components.Add(plane);

            // dispace the vertices of the plane, based on the given heightmap
            plane.SetHeightFromTexture(terrainHeightMap);

            // toogle wireframe out of the box
            plane.ShowWireframe = true;

            // create the various cameras
            basicCamera = new BasicCamera(this, 45f, 128);

            freeCamera = new FreeCamera(this, 45f);
            freeCamera.Position.Y = 20;

            surfaceFollowCamera = new SurfaceFollowCamera(this, 45f, plane, 1.76f); // my height 
            surfaceFollowCamera.MaxVelocity = 0.5f;
            surfaceFollowCamera.Acceleration = new Vector3(0.01f);

            // set the default camera
            currentCamera = surfaceFollowCamera;
            Components.Add(currentCamera);
            
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
            plane.UpdateShaderMatrices(currentCamera.ViewTransform, currentCamera.ProjectionTransform);
            worldAxis.UpdateShaderMatrices(currentCamera.ViewTransform, currentCamera.ProjectionTransform);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.20f, 0.20f, 0.20f));

            plane.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}

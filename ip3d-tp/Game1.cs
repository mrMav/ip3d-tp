using ip3d_tp.Particles;
using ip3d_tp.Physics3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ip3d_tp
{
    /// <summary>
    /// Holds global data and game flags
    /// </summary>
    public static class Global
    {

        // toggle for debug
        public static bool ShowHelp = true;

        // aim mode
        public enum PlayerAimMode
        {
            Camera,
            Keys
        }
        public static PlayerAimMode AimMode = PlayerAimMode.Camera;

        // should the world particles be rendered
        public static bool RenderParticles = true;

        // container for all the particle systems
        public static List<ParticleEmitter> ParticleEmitters = new List<ParticleEmitter>();

        // count particles alive particles in game
        public static int AliveParticles = 0;

        // define the player tank id
        public static short PlayerID = 0;

        // bot behaviour 
        public enum BotBehaviour
        {
            None,
            Seek,
            Flee,
            Pursuit,
            Evade
        }

        // bot population number
        public static int nBots = 4;

        // bots list
        public static Tank[] Bots = new Tank[nBots];

    }

    public class Game1 : Game
    {
        // flag indicating if the mouse is captured or not 
        bool captureMouse = true;
        
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
        
        // the player tank
        Tank tank1;

        // a particle system to feedback hits
        QuadParticleEmitter HitEmitter;

        /*
         * cameras
         */

        // holds the current active camera
        Camera currentCamera;
         
        // the cameras that will be attached to the first tank
        ThirdPersonCamera ThirdPersonCamera1;
                
        // acts as a free camera, meaning it is possible to fly around
        FreeCamera freeCamera;

        // follows the surface, at an offset from the ground
        SurfaceFollowCamera surfaceFollowCamera;

        /*
         * lights
         */

        // direction light properties
        // indicates the direction that the infinity light comes from
        public Vector3 LightDirection = new Vector3(-1, -2, -1);

        // the color of the light
        public Vector4 LightColor = Color.White.ToVector4();

        // the intensity of said light
        public float LightIntensity = 1.0f;

        Random rnd = new Random(10);

        Stopwatch stopwatch;

        long tickCount = 0;
        double sumOfMilliseconds = 0;
        double averageMilliseconds = 0;
        double maxMills = double.MinValue;
        double minMills = double.MaxValue;

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
            IsFixedTimeStep = true;

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

            ParticleManager.Game = this;

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
            plane = new Plane(this, "Textures/ground_diffuse", 128*3, 128*3, terrainHeightMap.Width - 1, terrainHeightMap.Height - 1, 0.2f);
            Components.Add(plane);

            // dispace the vertices of the plane, based on the given heightmap, and adjust by a scale
            plane.SetHeightFromTexture(terrainHeightMap, 0.08f);
            
            // create the tanks 
            tank1 = new Tank(this);
            tank1.Body.Mass = 8.5f;
            tank1.Body.MaxVelocity = 0.58f;
            tank1.Body.X = 4f;  // offset a bit so the two don't overlap
            tank1.TankID = Global.PlayerID;

            //tank2 = new Tank(this);
            //tank2.Body.X = -40f;
            //tank2.Body.Z =  40f;
            //tank2.TankID = 1;  // identify this tank as ID 1, used for the controls
            //tank2.BotBehaviour = Global.BotBehaviour.Pursuit;
            //tank2.TargetBody = tank1.Body;

            //tank3 = new Tank(this);
            //tank3.Body.X = 40f;
            //tank3.Body.Z = -40f;
            //tank3.TankID = 2;  // identify this tank as ID 1, used for the controls
            //tank3.BotBehaviour = Global.BotBehaviour.Pursuit;
            //tank3.TargetBody = tank2.Body;

            for(int i = 0; i < Global.nBots; i++)
            {
                Tank b = new Tank(this);

                b.Body.X = (float)Utils.RandomBetween(rnd, -60, 60);
                b.Body.Y = (float)Utils.RandomBetween(rnd, -60, 60);
                b.Body.MaxVelocity = 0.2f;
                b.TankID = (short)(i + 1);
                //b.BotBehaviour = (Global.BotBehaviour)(Utils.RandomBetween(rnd, 1.0, 5.0));
                b.BotBehaviour = Global.BotBehaviour.Pursuit;

                Global.Bots[i] = b;
                

            }
            for (int i = 0; i < Global.nBots; i++)
            {
                Global.Bots[i].TargetBody = tank1.Body;

            }

            // hit emitter
            HitEmitter = new QuadParticleEmitter(this, Vector3.Zero, 0.5f, 0.5f, "Textures/white_particle", 0.5f, 200, 3);
            HitEmitter.ParticleTint = Color.Yellow;
            HitEmitter.MakeParticles(1f, Color.White);
            HitEmitter.ParticleVelocity = new Vector3(0f, 5f, 0f);
            HitEmitter.SpawnRate = 0f;
            HitEmitter.Burst = true;
            HitEmitter.ParticlesPerBurst = 50;
            HitEmitter.XVelocityVariationRange = new Vector2(-1000f, 1000f);
            HitEmitter.YVelocityVariationRange = new Vector2(-20f, 1000f);
            HitEmitter.ZVelocityVariationRange = new Vector2(-1000f, 1000f);
            HitEmitter.ParticleLifespanMilliseconds = 1000f;
            HitEmitter.ParticleLifespanVariationMilliseconds = 200f;
            HitEmitter.Activated = false;
            HitEmitter.InitialScale = 5f;
            HitEmitter.FinalScale = 0.01f;
            ParticleManager.AddParticleEmitter(HitEmitter);
            

            /*
             * cameras
             * 
             */

            // create the various cameras
            ThirdPersonCamera1 = new ThirdPersonCamera(this, tank1, plane, 20f);  // see definition for an understanding

            //ThirdPersonCamera2 = new ThirdPersonCamera(this, tank2, plane, 20f);
            
            freeCamera = new FreeCamera(this, 45f);
            freeCamera.AccelerationValue = 1f;
            freeCamera.Position.X = 0;
            freeCamera.Position.Y = 5;
            freeCamera.Position.Z = 5;
            freeCamera.Yaw = -60f;
            freeCamera.Pitch = -60f;

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

            stopwatch = Stopwatch.StartNew();

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
                Global.AimMode = Global.PlayerAimMode.Camera;

            } else if (Controls.IsKeyPressed(Keys.F2))
            {
                currentCamera = surfaceFollowCamera;
                Global.AimMode = Global.PlayerAimMode.Keys;

            } else if (Controls.IsKeyPressed(Keys.F3))
            {
                currentCamera = freeCamera;
                Global.AimMode = Global.PlayerAimMode.Keys;

            }

            // toggle wireframe
            if (Controls.IsKeyPressed(Keys.F) && Global.ShowHelp)
            {
                plane.ShowWireframe = !plane.ShowWireframe;
            }

            // toggle normals
            if (Controls.IsKeyPressed(Keys.N) && Global.ShowHelp)
            {
                plane.ShowNormals = !plane.ShowNormals;
            }

            // toogle mouse capture
            if (Controls.IsKeyPressed(Keys.M))
            {
                IsMouseVisible = captureMouse;
                captureMouse = !captureMouse;
            }

            // toogle help
            if (Controls.IsKeyPressed(Keys.H))
            {
                Global.ShowHelp = !Global.ShowHelp;
            }

            #endregion

            // locking the mouse only after the components are updated
            if (captureMouse)
            {
                Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
            }

            // every component will be updated after base update
            base.Update(gameTime);

            // update light to see effects of shader
            //LightDirection.X = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
            //LightDirection.Z = (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds);

            // updates the state of the current camera
            currentCamera.Update(gameTime);

            // we update the tanks manually so we can more flexibility
            // they are not part of the component system.
            // Note: all this internal component system is a bit week.
            //       I hope in the future of this project to develop our own
            //       node based object structure. Let's see how that goes. If I have the time.
            tank1.Update(gameTime, currentCamera, plane);

            for(int i = 0; i < Global.Bots.Length; i++)
            {
                Global.Bots[i].Update(gameTime, currentCamera, plane);
            }

            // collision needs to run on a fairly high speed in order
            // to be accurate. The implication of this is the
            // method that we are defining the tanks height.
            // the tanks are glued to the ground, and fetch the new height
            // every frame. This leads to 'teleporting' and oder glitchs when 
            // applying the collision resolution.
            for (int i = 0; i < 2; i++)   // sampling 2 times for acuracy 
            {

                //if (Physics.SATCollide(tank1.Body, tank2.Body))
                //{

                //    tank1.BodyDebug.MaterialColor = Color.Red;

                //}
                //else
                //{
                //    tank1.BodyDebug.MaterialColor = Color.Blue;

                //};

                for(int j = 0; j < Global.Bots.Length; j++)
                {

                    Physics.SATCollide(tank1.Body, Global.Bots[j].Body);

                }

                for (int j = 0; j < Global.Bots.Length; j++)
                {

                    for (int k = 0; k < Global.Bots.Length; k++)
                    {

                        if (Global.Bots[j].TankID != Global.Bots[k].TankID)
                        {

                            Physics.SATCollide(Global.Bots[j].Body, Global.Bots[k].Body);

                        }


                    }

                }
                
                tank1.PostMotionUpdate(gameTime, currentCamera, plane);
                for (int j = 0; j < Global.Bots.Length; j++)
                {

                    Global.Bots[j].PostMotionUpdate(gameTime, currentCamera, plane);

                }
            }


            // tet projectile collision
            for(int i = 0; i < tank1.Bullets.Count; i++)
            {
                Projectile p = tank1.Bullets[i];

                if(p.Alive)
                {
                    for(int j = 0; j < Global.nBots; j++)
                    {
                        Body b = Global.Bots[j].Body;

                        if(Physics.SATIntersect(p.Body, b))
                        {
                            Console.WriteLine("hit");

                            p.Kill();

                            // fx

                            HitEmitter.Position = p.Body.Position;
                            HitEmitter.UpdateMatrices(Matrix.CreateTranslation(p.Body.Position));
                            HitEmitter.Activated = true;


                        }

                    }

                }

            }
            tank1.UpdateProjectiles(gameTime, plane, currentCamera);
            HitEmitter.Update(gameTime);


            tank1.CalculateAnimations(gameTime, currentCamera, plane);
            for (int j = 0; j < Global.Bots.Length; j++)
            {

                Global.Bots[j].CalculateAnimations(gameTime, currentCamera, plane);

            }

            // here we update the object shader(effect) matrices
            // so it can perform the space calculations on the vertices
            plane.UpdateShaderMatrices(currentCamera.ViewTransform, currentCamera.ProjectionTransform);
            worldAxis.UpdateShaderMatrices(currentCamera.ViewTransform, currentCamera.ProjectionTransform);
            
            // update the last keyboard state
            Controls.UpdateLastStates();


            stopwatch.Stop();

            ++tickCount;

            sumOfMilliseconds += stopwatch.Elapsed.TotalMilliseconds;
            averageMilliseconds = sumOfMilliseconds / tickCount;

            maxMills = stopwatch.Elapsed.TotalMilliseconds > maxMills && tickCount > 20 ? stopwatch.Elapsed.TotalMilliseconds : maxMills;
            minMills = stopwatch.Elapsed.TotalMilliseconds < minMills && tickCount > 20 ? stopwatch.Elapsed.TotalMilliseconds : minMills;

            //Console.WriteLine(
            //    $"RealTime: {stopwatch.Elapsed.TotalMilliseconds:0.0000}, Avg: {averageMilliseconds:0.0000}, Min: {minMills}, Max: {maxMills} "
            //);

        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(new Color(0.20f, 0.20f, 0.20f));
            GraphicsDevice.Clear(Color.DeepSkyBlue);

            // we need to call the draw manually for the plane
            // it extends component, and not drawable
            plane.DrawCustomShader(gameTime, currentCamera, LightDirection, LightColor, LightIntensity);

            // draw the tanks with the light created
            tank1.Draw(gameTime, currentCamera, LightDirection, LightColor, LightIntensity);
            for (int j = 0; j < Global.Bots.Length; j++)
            {

                Global.Bots[j].Draw(gameTime, currentCamera, LightDirection, LightColor, LightIntensity);

            }

            //foreach(ParticleEmitter e in Global.ParticleEmitters)
            //{
            //    e.Draw(gameTime, currentCamera);
            //}

            if(Global.RenderParticles)
                ParticleManager.DrawEmitters(gameTime, currentCamera);

            //Particles.Draw(gameTime, currentCamera);

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

            if(Global.ShowHelp)
            {

                spriteBatch.DrawString(font, $"{Math.Round(FrameRate.AverageFramesPerSecond)}", new Vector2(10f, 10f), new Color(0f, 1f, 0f));
                spriteBatch.DrawString(font, $"Help (H, Toogle): {Global.ShowHelp}\nWireframe (F, Toogle): {plane.ShowWireframe}\nNormals (N, Toogle): {plane.ShowNormals}", new Vector2(10f, 26f), new Color(0f, 1f, 0f));
                spriteBatch.DrawString(font, tank1.Body.GetDebugString(), new Vector2(10f, graphics.PreferredBackBufferHeight - 5 * 26f), new Color(0f, 1f, 0f));
                spriteBatch.DrawString(font, tank1.GetDebugInfo(), new Vector2(10f, graphics.PreferredBackBufferHeight - 7 * 26f), new Color(0f, 1f, 0f));
                spriteBatch.DrawString(font, $"Cycle between cameras in F1-F12\nAbout the camera:\n{currentCamera.About()}", new Vector2(graphics.PreferredBackBufferWidth / 2, 10f), new Color(0f, 1f, 0f));

            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

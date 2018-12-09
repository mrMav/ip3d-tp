using ip3d_tp.Physics3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ip3d_tp
{
    /// <summary>
    /// Simple box
    /// </summary>
    class Box
    {
        // reference to the game 
        Game Game;

        // wordl
        public Matrix WorldTransform;

        // position
        public Vector3 Position;

        public float Width, Height, Depth;

        // if true, a geometry update is needed
        public bool DirtyGeometry;

        // list of vertices and indices
        public VertexPositionNormalTexture[] VertexList;

        // gpu buffers
        VertexBuffer VertexBuffer;

        // the effect(shader) to apply when rendering
        Effect CustomEffect;

        public Color MaterialColor;

        // rasterizeres for solid and wireframe modes
        RasterizerState SolidRasterizerState;
        RasterizerState WireframeRasterizerState;

        // needed wile we have spritebatch messing around
        BlendState BlendState;

        // wireframe rendering toogle
        public bool ShowSolid;
        public bool ShowWireframe;        

        // constructor 
        public Box(Game game, Vector3 position, float width = 10f, float height = 10f, float depth = 10f)
        {

            // some basic assigning is done here
            Game = game;

            Position = position;

            Width = width;
            Height = height;
            Depth = depth;

            WorldTransform = Matrix.CreateTranslation(position);

            DirtyGeometry = false;
            
            // load our custom effect from the content
            CustomEffect = Game.Content.Load<Effect>("Effects/Phong");

            MaterialColor = new Color(208, 208, 208);

            ShowSolid = true;
            ShowWireframe = false;  // disable out of the box wireframe

            // setup the rasterizers
            SolidRasterizerState = new RasterizerState();
            WireframeRasterizerState = new RasterizerState();

            SolidRasterizerState.FillMode = FillMode.Solid;
            WireframeRasterizerState.FillMode = FillMode.WireFrame;

            BlendState = new BlendState();
            BlendState.AlphaBlendFunction = BlendFunction.Add;

            // create the geometry
            CreateGeometry();

        }

        public void Draw(GameTime gameTime, Camera camera)
        {

            Game.GraphicsDevice.BlendState = this.BlendState;
            Game.GraphicsDevice.SetVertexBuffer(VertexBuffer);
            
            Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(WorldTransform));

            CustomEffect.Parameters["World"].SetValue(WorldTransform);
            CustomEffect.Parameters["View"].SetValue(camera.ViewTransform);
            CustomEffect.Parameters["Projection"].SetValue(camera.ProjectionTransform);
            CustomEffect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
            CustomEffect.Parameters["MaterialColor"].SetValue(MaterialColor.ToVector4());

            CustomEffect.CurrentTechnique.Passes[0].Apply();

            if (ShowSolid)
            {

                Game.GraphicsDevice.RasterizerState = this.SolidRasterizerState;
                Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 36);

            }
            if(ShowWireframe)
            {
                // same as before but in wireframe rasterizer
                Game.GraphicsDevice.RasterizerState = WireframeRasterizerState;
                Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 36);

            }

        }

        private void CreateGeometry()
        {
            VertexList = new VertexPositionNormalTexture[36];

            // Normal vectors for each face (needed for lighting / display)
            Vector3 normalFront = new Vector3(0.0f, 0.0f, 1.0f);
            Vector3 normalBack = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 normalTop = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 normalBottom = new Vector3(0.0f, -1.0f, 0.0f);
            Vector3 normalLeft = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 normalRight = new Vector3(1.0f, 0.0f, 0.0f);

            // UV texture coordinates
            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);

            // Calculate the position of the vertices on the top face.
            Vector3 topLeftFront = new Vector3(-0.5f * Width, 0.5f * Height, -0.5f * Depth);
            Vector3 topLeftBack = new Vector3(-0.5f * Width, 0.5f * Height, 0.5f * Depth);
            Vector3 topRightFront = new Vector3(0.5f * Width, 0.5f * Height, -0.5f * Depth);
            Vector3 topRightBack = new Vector3(0.5f * Width, 0.5f * Height, 0.5f * Depth);

            // Calculate the position of the vertices on the bottom face.
            Vector3 btmLeftFront = new Vector3(-0.5f * Width, -0.5f * Height, -0.5f * Depth);
            Vector3 btmLeftBack = new Vector3(-0.5f * Width, -0.5f * Height, 0.5f * Depth);
            Vector3 btmRightFront = new Vector3(0.5f * Width, -0.5f * Height, -0.5f * Depth);
            Vector3 btmRightBack = new Vector3(0.5f * Width, -0.5f * Height, 0.5f * Depth);

            // Add the vertices for the FRONT face.
            VertexList[0] = new VertexPositionNormalTexture(topLeftFront, normalFront, textureTopLeft);
            VertexList[1] = new VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft);
            VertexList[2] = new VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight);
            VertexList[3] = new VertexPositionNormalTexture(btmLeftFront, normalFront, textureBottomLeft);
            VertexList[4] = new VertexPositionNormalTexture(btmRightFront, normalFront, textureBottomRight);
            VertexList[5] = new VertexPositionNormalTexture(topRightFront, normalFront, textureTopRight);

            // Add the vertices for the BACK face.
            VertexList[6] = new VertexPositionNormalTexture(topLeftBack, normalBack, textureTopRight);
            VertexList[7] = new VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft);
            VertexList[8] = new VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight);
            VertexList[9] = new VertexPositionNormalTexture(btmLeftBack, normalBack, textureBottomRight);
            VertexList[10] = new VertexPositionNormalTexture(topRightBack, normalBack, textureTopLeft);
            VertexList[11] = new VertexPositionNormalTexture(btmRightBack, normalBack, textureBottomLeft);

            // Add the vertices for the TOP face.
            VertexList[12] = new VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft);
            VertexList[13] = new VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight);
            VertexList[14] = new VertexPositionNormalTexture(topLeftBack, normalTop, textureTopLeft);
            VertexList[15] = new VertexPositionNormalTexture(topLeftFront, normalTop, textureBottomLeft);
            VertexList[16] = new VertexPositionNormalTexture(topRightFront, normalTop, textureBottomRight);
            VertexList[17] = new VertexPositionNormalTexture(topRightBack, normalTop, textureTopRight);

            // Add the vertices for the BOTTOM face. 
            VertexList[18] = new VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft);
            VertexList[19] = new VertexPositionNormalTexture(btmLeftBack, normalBottom, textureBottomLeft);
            VertexList[20] = new VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight);
            VertexList[21] = new VertexPositionNormalTexture(btmLeftFront, normalBottom, textureTopLeft);
            VertexList[22] = new VertexPositionNormalTexture(btmRightBack, normalBottom, textureBottomRight);
            VertexList[23] = new VertexPositionNormalTexture(btmRightFront, normalBottom, textureTopRight);

            // Add the vertices for the LEFT face.
            VertexList[24] = new VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight);
            VertexList[25] = new VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft);
            VertexList[26] = new VertexPositionNormalTexture(btmLeftFront, normalLeft, textureBottomRight);
            VertexList[27] = new VertexPositionNormalTexture(topLeftBack, normalLeft, textureTopLeft);
            VertexList[28] = new VertexPositionNormalTexture(btmLeftBack, normalLeft, textureBottomLeft);
            VertexList[29] = new VertexPositionNormalTexture(topLeftFront, normalLeft, textureTopRight);

            // Add the vertices for the RIGHT face. 
            VertexList[30] = new VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft);
            VertexList[31] = new VertexPositionNormalTexture(btmRightFront, normalRight, textureBottomLeft);
            VertexList[32] = new VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight);
            VertexList[33] = new VertexPositionNormalTexture(topRightBack, normalRight, textureTopRight);
            VertexList[34] = new VertexPositionNormalTexture(topRightFront, normalRight, textureTopLeft);
            VertexList[35] = new VertexPositionNormalTexture(btmRightBack, normalRight, textureBottomRight);


            VertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, 36, BufferUsage.WriteOnly);
            VertexBuffer.SetData<VertexPositionNormalTexture>(VertexList);

        }

    }        

}
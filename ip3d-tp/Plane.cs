using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ip3d_tp
{
    /// <summary>
    /// The plane class creates a plane primitive in the XZ plane.
    /// There are options to define the size and number of subdivisions.
    /// </summary>
    class Plane : GameComponent
    {

        // basic dimensions
        public float Width { get; private set; }
        public float Depth { get; private set; }
        public int XSubs { get; private set; }
        public int ZSubs { get; private set; }

        public float SubWidth { get; private set; }
        public float SubHeight { get; private set; }

        // if true, a geometry update is needed
        public bool DirtyGeometry;
        
        // world transform or matrix is the matrix we will multiply
        // our vertices for. this transforms the vertices from local
        // space to world space
        Matrix WorldTransform;

        // list of vertices and indices
        public VertexPositionTexture[] VertexList;
        short[] IndicesList;

        // gpu buffers
        VertexBuffer VertexBuffer;
        IndexBuffer IndexBuffer;

        // the effect(shader) to apply when rendering
        // we will use Monogame built in BasicEffect for the purpose
        BasicEffect ColorShaderEffect;
        BasicEffect TextureShaderEffect;

        // reference to the textures
        Texture2D DiffuseMap;

        // uv scale, for defining the texture scale
        // when updated, there is a need to flag the dirty geometry flag
        float UVScale;

        // rasterizeres for solid and wireframe modes
        RasterizerState SolidRasterizerState;
        RasterizerState WireframeRasterizerState;

        // wireframe rendering toogle
        public bool ShowWireframe;
        
        // constructor 
        public Plane(Game game, string textureKey, float width = 10f, float depth = 10f, int xSubs = 1, int zSubs = 1, float uvscale = 1f) : base(game)
        {

            // some basic assigning is done here

            Width = width;
            Depth = depth;
            XSubs = xSubs;
            ZSubs = zSubs;

            DirtyGeometry = false;

            WorldTransform = Matrix.Identity;

            DiffuseMap = Game.Content.Load<Texture2D>(textureKey);
            UVScale = uvscale;

            // in this phase, I want to stop everything if the texture is null
            if (DiffuseMap == null)
            {
                throw new Exception($"diffuseMap is null. key {textureKey} not found in content.");
            }

            // properties for the texture shader
            TextureShaderEffect = new BasicEffect(game.GraphicsDevice);
            TextureShaderEffect.LightingEnabled = false;  // still not using lighting
            TextureShaderEffect.VertexColorEnabled = false;  // turn off the color 
            TextureShaderEffect.TextureEnabled = true;  // but do turn up the texture channel
            TextureShaderEffect.Texture = DiffuseMap;  // assign out texture        
            
            // this is the shader for the wireframe
            ColorShaderEffect = new BasicEffect(game.GraphicsDevice);
            ColorShaderEffect.VertexColorEnabled = false;
            ColorShaderEffect.DiffuseColor = new Vector3(0, 0, 0);
            ColorShaderEffect.LightingEnabled = false;  // we won't be using light. we would need normals for that
            
            ShowWireframe = true;  // enable out of the box wireframe
            
            // setup the rasterizers
            SolidRasterizerState = new RasterizerState();
            WireframeRasterizerState = new RasterizerState();

            SolidRasterizerState.FillMode = FillMode.Solid;
            WireframeRasterizerState.FillMode = FillMode.WireFrame;
            
            // create the geometry
            CreateGeometry();

        }

        public void Draw(GameTime gameTime)
        {

            Game.GraphicsDevice.Indices = IndexBuffer;
            Game.GraphicsDevice.SetVertexBuffer(VertexBuffer);

            // render the geometry using a triangle list
            // we only use one pass of the shader, but we could have more.
            Game.GraphicsDevice.RasterizerState = SolidRasterizerState;
            TextureShaderEffect.CurrentTechnique.Passes[0].Apply();

            // send a draw call to the gpu, using a triangle list as a primitive. 
            // I could be using a triangle strip, but nowadays computers have tons of memory
            // and I'd rather keep the draw calls at a minimum and save the cpu the struggle
            Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndicesList.Length / 3);
            
            if (ShowWireframe)
            {
                // same as before but in wireframe rasterizer

                ColorShaderEffect.CurrentTechnique.Passes[0].Apply();
                Game.GraphicsDevice.RasterizerState = WireframeRasterizerState;
                Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndicesList.Length / 3);

            }

        }

        // updates the effects matrices
        public void UpdateShaderMatrices(Matrix viewTransform, Matrix projectionTransform)
        {
            ColorShaderEffect.Projection = projectionTransform;
            ColorShaderEffect.View = viewTransform;
            ColorShaderEffect.World = WorldTransform;

            TextureShaderEffect.Projection = projectionTransform;
            TextureShaderEffect.View = viewTransform;
            TextureShaderEffect.World = WorldTransform;

        }

        private void CreateGeometry()
        {

            // creates the geometry, whithout taking into account any heightmap!
            // if we want that, next, we call the setheight function


            // calculate the number of vertices we will have in each side
            int nVerticesWidth = XSubs + 1;
            int nVerticesDepth = ZSubs + 1;

            // total vertices and total indices
            int verticesCount = nVerticesWidth * nVerticesDepth;
            int indicesCount = XSubs * ZSubs * 6;  // 6 because a subdivision is made of 2 triagnles, each one with 3 vertices(indexed)

            // the array of vertices and indices are created
            VertexList = new VertexPositionTexture[verticesCount];
            IndicesList = new short[indicesCount];

            // the size of each subdivision
            SubWidth  = Width / XSubs;
            SubHeight = Depth / ZSubs;

            int currentVertice = 0;

            // create the vertices
            for(int z = 0; z <= ZSubs; z++)
            {
                for(int x = 0; x <= XSubs; x++)
                {

                    // we will put the 0, 0 on the center of the plane
                    // because of that we will translate every vertex halfwidth and halfdepth
                    // this ensures our plane has the origin in the middle

                    float xx = x * SubWidth  - Width / 2;
                    float zz = z * SubHeight - Depth / 2;

                    // texture coordinates, must be normalized
                    float u = (float)x / ((float)XSubs * UVScale);
                    float v = (float)z / ((float)ZSubs * UVScale);

                    // create the vertice, and increment to the next one
                    VertexList[currentVertice++] = new VertexPositionTexture(new Vector3(xx, 0, zz), new Vector2(u, v));

                }
            }

            // create indices
            int currentIndice = 0;
            int currentSubDivison = 0;
            for (int z = 0; z < ZSubs; z++)
            {
                for (int x = 0; x < XSubs; x++)
                {
                    /* calculate positions in the array
                     * 
                     *  1---2
                     *  | / |
                     *  4---3
                     *  
                     */

                    int vert1 = currentSubDivison + z;
                    int vert2 = vert1 + 1;
                    int vert3 = vert2 + nVerticesWidth;
                    int vert4 = vert3 - 1;

                    // first tri
                    IndicesList[currentIndice++] = (short)vert1;
                    IndicesList[currentIndice++] = (short)vert2;
                    IndicesList[currentIndice++] = (short)vert4;

                    // secon tri
                    IndicesList[currentIndice++] = (short)vert4;
                    IndicesList[currentIndice++] = (short)vert2;
                    IndicesList[currentIndice++] = (short)vert3;

                    currentSubDivison++;

                }

            }

            // create the buffers, and attach the corresponding data:

            VertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionTexture.VertexDeclaration, VertexList.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData<VertexPositionTexture>(VertexList);
            
            IndexBuffer = new IndexBuffer(Game.GraphicsDevice, typeof(short), IndicesList.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData<short>(IndicesList);

        }

        public void SetHeightFromTexture(Texture2D texture, float scaler = 1f)
        {

            // extract the color data from the texture
            Color[] colorData = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(colorData);

            int currentVertice = 0;

            // cycle through every subdivision, assigning the height
            // corresponding to the texture
            for (int z = 0; z <= ZSubs; z++)
            {
                for (int x = 0; x <= XSubs; x++)
                {

                    int textureX = x;
                    int textureY = z;

                    Color currentColor = colorData[z * texture.Width + x];
                    
                    VertexList[currentVertice++].Position.Y = currentColor.R * scaler;

                }
            }

            // unbound the buffer, so we can change it
            Game.GraphicsDevice.SetVertexBuffer(null);

            // set the new data in
            VertexBuffer.SetData<VertexPositionTexture>(VertexList);

        }

    }

}

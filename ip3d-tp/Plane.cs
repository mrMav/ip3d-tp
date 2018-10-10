using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ip3d_tp
{
    class Plane : GameComponent
    {

        // the prism dimensions
        public float Width { get; private set; }
        public float Depth { get; private set; }
        public int XSubs { get; private set; }
        public int ZSubs { get; private set; }

        // if true, a geometry update is needed
        public bool DirtyGeometry;

        // the current model rotation        
        Vector3 ModelRotation;
        // TODO: add some scale and position for some fun

        // world transform or matrix is the matrix we will multiply 
        // our vertices for. this transforms the vertices from local
        // space to world space
        Matrix WorldTransform;

        VertexPositionColor[] VertexList;
        short[] IndicesList;

        VertexBuffer VertexBuffer;
        IndexBuffer IndexBuffer;

        // the effect(shader) to apply when rendering
        // we will use Monogame built in BasicEffect for the purpose
        BasicEffect ColorShaderEffect;

        // wireframe rendering toogle
        public bool ShowWireframe;
        
        public Plane(Game game, float width = 10, float depth = 10, int xSubs = 1, int zSubs = 1) : base(game)
        {

            Width = width;
            Depth = depth;
            XSubs = xSubs;
            ZSubs = zSubs;

            DirtyGeometry = false;

            ModelRotation = Vector3.Zero;
            WorldTransform = Matrix.Identity;

            ColorShaderEffect = new BasicEffect(game.GraphicsDevice);
            ColorShaderEffect.LightingEnabled = false;  // we won't be using light. we would need normals for that
            ColorShaderEffect.VertexColorEnabled = true;  // we do want color though
            ShowWireframe = true;  // enable out of the box wireframe
            
            // create the geometry
            CreateGeometry();

        }

        public void Draw(GameTime gameTime)
        {

            Game.GraphicsDevice.Indices = IndexBuffer;
            Game.GraphicsDevice.SetVertexBuffer(VertexBuffer);

            // render the geometry using a triangle list
            // we only use one pass of the shader, but we could have more.
            ColorShaderEffect.VertexColorEnabled = true;
            ColorShaderEffect.DiffuseColor = new Vector3(1, 1, 1);
            ColorShaderEffect.CurrentTechnique.Passes[0].Apply();

            Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndicesList.Length / 3); 

            if (ShowWireframe)
            {
                // the color of the wireframe is white by default
                // it is stored in the DiffuseColor porperty of the effect

                ColorShaderEffect.VertexColorEnabled = false;  // deactivate the color channel
                ColorShaderEffect.DiffuseColor = new Vector3(1, 0, 0);
                ColorShaderEffect.CurrentTechnique.Passes[0].Apply();

                Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineStrip, 0, 0, IndicesList.Length - 1 ); // here we connect all vertices width a line strip

            }

        }

        // updates the effect matrices
        public void UpdateShaderMatrices(Matrix viewTransform, Matrix projectionTransform)
        {
            ColorShaderEffect.Projection = projectionTransform;
            ColorShaderEffect.View = viewTransform;
            ColorShaderEffect.World = WorldTransform;
        }

        private void CreateGeometry()
        {

            int nVerticesWidth = XSubs + 1;
            int nVerticesDepth = ZSubs + 1;

            int verticesCount = nVerticesWidth * nVerticesDepth;
            int indicesCount = XSubs * ZSubs * 6;

            // first, the array of vertices is created
            VertexList = new VertexPositionColor[verticesCount];
            IndicesList = new short[indicesCount];

            // the size of each subdivision
            float subWidth = Width / XSubs;
            float subDepth = Depth / ZSubs;

            int currentVertice = 0;

            // create the vertices
            for(int z = 0; z <= ZSubs; z++)
            {
                for(int x = 0; x <= XSubs; x++)
                {

                    // we will put the 0, 0 on the center of the plane
                    // because of that we will translate every vertice halfwidth and halfdepth

                    float xx = x * subWidth - Width / 2;
                    float zz = z * subDepth - Depth / 2;

                    // texture coordinates, must be normalized
                    float u = x / XSubs;
                    float v = z / ZSubs;

                    //VertexList[currentVertice++] = new VertexPositionTexture(new Vector3(xx, 0, zz), new Vector2(u, v));
                    VertexList[currentVertice++] = new VertexPositionColor(new Vector3(xx, 0, zz), Color.LightGray);

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

            //VertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionTexture.VertexDeclaration, VertexList.Length, BufferUsage.WriteOnly);
            //VertexBuffer.SetData<VertexPositionTexture>(VertexList);
            VertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionColor.VertexDeclaration, VertexList.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData<VertexPositionColor>(VertexList);
            
            IndexBuffer = new IndexBuffer(Game.GraphicsDevice, typeof(short), IndicesList.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData<short>(IndicesList);

        }

    }

}

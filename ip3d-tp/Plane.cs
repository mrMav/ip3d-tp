using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        VertexPositionTexture[] VertexList;
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

        private void CreateGeometry()
        {

            // first, the array of vertices is created
            int verticesCount = (XSubs + 1) * (ZSubs + 1);
            int indicesCount = (XSubs * 2) * (ZSubs * 2);

            VertexList = new VertexPositionTexture[verticesCount];
            IndicesList = new short[indicesCount];

            // the size of each subdivision
            float subWidth = Width / XSubs;
            float subDepth = Depth / ZSubs;

            int currentVertice = 0;

            // create the vertices
            for(int z = 0; z < ZSubs; z++)
            {
                for(int x = 0; x < XSubs; x++)
                {

                    // we will put the 0, 0 on the center of the plane
                    // because of that we will translate every vertice halfwidth and halfdepth

                    float xx = x * subWidth - Width / 2;
                    float zz = z * subDepth - Depth / 2;

                    // texture coordinates, must be normalized
                    float u = x / XSubs;
                    float v = z / ZSubs;

                    VertexList[currentVertice++] = new VertexPositionTexture(new Vector3(xx, 0, zz), new Vector2(u, v));

                }
            }
        }
    }

}

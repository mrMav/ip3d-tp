using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ip3d_tp
{
    class Axis3D : DrawableGameComponent
    {
        VertexPositionColor[] vertices;
        BasicEffect effect;
        Matrix worldMatrix;

        Camera Camera;

        Vector3 Position;
        float Size;

        public Axis3D(Game game, Camera camera, Vector3 position, float size = 1f) : base(game)
        {

            Size = size;
            Camera = camera;
            Position = position;

            worldMatrix = Matrix.Identity;

            effect = new BasicEffect(game.GraphicsDevice);
            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;

            // Cria os eixos 3D
            CreateGeometry();
        }

        private void CreateGeometry()
        {
            int vertexCount = 6; 

            vertices = new VertexPositionColor[vertexCount];

            // Linha sobre o eixo X
            this.vertices[0] = new VertexPositionColor(new Vector3(Position.X, Position.Y, Position.Z), Color.Red);
            this.vertices[1] = new VertexPositionColor(new Vector3(Position.X + Size, Position.Y, Position.Z), Color.Red);

            // Linha sobre o eixo Y
            this.vertices[2] = new VertexPositionColor(new Vector3(Position.X, Position.Y, Position.Z), Color.Green);
            this.vertices[3] = new VertexPositionColor(new Vector3(Position.X, Position.Y + Size, Position.Z), Color.Green);

            // Linha sobre o eixo Z
            this.vertices[4] = new VertexPositionColor(new Vector3(Position.X, Position.Y, Position.Z), Color.Blue);
            this.vertices[5] = new VertexPositionColor(new Vector3(Position.X, Position.Y, Position.Z + Size), Color.Blue);
        }

        // updates the effect matrices
        public void UpdateShaderMatrices(Matrix viewTransform, Matrix projectionTransform)
        {
            effect.Projection = projectionTransform;
            effect.View = viewTransform;
            effect.World = worldMatrix;
        }        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
                        
            effect.CurrentTechnique.Passes[0].Apply();
            Game.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>( PrimitiveType.LineList, vertices, 0, 3);
            
        }
    }
}

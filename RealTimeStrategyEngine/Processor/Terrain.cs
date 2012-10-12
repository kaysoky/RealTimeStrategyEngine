using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RealTimeStrategyEngine.Processor
{
    public class Terrain
    {
        Texture2D TerrainMap;
        Texture2D ColorMap;
        VertexPositionTexture[] Grid;
        VertexDeclaration GridDeclaration;
        int[] IndexBuffer;
        public float Scale;
        public int GridWidth;
        public int GridHeight;

        public Terrain(float Scale)
        {
            //Get the textures and color for the terrain
            TerrainMap = Manager.GeneratePerlinNoise(MyGame.random.Next(15, 30));
            Color[] textureData = new Color[TerrainMap.Width * TerrainMap.Height];
            TerrainMap.GetData<Color>(textureData);
            Color[] ColorScheme = Manager.GenerateStaticNoise(8, 8);
            for (int i = 0; i < ColorScheme.Length; i++)
            {
                ColorScheme[i] = Color.Lerp(ColorScheme[i], Color.Black, 0.8f + 0.2f * (float)MyGame.random.NextDouble());
            }
            ColorMap = new Texture2D(MyGame.graphics.GraphicsDevice, 8, 8);
            ColorMap.SetData<Color>(ColorScheme);

            //Calculate the necessary number of vertices to make the terrain mesh
            this.Scale = Scale;
            GridWidth = TerrainMap.Width / 10;
            GridHeight = TerrainMap.Width / 10;
            //Initializes the terrain with modulation based on the noise texture
            Grid = new VertexPositionTexture[GridWidth * GridHeight];
            GridDeclaration = new VertexDeclaration(MyGame.graphics.GraphicsDevice, VertexPositionTexture.VertexElements);
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    Grid[x + y * GridWidth] = new VertexPositionTexture(
                        new Vector3(x * Scale
                            , Scale * (textureData[5 * x + 5 * y * TerrainMap.Width].B / 32.0f - 8.0f)
                            , y * Scale)
                        , new Vector2((float)x / (GridWidth - 1.0f), (float)y / (GridHeight - 1.0f)));
                }
            }
            int counter = 0;
            IndexBuffer = new int[6 * (GridWidth - 1) * (GridHeight - 1)];
            for (int x = 0; x < GridWidth - 1; x++)
            {
                for (int y = 0; y < GridHeight - 1; y++)
                {
                    IndexBuffer[counter++] = x + y * GridWidth;
                    IndexBuffer[counter++] = x + 1 + y * GridWidth;
                    IndexBuffer[counter++] = x + 1 + (y + 1) * GridWidth;
                    IndexBuffer[counter++] = x + y * GridWidth;
                    IndexBuffer[counter++] = x + 1 + (y + 1) * GridWidth;
                    IndexBuffer[counter++] = x + (y + 1) * GridWidth;
                }
            }
        }
        public float FindElevation(Vector3 Position)
        {
            int XIndex = (int)(Position.X / Scale);
            XIndex = XIndex < 0 ? 0 : XIndex;
            XIndex = XIndex >= GridWidth - 1? GridWidth - 2 : XIndex;
            int ZIndex = (int)(Position.Z / Scale);
            ZIndex = ZIndex < 0 ? 0 : ZIndex;
            ZIndex = ZIndex >= GridHeight - 1? GridHeight - 2 : ZIndex;
            float XWeight = Position.X / Scale - XIndex;
            float ZWeight = Position.Z / Scale - ZIndex;
            return Grid[XIndex + ZIndex * GridWidth].Position.Y * (1.0f - XWeight) * (1.0f - ZWeight)
                + Grid[XIndex + 1 + ZIndex * GridWidth].Position.Y * XWeight * (1.0f - ZWeight)
                + Grid[XIndex + (ZIndex + 1) * GridWidth].Position.Y * (1.0f - XWeight) * ZWeight
                + Grid[XIndex + 1 + (ZIndex + 1) * GridWidth].Position.Y * XWeight * ZWeight;
        }
        public Vector3 GetRandomPosition()
        {
            Vector3 Position = new Vector3((float)MyGame.random.NextDouble() * GridWidth * Scale
                , 0.0f
                , (float)MyGame.random.NextDouble() * GridHeight * Scale);
            Position.Y = FindElevation(Position) + (float)MyGame.random.NextDouble() * Scale * (float)Math.Sqrt(GridWidth + GridHeight);
            return Position;
        }
        public void CalculateMousePosition()
        {            
            for (int x = 0; x < GridWidth - 1; x++)
            {
                for (int z = 0; z < GridHeight - 1; z++)
                {
                    if (CheckMouseIntersection(x, z))
                    {
                        Manager.cursor.isOnTerrain = true;
                        return;
                    }
                }
            }
            Manager.cursor.isOnTerrain = false;
        }
        /// <summary>
        /// Checks the current Mouse for intersection with the Terrain
        /// Saves the value of the intersection if true
        /// </summary>
        bool CheckMouseIntersection(int x, int z)
        {
            //Intersect with the Bottom Right Triangle of the terrain
            float? point = Manager.cursor.MouseRay.Intersects(
                new Plane(Grid[x + z * GridWidth].Position
                    , Grid[x + 1 + z * GridWidth].Position
                    , Grid[x + 1 + (z + 1) * GridWidth].Position));
            if (point.HasValue)
            {
                Vector3 vertex = Manager.cursor.MouseRay.Position
                    + point.GetValueOrDefault() * Manager.cursor.MouseRay.Direction;
                if (vertex.X >= Grid[x + z * GridWidth].Position.X
                    && vertex.X <= Grid[x + 1 + (z + 1) * GridWidth].Position.X
                    && vertex.Z >= Grid[x + z * GridWidth].Position.Z
                    && vertex.Z <= Grid[x + 1 + (z + 1) * GridWidth].Position.Z
                    && vertex.X >= vertex.Z)
                {
                    Manager.cursor.TerrainIntersection = vertex;
                    return true;
                }
            }
            //Intersect with the Upper Left Triangle of the terrain
            point = Manager.cursor.MouseRay.Intersects(
                new Plane(Grid[x + z * GridWidth].Position
                     , Grid[x + 1 + (z + 1) * GridWidth].Position
                     , Grid[x + (z + 1) * GridWidth].Position));
            if (point.HasValue)
            {
                Vector3 vertex = Manager.cursor.MouseRay.Position
                    + point.GetValueOrDefault() * Manager.cursor.MouseRay.Direction;
                if (vertex.X >= Grid[x + z * GridWidth].Position.X
                    && vertex.X <= Grid[x + 1 + (z + 1) * GridWidth].Position.X
                    && vertex.Z >= Grid[x + z * GridWidth].Position.Z
                    && vertex.Z <= Grid[x + 1 + (z + 1) * GridWidth].Position.Z
                    && vertex.X <= vertex.Z)
                {
                    Manager.cursor.TerrainIntersection = vertex;
                    return true;
                }
            }
            return false;
        }
        public void Draw()
        {
            Manager.ResetFor3D();
            MyGame.graphics.GraphicsDevice.VertexDeclaration = GridDeclaration;
            Manager.TexturedEffect.CurrentTechnique = Manager.TexturedEffect.Techniques["Textured"];
            Manager.TexturedEffect.Parameters["InputTexture"].SetValue(TerrainMap);
            Manager.TexturedEffect.Parameters["ColorMapTexture"].SetValue(ColorMap);
            Manager.TexturedEffect.Parameters["TextureAlphaThreshold"].SetValue(0.0f);
            Manager.TexturedEffect.Parameters["World"].SetValue(Matrix.Identity);
            Manager.TexturedEffect.Begin();
            Manager.TexturedEffect.CurrentTechnique.Passes.First<EffectPass>().Begin();
            MyGame.graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
                PrimitiveType.TriangleList
                , Grid
                , 0
                , Grid.Length
                , IndexBuffer
                , 0
                , IndexBuffer.Length / 3);
            Manager.TexturedEffect.CurrentTechnique.Passes.First<EffectPass>().End();
            Manager.TexturedEffect.End();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RealTimeStrategyEngine.Processor
{
    class Controller
    {
        public List<Unit> units = new List<Unit>();
        public Unit_KDTree SpatialTree;

        Color DisplayColor;
        public short Allegiance;
        double spawnTimer = 1.0;
        VertexDeclaration UnitDeclaration;

        public Controller()
        {
            DisplayColor = Manager.GetRandomColor(false);
            Allegiance = (short)(MyGame.random.Next() % short.MaxValue);
            UnitDeclaration = new VertexDeclaration(MyGame.graphics.GraphicsDevice, VertexPositionColor.VertexElements);
        }

        public void Update(GameTime gameTime, List<Controller> AllControllers, SpawnRateController.DataPoint Statistics)
        {
            List<Unit_KDTree> EnemyTrees = new List<Unit_KDTree>();
            for (int i = 0; i < AllControllers.Count; i++)
            {
                if (AllControllers[i].Allegiance != Allegiance)
                {
                    EnemyTrees.Add(AllControllers[i].SpatialTree);
                }
            }
            for (int i = 0; i < units.Count; i++)
            {
                units[i].Update(gameTime, EnemyTrees, SpatialTree);
                if (units[i].HitPoints <= 0.0)
                {
                    units.RemoveAt(i);
                }
            }
            spawnTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            if (spawnTimer <= 0.0)
            {
                spawnTimer = Manager.GeneralSpawnTimer;
                units.Add(new Unit(Manager.terrain.GetRandomPosition()
                    , (Unit.StatDistribution)MyGame.random.Next((int)Unit.StatDistribution.End)
                    , (Unit.BehaviorType)MyGame.random.Next((int)Unit.BehaviorType.End)));
                SpatialTree = new Unit_KDTree(-1, units.ToArray());
            }
        }

        public void Draw(GameTime gameTime, BoundingFrustum VisibleArea)
        {
            if (units.Count > 0)
            {
                VertexPositionColor[] display = new VertexPositionColor[units.Count * 2];
                int[] pointIndex = new int[units.Count];
                for (int i = 0; i < units.Count; i++)
                {
                    display[i * 2] = new VertexPositionColor(units[i].Position, DisplayColor);
                    pointIndex[i] = i * 2;
                    if (units[i].Target != null)
                    {
                        display[i * 2 + 1] = new VertexPositionColor(units[i].Target.Position, DisplayColor);
                    }
                    else
                    {
                        display[i * 2 + 1] = display[i * 2];
                    }
                }

                Manager.ResetFor3D();
                MyGame.graphics.GraphicsDevice.VertexDeclaration = UnitDeclaration;
                MyGame.graphics.GraphicsDevice.RenderState.PointSize = 5.0f;
                Manager.OrdinaryEffect.CurrentTechnique = Manager.OrdinaryEffect.Techniques["Ordinary"];
                Manager.OrdinaryEffect.Parameters["World"].SetValue(Matrix.Identity);
                Manager.OrdinaryEffect.Begin();
                Manager.OrdinaryEffect.CurrentTechnique.Passes.First<EffectPass>().Begin();
                MyGame.graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, display, 0, display.Length / 2);
                MyGame.graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.PointList, display, 0, display.Length
                    , pointIndex, 0, pointIndex.Length);
                Manager.OrdinaryEffect.CurrentTechnique.Passes.First<EffectPass>().End();
                Manager.OrdinaryEffect.End();
            }
        }
    }
}

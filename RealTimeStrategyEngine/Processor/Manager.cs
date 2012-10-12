using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RealTimeStrategyEngine.Processor
{
    class Manager
    {
        #region Drawing Items
        public static SpriteBatch spriteBatch;
        public static SpriteFont Kootenay16;
        public static string DebugText = "";

        /// <summary>
        /// For any complex models, this corrects the changes the SpriteBatch makes to the MyGame.graphicsDevice
        /// Prevents triangles in the back from being drawn in the front
        /// </summary>
        public static void ResetFor3D()
        {
            MyGame.graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            MyGame.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            MyGame.graphics.GraphicsDevice.RenderState.AlphaTestEnable = true;
        }
        /// <summary>
        /// Makes 3D drawing behave like 2D drawing
        /// </summary>
        public static void ResetFor2D()
        {
            MyGame.graphics.GraphicsDevice.RenderState.DepthBufferEnable = false;
            MyGame.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            MyGame.graphics.GraphicsDevice.RenderState.AlphaTestEnable = true;
        }

        public static Cursor cursor = new Cursor();
        public static Rectangle GameWindow;

        public static Vector3 CameraFocus;
        public static Vector3 CameraLocation;
        public static Vector3 CameraUp;
        public static Matrix View;
        public static Matrix Projection;

        public static KeyboardState PreviousKeyboard;
        public static MouseState PreviousMouse;

        public static Effect OrdinaryEffect;
        public static Effect TexturedEffect;
        public static Effect PostProcessEffect;

        public static Texture2D BlankWhiteTexture;
        Vector3 ClickPoint = Vector3.Zero;
        #endregion

        public static Terrain terrain;
        public static List<Controller> controllers;
        public static double GeneralSpawnTimer = 0.3;

        ScrollingBar DefaultHP;
        ScrollingBar DefaultDamage;
        ScrollingBar DefaultSpeed;
        ScrollingBar DefaultRange;
        ScrollingBar ProcessingGoal;

        public Vector3 MouseProjection = new Vector3();

        public Manager(SpriteBatch spritebatch)
        {
            #region Drawing Initialization
            Manager.GameWindow = new Rectangle(0, 0, MyGame.graphics.PreferredBackBufferWidth, MyGame.graphics.PreferredBackBufferHeight);
            Manager.spriteBatch = spritebatch;

            Kootenay16 = MyGame.content.Load<SpriteFont>("Kootenay16");

            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4
                , MyGame.graphics.GraphicsDevice.Viewport.AspectRatio
                , 0.001f
                , 1000.0f);
            CameraFocus = Vector3.Zero;
            CameraLocation = 1000.0f * Vector3.One;
            CameraUp = Vector3.UnitY;
            View = Matrix.CreateLookAt(
                CameraFocus + CameraLocation
                , CameraFocus
                , CameraUp);

            PreviousKeyboard = Keyboard.GetState();
            PreviousMouse = Mouse.GetState();

            OrdinaryEffect = MyGame.content.Load<Effect>("Effects\\Ordinary");
            TexturedEffect = MyGame.content.Load<Effect>("Effects\\Textured");
            PostProcessEffect = MyGame.content.Load<Effect>("Effects\\PerlinNoise");

            BlankWhiteTexture = MyGame.content.Load<Texture2D>("Textures\\BlankBox");
            #endregion

            //Initialize the terrain
            terrain = new Terrain(50.0f + 50.0f * (float)MyGame.random.NextDouble());
            CameraFocus = new Vector3(terrain.GridWidth * terrain.Scale / 2, 0.0f, terrain.GridHeight * terrain.Scale / 2);
            ClickPoint = CameraFocus;

            //Initialize the unit controllers
            int NumControllers = MyGame.random.Next(5, 10);
            controllers = new List<Controller>();
            for (int i = 0; i < NumControllers; i++)
            {
                controllers.Add(new Controller());
            }

            //Initialize the scrolling bars
            DefaultHP = new ScrollingBar(new Rectangle(GameWindow.Width - 205, 5, 200, 25), "Unit HP", 50.0, 500.0, 100.0);
            DefaultDamage = new ScrollingBar(new Rectangle(GameWindow.Width - 205, 35, 200, 25), "Unit Damage", 1.0, 10.0, 2.0);
            DefaultSpeed = new ScrollingBar(new Rectangle(GameWindow.Width - 205, 65, 200, 25), "Unit Speed", 0.0, 100.0, 25.0);
            DefaultRange = new ScrollingBar(new Rectangle(GameWindow.Width - 205, 95, 200, 25), "Unit Range", 100.0, 2500.0, 500.0);
            ProcessingGoal = new ScrollingBar(new Rectangle(GameWindow.Width - 205, 125, 200, 25), "Allowable Lag", 2.5, 7.5, 5.0);

            //Initialize the statistics tracker
            LatestStatistics = new SpawnRateController.DataPoint();
        }

        int IterationCounter = 0;
        SpawnRateController.DataPoint Statistics;
        public static SpawnRateController.DataPoint LatestStatistics;
        public void Update(GameTime gameTime)
        {
            if (IterationCounter == 0)
            {
                Statistics = new SpawnRateController.DataPoint();
            }
            //Print out the Machine Learning stats
            GeneralSpawnTimer = (1.0 - gameTime.ElapsedGameTime.TotalSeconds) * GeneralSpawnTimer
                + gameTime.ElapsedGameTime.TotalSeconds * MyGame.rateController.RecommendedGeneralSpawnTimer;
            DebugText = "Spawn Rate: " + Math.Round(controllers.Count / GeneralSpawnTimer, 2) + " Units per Second";

            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            UpdateCamera(gameTime, keyboard, mouse);
            cursor.Update(mouse);
            //Update the scrolling bars for Units
            Unit.DefaultHP = DefaultHP.Update(mouse);
            Unit.DefaultDamage = DefaultDamage.Update(mouse);
            Unit.DefaultSpeed = (float)DefaultSpeed.Update(mouse);
            Unit.DefaultRange = (float)DefaultRange.Update(mouse);
            SpawnRateController.OptimalProcessingTime = ProcessingGoal.Update(mouse);

            terrain.CalculateMousePosition();
            DateTime StartTime = DateTime.Now;
            int UnitCount = 0;
            for (int i = 0; i < controllers.Count; i++)
            {
                controllers[i].Update(gameTime, controllers, Statistics);
                UnitCount += controllers[i].units.Count;
            }
            CalculateStatistics(UnitCount);
            Statistics.ProcessingTime += DateTime.Now.Subtract(StartTime).Milliseconds;
            DebugText += "\nTotal Units: " + UnitCount;
            if (MyGame.rateController.FittingError > 0)
            {
                DebugText += "\nFitting Error: " + Math.Round(MyGame.rateController.FittingError, 4);
                if (MyGame.rateController.TestError > 0)
                {
                    DebugText += "\nTest Error: " + Math.Round(MyGame.rateController.TestError, 4);
                }
                else
                {
                    DebugText += "\nNo Test Set currently";
                }
            }
            else
            {
                DebugText += "\nGathering Data..." + Math.Abs(MyGame.rateController.FittingError * 100) + "%";
            }

            PreviousKeyboard = keyboard;
            PreviousMouse = mouse;

            IterationCounter++;
            if (IterationCounter >= 100)
            {
                IterationCounter = 0;
                MyGame.rateController.AddDataToQueue(Statistics, 100);
            }
        }
        void CalculateStatistics(int UnitCount)
        {
            LatestStatistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.UnitCount] = UnitCount;
            LatestStatistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.UnitCountSquare] = Math.Pow(UnitCount, 2);
            Statistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.UnitCount] += UnitCount;
            Statistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.UnitCountSquare] += Math.Pow(UnitCount, 2);
            if (UnitCount > 0)
            {
                double HPAverage = 0.0;
                double DmgAverage = 0.0;
                double SpeedAverage = 0.0;
                double RangeAverage = 0.0;
                for (int i = 0; i < controllers.Count; i++)
                {
                    for (int j = 0; j < controllers[i].units.Count; j++)
                    {
                        HPAverage += controllers[i].units[j].HitPoints;
                        DmgAverage += controllers[i].units[j].Damage;
                        SpeedAverage += controllers[i].units[j].Speed;
                        RangeAverage += controllers[i].units[j].Range;
                    }
                }
                LatestStatistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.HPAverage] = (HPAverage /= UnitCount);
                LatestStatistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.DmgAverage] = (DmgAverage /= UnitCount);
                LatestStatistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.SpeedAverage] = (SpeedAverage /= UnitCount);
                LatestStatistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.RangeAverage] = (RangeAverage /= UnitCount);
                Statistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.HPAverage] += HPAverage;
                Statistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.DmgAverage] += DmgAverage;
                Statistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.SpeedAverage] += SpeedAverage;
                Statistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.RangeAverage] += RangeAverage;

                double HPVariance = 0.0;
                double DmgVariance = 0.0;
                double SpeedVariance = 0.0;
                double RangeVariance = 0.0;
                for (int i = 0; i < controllers.Count; i++)
                {
                    for (int j = 0; j < controllers[i].units.Count; j++)
                    {
                        HPVariance += Math.Pow(controllers[i].units[j].HitPoints - HPAverage, 2);
                        DmgVariance += Math.Pow(controllers[i].units[j].Damage - DmgAverage, 2);
                        SpeedVariance += Math.Pow(controllers[i].units[j].Speed - SpeedAverage, 2);
                        RangeVariance += Math.Pow(controllers[i].units[j].Range - RangeAverage, 2);
                    }
                }
                HPVariance = Math.Sqrt(HPVariance / UnitCount);
                DmgVariance = Math.Sqrt(DmgVariance / UnitCount);
                SpeedVariance = Math.Sqrt(SpeedVariance / UnitCount);
                RangeVariance = Math.Sqrt(RangeVariance / UnitCount);
                LatestStatistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.HPVariance] = HPVariance;
                LatestStatistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.DmgVariance] = DmgVariance;
                LatestStatistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.SpeedVariance] = SpeedVariance;
                LatestStatistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.RangeVariance] = RangeVariance;
                Statistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.HPVariance] += HPVariance;
                Statistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.DmgVariance] += DmgVariance;
                Statistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.SpeedVariance] += SpeedVariance;
                Statistics.EnumeratedData[(int)SpawnRateController.DataPoint.Type.RangeVariance] += RangeVariance;
            }
        }

        void UpdateCamera(GameTime gameTime, KeyboardState keyboard, MouseState mouse)
        {
            //Find the three spatial directions
            Vector3 forward = Vector3.Normalize(-CameraLocation);
            Vector3 side = Vector3.Normalize(Vector3.Cross(forward, CameraUp));
            CameraUp = Vector3.Normalize(Vector3.Cross(side, forward));
            float panSpeed = 0.75f * CameraLocation.Length();
            //The arrow pad rotates the screen 
            if (keyboard.IsKeyDown(Keys.Up))
            {
                CameraLocation = Vector3.Transform(CameraLocation
                    , Matrix.CreateFromAxisAngle(side, 0.75f * (float)gameTime.ElapsedGameTime.TotalSeconds));
            }
            if (keyboard.IsKeyDown(Keys.Down))
            {
                CameraLocation = Vector3.Transform(CameraLocation
                    , Matrix.CreateFromAxisAngle(side, -0.75f * (float)gameTime.ElapsedGameTime.TotalSeconds));
            }
            float endLength = CameraLocation.Length();
            endLength += (PreviousMouse.ScrollWheelValue - mouse.ScrollWheelValue)
                 * panSpeed / 8.0f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //Page up/down and the mouse wheel controls zoom level
            if (keyboard.IsKeyDown(Keys.PageUp))
            {
                endLength -= panSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (keyboard.IsKeyDown(Keys.PageDown))
            {
                endLength += panSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            //For large CameraFocus, CameraLocation is equivalent to zero, leading to wierd camera movements
            float SigFigAdjust = CameraFocus.Length() + 1.0f;
            if (endLength < SigFigAdjust * 0.001f)
            {
                endLength = SigFigAdjust * 0.001f;
            }
            CameraLocation =
                Vector3.Normalize(CameraLocation)
                * endLength;
            if (endLength < 1.0f)
            {
                endLength = 1.0f;
            }
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4
                , MyGame.graphics.GraphicsDevice.Viewport.AspectRatio
                , endLength * 0.0005f
                , endLength * 2500.0f);
            //WASD controls panning
            if (keyboard.IsKeyDown(Keys.Left))
            {
                CameraLocation = Vector3.Transform(CameraLocation
                    , Matrix.CreateFromAxisAngle(CameraUp, 0.75f * (float)gameTime.ElapsedGameTime.TotalSeconds));
            }
            if (keyboard.IsKeyDown(Keys.Right))
            {
                CameraLocation = Vector3.Transform(CameraLocation
                    , Matrix.CreateFromAxisAngle(CameraUp, -0.75f * (float)gameTime.ElapsedGameTime.TotalSeconds));
            }
            if (cursor.isOnTerrain
                && mouse.LeftButton == ButtonState.Pressed
                && PreviousMouse.LeftButton == ButtonState.Released)
            {
                ClickPoint = cursor.TerrainIntersection;
            }
            CameraFocus = Vector3.Lerp(CameraFocus, ClickPoint, (float)gameTime.ElapsedGameTime.TotalSeconds);
            CameraUp = Vector3.Normalize(Vector3.Lerp(CameraUp, Vector3.UnitY, 8.0f * (float)gameTime.ElapsedGameTime.TotalSeconds));
            //Recalculate the camera
            View = Matrix.CreateLookAt(
                CameraFocus + CameraLocation
                , CameraFocus
                , CameraUp);
        }

        public void Draw(GameTime gameTime)
        {
            //Set the camera matrix for all the effects
            Matrix ViewXProjection = View * Projection;
            OrdinaryEffect.Parameters["ViewXProjection"].SetValue(ViewXProjection);
            TexturedEffect.Parameters["ViewXProjection"].SetValue(ViewXProjection);

            spriteBatch.Begin();
            spriteBatch.Draw(BlankWhiteTexture, Vector2.Zero, Color.TransparentWhite);
            spriteBatch.End();

            //Sort objects according to distance from camera
            BoundingFrustum VisibleArea = new BoundingFrustum(ViewXProjection);

            terrain.Draw();
            for (int i = 0; i < controllers.Count; i++)
            {
                controllers[i].Draw(gameTime, VisibleArea);
            }
            //Draw the scrolling bars for Units
            DefaultHP.Draw();
            DefaultDamage.Draw();
            DefaultSpeed.Draw();
            DefaultRange.Draw();
            ProcessingGoal.Draw();

            //Draw a five pixel black border around the edge
            spriteBatch.Begin();
            spriteBatch.Draw(BlankWhiteTexture
                , new Rectangle(0, 0, 5, GameWindow.Height)
                , Color.Black);
            spriteBatch.Draw(BlankWhiteTexture
                , new Rectangle(GameWindow.Width - 5, 0, 5, GameWindow.Height)
                , Color.Black);
            spriteBatch.Draw(BlankWhiteTexture
                , new Rectangle(0, 0, GameWindow.Width, 5)
                , Color.Black);
            spriteBatch.Draw(BlankWhiteTexture
                , new Rectangle(0, GameWindow.Height - 5, GameWindow.Width, 5)
                , Color.Black);
            spriteBatch.DrawString(Kootenay16, DebugText, Vector2.One * 5.0f, Color.White);
            spriteBatch.End();

            cursor.Draw();
        }

        /// <summary>
        /// Provides the transformation matrix from one normal to another
        /// </summary>
        /// <param name="objectNormal">The starting normal of the object</param>
        /// <param name="desiredNormal">The resulting normal after rotation</param>
        public static Matrix GetRotationFromNormal(Vector3 objectNormal, Vector3 desiredNormal)
        {
            objectNormal.Normalize();
            desiredNormal.Normalize();
            Vector3 axis = Vector3.Cross(objectNormal
                , desiredNormal);
            axis.Normalize();
            float dotAngle = Vector3.Dot(objectNormal
                , desiredNormal);
            float angle = 0f;
            if (dotAngle < 0)
            {
                angle = MathHelper.PiOver2 + (float)Math.Asin(Math.Abs(dotAngle));
            }
            else
            {
                angle = (float)Math.Acos(dotAngle);
            }
            if (float.IsNaN(angle))
            {
                return Matrix.Identity;
            }
            else
            {
                return Matrix.CreateFromAxisAngle(axis, angle);
            }

        }

        /// <summary>
        /// Generates a normalized vector of random direction
        /// </summary>
        public static Vector3 GetRandomNormal()
        {
            return Vector3.Normalize(new Vector3(
                   0.5f - (float)MyGame.random.NextDouble()
                   , 0.5f - (float)MyGame.random.NextDouble()
                   , 0.5f - (float)MyGame.random.NextDouble()));
        }

        /// <summary>
        /// Generates a color with random RGB components
        /// </summary>
        public static Color GetRandomColor(bool modulateAlpha)
        {
            Color color = new Color((float)MyGame.random.NextDouble()
                , (float)MyGame.random.NextDouble()
                , (float)MyGame.random.NextDouble());
            if (modulateAlpha)
            {
                color.A = (byte)MyGame.random.Next(byte.MaxValue);
            }
            return color;
        }

        /// <summary>
        /// Returns a custom texture of Perlin Noise
        /// </summary>
        public static Texture2D GeneratePerlinNoise(int TextureWidth, int TextureHeight
            , Color[] NoiseData, int NoiseWidth, int NoiseHeight, float NoiseShift, float Sharpness)
        {
            RenderTarget2D renderTarget = new RenderTarget2D(MyGame.graphics.GraphicsDevice
                , TextureWidth
                , TextureHeight
                , 1
                , MyGame.graphics.GraphicsDevice.DisplayMode.Format);
            MyGame.graphics.GraphicsDevice.SetRenderTarget(0, renderTarget);
            DepthStencilBuffer OriginalBuffer = MyGame.graphics.GraphicsDevice.DepthStencilBuffer;
            MyGame.graphics.GraphicsDevice.DepthStencilBuffer = new DepthStencilBuffer(MyGame.graphics.GraphicsDevice
                , renderTarget.Width
                , renderTarget.Height
                , MyGame.graphics.GraphicsDevice.DepthStencilBuffer.Format);

            Texture2D noiseMap = new Texture2D(MyGame.graphics.GraphicsDevice
                , NoiseWidth
                , NoiseHeight);
            noiseMap.SetData<Color>(NoiseData);

            PostProcessEffect.CurrentTechnique = PostProcessEffect.Techniques["GenerateNoise"];
            PostProcessEffect.Parameters["InputTexture"].SetValue(noiseMap);
            PostProcessEffect.Parameters["NoiseShift"].SetValue(NoiseShift);
            PostProcessEffect.Parameters["Sharpness"].SetValue(Sharpness);
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            PostProcessEffect.Begin();
            PostProcessEffect.CurrentTechnique.Passes.First<EffectPass>().Begin();
            spriteBatch.Draw(noiseMap
                , new Rectangle(0, 0, renderTarget.Width, renderTarget.Height)
                , Color.White);
            PostProcessEffect.CurrentTechnique.Passes.First<EffectPass>().End();
            PostProcessEffect.End();
            spriteBatch.End();

            MyGame.graphics.GraphicsDevice.SetRenderTarget(0, null);
            MyGame.graphics.GraphicsDevice.DepthStencilBuffer = OriginalBuffer;
            MyGame.graphics.GraphicsDevice.Clear(Color.Black);

            return renderTarget.GetTexture();
        }
        /// <summary>
        /// Returns a 1000 by 1000 pixel texture of Perlin Noise
        /// </summary>
        /// <param name="resolution">The amount of static to generate the noise from</param>
        public static Texture2D GeneratePerlinNoise(int Resolution)
        {
            return GeneratePerlinNoise(1000, 1000
                , GenerateStaticNoise(Resolution, Resolution), Resolution, Resolution
                , (float)MyGame.random.NextDouble(), 0.9f + 0.1f * (float)MyGame.random.NextDouble());
        }

        /// <summary>
        /// Returns a block of bright-ish colors
        /// </summary>
        public static Color[] GenerateStaticNoise(int horizontalResolution, int verticalResolution)
        {
            Color[] noiseData = new Color[horizontalResolution * verticalResolution];
            for (int x = 0; x < horizontalResolution; x++)
            {
                for (int y = 0; y < verticalResolution; y++)
                {
                    noiseData[y * horizontalResolution + x] = GetRandomColor(false);
                }
            }
            return noiseData;
        }
    }
}
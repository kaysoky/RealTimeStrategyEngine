using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

/*To Do List;
 * Implement different Unit AI/Types
 * Insert static units/buildings
 */
namespace RealTimeStrategyEngine
{
    public class MyGame : Microsoft.Xna.Framework.Game
    {
        public static GraphicsDeviceManager graphics;
        public static ContentManager content;
        public static Random random;

        Processor.Manager processorManager;
        public static Processor.SpawnRateController rateController;

        public MyGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            content = this.Content;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.IsFullScreen = false;  //For debug purposes, full screen is off
            graphics.ApplyChanges();
            Window.AllowUserResizing = false;
            IsMouseVisible = false;

            random = new Random();

            processorManager = new Processor.Manager(new SpriteBatch(graphics.GraphicsDevice));
            rateController = new Processor.SpawnRateController();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(rateController.ProcessData), null);
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            processorManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            processorManager.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}

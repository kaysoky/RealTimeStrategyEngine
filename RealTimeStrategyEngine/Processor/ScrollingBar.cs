using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RealTimeStrategyEngine.Processor
{
    class ScrollingBar
    {
        Rectangle Area;
        string Label;
        Vector2 LabelPosition;
        double Minimum;
        double Range;
        public double Value;

        public ScrollingBar(Rectangle Position, string Label, double Minimum, double Maximum, double StartingValue)
        {
            Area = Position;
            this.Label = Label;
            LabelPosition = new Vector2(Area.X + Area.Width / 2.0f, Area.Y + Area.Height / 2.0f) - 0.5f * Manager.Kootenay16.MeasureString(Label);
            this.Minimum = Minimum;
            this.Range = Maximum - Minimum;
            this.Value = StartingValue;
        }

        public double Update(MouseState mouse)
        {
            Point MousePoint = new Point(mouse.X, mouse.Y);
            if (Area.Contains(MousePoint))
            {
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    Value = Minimum + (double)(mouse.X - Area.X) / Area.Width * Range;
                }
            }
            return Value;
        }

        public void Draw()
        {
            Manager.spriteBatch.Begin();
            Manager.spriteBatch.Draw(Manager.BlankWhiteTexture, Area, Color.White);
            Manager.spriteBatch.Draw(Manager.BlankWhiteTexture
                , new Rectangle(Area.X + 1, Area.Y + 1, Area.Width - 2, Area.Height - 2)
                , Color.Black);
            Manager.spriteBatch.Draw(Manager.BlankWhiteTexture
                , new Rectangle(Area.X, Area.Y + 1, (int)(Area.Width * (Value - Minimum) / Range), Area.Height - 2)
                , new Color(Color.Blue, 0.5f));
            Manager.spriteBatch.DrawString(Manager.Kootenay16, Label, LabelPosition, Color.LightGray);
            Manager.spriteBatch.End();
        }
    }
}

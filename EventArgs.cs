using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace RJGL
{
    [Flags]
    public enum MouseButtons
    {
        None=0,
        Left,
        Right,
        Middle,
        XButtonOne,
        XButtonTwo
    }
    //https://stackoverflow.com/a/65056572/15755351
    public class EventArgs_Draw : EventArgs
    {
        public SKRect Bounds { get; }
        public SKCanvas Canvas { get; }

        public EventArgs_Draw(SKCanvas canvas, SKRect bounds)
        {
            Canvas = canvas;
            Bounds = bounds;
        }
    }
    public class EventArgs_Click : EventArgs
    {
        public SKPoint Position { get; }
        public MouseButtons Button { get; }

        public EventArgs_Click(SKPoint pos, MouseButtons button)
        {
            Position = pos;
            Button = button;
        }
    }
    public class EventArgs_MouseMove : EventArgs
    {
        public SKPoint Position { get; }

        public EventArgs_MouseMove(SKPoint pos)
        {
            Position = pos;
        }
    }
    public class EventArgs_Scroll : EventArgs
    {
        public SKPoint Position { get; }
        public int Clicks { get; }
        public EventArgs_Scroll(SKPoint pos, int clicks)
        {
            Position = pos;
            Clicks = clicks;
        }
    }
    public class EventArgs_KeyDown : EventArgs
    {
        public int KeyCode { get; }
        public bool Shift { get; }
        public bool Ctrl { get; }
        public bool Alt { get; }
        public EventArgs_KeyDown(int keyCode, bool shift, bool ctrl, bool alt)
        {
            KeyCode = keyCode;
            Shift = shift;
            Ctrl = ctrl;
            Alt = alt;
        }
    }
}

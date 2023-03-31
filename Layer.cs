using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RJGL
{
    //highly modified version of this
    //https://stackoverflow.com/a/65056572/15755351
    public class Layer
    {
        public event EventHandler<EventArgs_Draw>? Draw;
        // The finished recording
        private SKPicture? picture = null;
        private bool IsValid = false;

        internal bool isMouseInLayer = false;

        // Raises the Draw event and records any drawing commands to an SKPicture for later playback.
        // This can be called from any thread.
        public void Render(SKRect clippingBounds)
        {
            // Only redraw the Layer if it has been invalidated
            if (!IsValid)
            {
                // Create an SKPictureRecorder to record the Canvas Draw commands to an SKPicture
                using (var recorder = new SKPictureRecorder())
                {
                    // Start recording 
                    recorder.BeginRecording(clippingBounds);

                    // Dispose of any previous Pictures
                    picture?.Dispose();

                    // Raise the Draw event.  The subscriber can then draw on the Canvas provided in the event
                    // and the commands will be recorded for later playback.
                    OnDraw(new EventArgs_Draw(recorder.RecordingCanvas, clippingBounds));

                    // Create a new SKPicture with recorded Draw commands 
                    picture = recorder.EndRecording();

                    RenderCount++;

                    IsValid = true;
                }
            }
        }

        public Layer(){}


        // Gets the number of times that this Layer has been rendered

        public int RenderCount { get; private set; }

        // Paints the previously recorded SKPicture to the provided skglControlCanvas.  This basically plays 
        // back the draw commands from the last Render.  This should be called from the SKGLControl.PaintSurface
        // event using the GUI thread.

        public void Paint(SKCanvas skglControlCanvas)
        {
            if (picture != null)
            {
                // Play back the previously recorded Draw commands to the skglControlCanvas using the GUI thread
                try { skglControlCanvas.DrawPicture(picture); }
                catch (AccessViolationException)
                { Console.WriteLine("AccessViolationException"); }

                PaintCount++;
            }
        }

        // Gets the number of times that this Layer has been painted
        public int PaintCount { get; private set; }

        // Forces the Layer to be redrawn with the next rendering cycle
        public void Invalidate()
        {
            IsValid = false;
        }

        public virtual bool IsInLayer(SKPoint p)
        {
            return true;
        }
        public delegate void MouseDownEventHandler(object sender, EventArgs_Click e);
        public event MouseDownEventHandler MouseDown;
        public virtual bool OnMouseDown(EventArgs_Click e)
        {
            MouseDown?.Invoke(this, e);
            // return whether the event should be stopped at this handler
            // and not propagate to the other layers
            return false;
        }
        public delegate void MouseUpEventHandler(object sender, EventArgs_Click e);
        public event MouseUpEventHandler MouseUp;
        public virtual bool OnMouseUp(EventArgs_Click e)
        {
            MouseUp?.Invoke(this, e);
            return false;
        }

        public delegate void MouseWheelEventHandler(object sender, EventArgs_Scroll e);
        public event MouseWheelEventHandler MouseWheel;
        public virtual bool OnMouseWheel(EventArgs_Scroll e)
        {
            MouseWheel?.Invoke(this, e);
            return false;
        }
        public delegate void MouseMoveEventHandler(object sender, EventArgs_MouseMove e);
        public event MouseMoveEventHandler MouseMove;
        public virtual bool OnMouseMove(EventArgs_MouseMove e)
        {
            MouseMove?.Invoke(this, e);
            return false;
        }
        public delegate void MouseLeaveEventHandler(object sender, EventArgs_MouseMove e);
        public event MouseLeaveEventHandler MouseLeave;
        public virtual void OnMouseLeave(EventArgs_MouseMove e) => MouseLeave?.Invoke(this, e);
        public delegate void KeyDownEventHandler(object sender, EventArgs_KeyDown e);
        public event KeyDownEventHandler KeyDown;
        public virtual bool OnKeyDown(EventArgs_KeyDown e)
        {
            KeyDown?.Invoke(this, e);
            return false;
        }
        public virtual void OnDraw(EventArgs_Draw e)
        {
            Draw?.Invoke(this, e);
        }
    }
}

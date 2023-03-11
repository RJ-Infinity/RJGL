using System;
using System.ComponentModel;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using WinForms = System.Windows.Forms;

namespace RJGL
{
    public class RJGLForm : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer? components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.SkiaSurface = new RJGLSKGLControl();
            this.SuspendLayout();
            // 
            // skglControl1
            // 
            SkiaSurface.BackColor = Color.Black;
            SkiaSurface.Dock = DockStyle.Fill;
            SkiaSurface.Location = new Point(0, 0);
            SkiaSurface.Margin = new Padding(4, 3, 4, 3);
            SkiaSurface.Name = "skglControl1";
            SkiaSurface.Size = new Size(800, 450);
            SkiaSurface.TabIndex = 0;
            SkiaSurface.VSync = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(SkiaSurface);
            ResumeLayout(false);
        }
        public RJGLSKGLControl SkiaSurface { get; set; }

        public List<Layer> Layers = new();

        private Thread RenderThread;
        private AutoResetEvent ThreadGate;
        private bool alive = true;
#pragma warning disable CS8618
        public RJGLForm()
        {
            InitializeComponent();
            //Console.WriteLine(SkiaSurface.ParentForm == this);

            //SkiaSurface.MouseDown += SkiaSurface_MouseDown;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Subscribe to the SKGLControl events
            SkiaSurface.PaintSurface += SkglControl1_PaintSurface;
            SkiaSurface.Resize += SkglControl1_Resize;

            SkiaSurface.MouseDown += SkglControl1_MouseDown;
            SkiaSurface.MouseMove += SkglControl1_MouseMove;
            SkiaSurface.MouseUp += SkglControl1_MouseUp;
            SkiaSurface.MouseWheel += SkglControl1_MouseWheel;
            SkiaSurface.KeyDown += SkiaSurface_KeyDown; ;


            // Create a background rendering thread
            RenderThread = new Thread(RenderLoopMethod);
            ThreadGate = new AutoResetEvent(false);

            // Start the rendering thread
            RenderThread.Start();

        }

        private void SkglControl1_MouseDown(object? sender, MouseEventArgs e)=>OnMouseDown(e);
        private void SkglControl1_MouseMove(object? sender, MouseEventArgs e)=>OnMouseMove(e);
        private void SkglControl1_MouseUp(object? sender, MouseEventArgs e)=>OnMouseUp(e);
        private void SkglControl1_MouseWheel(object? sender, MouseEventArgs e)=>OnMouseWheel(e);
        private void SkiaSurface_KeyDown(object? sender, KeyEventArgs e) => OnKeyDown(e);

        protected override void OnClosing(CancelEventArgs e)
        {
            // Let the rendering thread terminate
            alive = false;
            ThreadGate.Set();

            base.OnClosing(e);
        }
        private void SkglControl1_PaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
        {
            // Clear the Canvas
            e.Surface.Canvas.Clear(SKColors.Black);

            // Paint each pre-rendered layer onto the Canvas using this GUI thread
            foreach (Layer layer in Layers)
            {
                layer.Paint(e.Surface.Canvas);
            }


            //using (SKPaint paint = new SKPaint())
            //{
            //    paint.Color = SKColors.LimeGreen;

                //for (int i = 0; i < Layers.Count; i++)
                //{
                //    Layer layer = Layers[i];
                //    string text = $"{layer.Title} - Renders = {layer.RenderCount}, Paints = {layer.PaintCount}";
                //    SKPoint textLoc = new SKPoint(10, 10 + (i * 15));

                //    e.Surface.Canvas.DrawText(text, textLoc, paint);
                //}

            //    paint.Color = SKColors.Cyan;
            //}
        }
        private void InverseLayerLoop(Func<Layer,bool> callback)
        {
            for (int i = Layers.Count - 1; i >= 0; i--)
            {if (callback(Layers[i])){break;}}
        }
        private MouseButtons ConvertMouseButtons(WinForms.MouseButtons b) => b switch
        {
            WinForms.MouseButtons.Left => RJGL.MouseButtons.Left,
            WinForms.MouseButtons.Right => RJGL.MouseButtons.Right,
            WinForms.MouseButtons.Middle => RJGL.MouseButtons.Middle,
            WinForms.MouseButtons.XButton1 => RJGL.MouseButtons.XButtonOne,
            WinForms.MouseButtons.XButton2 => RJGL.MouseButtons.XButtonTwo,
            _ =>throw new InvalidEnumArgumentException(nameof(b))
        };
        protected override void OnMouseDown(MouseEventArgs e)
        {
            SKPoint point = new SKPoint(e.X, e.Y);
            InverseLayerLoop(
                // if the point is in the layer the mouse down event is run
                (Layer l) => l.IsInLayer(point) &&
                // if the mouseDown event returns true we stop the recursion
                l.OnMouseDown(new EventArgs_Click(point, ConvertMouseButtons(e.Button)))
            );
            base.OnMouseDown(e);
            UpdateDrawing();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            SKPoint point = new SKPoint(e.X, e.Y);
            InverseLayerLoop(
                (Layer l) => l.IsInLayer(point) &&
                l.OnMouseUp(new EventArgs_Click(point, ConvertMouseButtons(e.Button)))
            );
            base.OnMouseUp(e);
            UpdateDrawing();
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            SKPoint point = new SKPoint(e.X, e.Y);
            InverseLayerLoop(
                (Layer l) => l.IsInLayer(point) &&
                l.OnMouseWheel(new EventArgs_Scroll(point, e.Delta / SystemInformation.MouseWheelScrollDelta))
            );
            base.OnMouseWheel(e);
            UpdateDrawing();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            SKPoint point = new SKPoint(e.X, e.Y);
            InverseLayerLoop((Layer l) => {
                bool inLayer = l.IsInLayer(point);
                if (l.isMouseInLayer && !inLayer)
                {l.OnMouseLeave(new EventArgs_MouseMove(point));}
                l.isMouseInLayer = inLayer;
                return false;
            });
            InverseLayerLoop(
                (Layer l) => l.isMouseInLayer &&
                l.OnMouseMove(new EventArgs_MouseMove(point))
            );
            base.OnMouseMove(e);
            UpdateDrawing();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            InverseLayerLoop((Layer l) => l.OnKeyDown(new EventArgs_KeyDown(e.KeyValue, e.Shift, e.Control, e.Alt)));
            base.OnKeyDown(e);
            UpdateDrawing();
        }
        private void SkglControl1_Resize(object? sender, EventArgs e)
        {
            // Invalidate all of the Layers
            foreach (Layer layer in Layers) { layer.Invalidate(); }

            // Start a new rendering cycle to redraw all of the layers.
            UpdateDrawing();
        }
        public virtual void UpdateDrawing()
        {
            // Unblock the rendering thread to begin a render cycle.  Only the invalidated
            // Layers will be re-rendered, but all will be repainted onto the SKGLControl.
            ThreadGate.Set();
        }
        private void RenderLoopMethod()
        {
            while (alive)
            {
                // Draw any invalidated layers using this Render thread
                DrawLayers();

                // Invalidate the SKGLControl to run the PaintSurface event on the GUI thread
                // The PaintSurface event will Paint the layer stack to the SKGLControl
                SkiaSurface.Invalidate();

                // DoEvents to ensure that the GUI has time to process
                Application.DoEvents();

                // Block and wait for the next rendering cycle
                ThreadGate.WaitOne();
            }
        }
        private void DrawLayers()
        {
            // Iterate through the collection of layers and raise the Draw event for each layer that is
            // invalidated.  Each event handler will receive a Canvas to draw on along with the Bounds for 
            // the Canvas, and can then draw the contents of that layer. The Draw commands are recorded and  
            // stored in an SKPicture for later playback to the SKGLControl.  This method can be called from
            // any thread.

            SKRect clippingBounds = SkiaSurface.ClientRectangle.ToSKRect();

            foreach (Layer layer in Layers)
            {
                layer.Render(clippingBounds);
            }
        }
    }
}
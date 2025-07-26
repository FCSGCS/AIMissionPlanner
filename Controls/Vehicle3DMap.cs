using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MissionPlanner.Controls
{
    /// <summary>
    /// Simple 3D map view showing a vehicle and optional path lines. Designed to be responsive on resize.
    /// </summary>
    public class Vehicle3DMap : GLControl
    {
        /// <summary>Current vehicle position in world coordinates.</summary>
        public Vector3 VehiclePosition { get; set; }

        /// <summary>Vehicle orientation in degrees (roll, pitch, yaw).</summary>
        public Vector3 VehicleOrientation { get; set; }

        /// <summary>Path points that will be drawn as a line strip.</summary>
        public List<Vector3> Path { get; set; } = new List<Vector3>();

        public Vehicle3DMap() : base()
        {
            this.Resize += OnResize;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color.CornflowerBlue);
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (!this.Context.IsCurrent)
                this.Context.MakeCurrent(null);
            GL.Viewport(0, 0, Width, Height);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), (float)Width / Height, 0.1f, 1000f);
            Matrix4 lookat = Matrix4.LookAt(new Vector3(0, -20, 10), Vector3.Zero, Vector3.UnitZ);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            DrawGrid();
            DrawPath();
            DrawVehicle();

            SwapBuffers();
        }

        private void DrawGrid()
        {
            GL.Disable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Gray);
            for (int i = -50; i <= 50; i += 1)
            {
                GL.Vertex3(i, -50, 0);
                GL.Vertex3(i, 50, 0);
                GL.Vertex3(-50, i, 0);
                GL.Vertex3(50, i, 0);
            }
            GL.End();
        }

        private void DrawPath()
        {
            if (Path == null || Path.Count < 2)
                return;
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.LineStrip);
            foreach (var p in Path)
                GL.Vertex3(p);
            GL.End();
        }

        private void DrawVehicle()
        {
            GL.PushMatrix();
            GL.Translate(VehiclePosition);
            GL.Rotate(VehicleOrientation.Z, 0, 0, 1); // yaw
            GL.Rotate(VehicleOrientation.Y, 1, 0, 0); // pitch
            GL.Rotate(VehicleOrientation.X, 0, 1, 0); // roll

            GL.Color3(Color.Yellow);
            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(0, 1, 0);
            GL.Vertex3(-0.5f, -1, 0.5f);
            GL.Vertex3(0.5f, -1, 0.5f);

            GL.Vertex3(0, 1, 0);
            GL.Vertex3(0.5f, -1, 0.5f);
            GL.Vertex3(0.5f, -1, -0.5f);

            GL.Vertex3(0, 1, 0);
            GL.Vertex3(0.5f, -1, -0.5f);
            GL.Vertex3(-0.5f, -1, -0.5f);

            GL.Vertex3(0, 1, 0);
            GL.Vertex3(-0.5f, -1, -0.5f);
            GL.Vertex3(-0.5f, -1, 0.5f);
            GL.End();
            GL.PopMatrix();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobustGeometry.HalfedgeMesh;
using RobustGeometry.Wpf.HalfedgeMesh;
// using Xceed.Wpf.Toolkit.Zoombox;

namespace RobustGeometry.Test.HalfedgeMesh.WpfUI
{
    public class PointTriangleMeshCanvas : Canvas
    {
        PointTriangleMesh _mesh;
        public PointTriangleMesh Mesh 
        { 
            get
            {
                return _mesh;
            }
            set
            {
                _mesh = value;
                Children.Clear();
                AddMeshElements();
            }
        }

        public PointTriangleMeshCanvas()
        {
            var translate = new TranslateTransform();
            translate.X = 10; // Should bind to ActualWidth.X * 0.5
            translate.Y = 10; // Should bind to ActualWidth.Y * 0.5
            var scale = new ScaleTransform();
            scale.ScaleX = 1;
            scale.ScaleY = -1;
            scale.CenterX = translate.X;
            scale.CenterY = translate.Y;
            var tfg = new TransformGroup();

            tfg.Children.Add(translate);
            tfg.Children.Add(scale);

            RenderTransform = tfg;
        }

        void AddMeshElements()
        {
            foreach (var vertex in _mesh.Vertices)
            {
                AddVertex(vertex.Traits);
            }
            foreach (var edge in _mesh.Edges)
            {
                AddEdge(edge.Half1.Source.Traits, edge.Half1.Target.Traits);
            }
        }

        void AddVertex(IPoint point)
        {
            const double vertexRadius = 2.5;
            var el = new Ellipse();
            el.Fill = Brushes.Black;
            el.Width = vertexRadius * 2.0;
            el.Height = vertexRadius * 2.0;
            SetLeft(el, point.X - vertexRadius);
            SetTop(el, point.Y - vertexRadius);
            Children.Add(el);
        }

        void AddEdge(IPoint pointStart, IPoint pointEnd)
        {
            var line = new Line();
            line.X1 = pointStart.X;
            line.Y1 = pointStart.Y;
            line.X2 = pointEnd.X;
            line.Y2 = pointEnd.Y;
            line.StrokeThickness = 2;
            line.Stroke = Brushes.Green;
            Children.Add(line);
        }

    }

    [TestClass]
    public class PointMeshCanvasTests
    {
        [TestMethod]
        public void ShowPointMeshCanvas()
        {
            var mesh = new PointTriangleMesh();
            PointVertexTraits p0 = new Point(0, 0);
            PointVertexTraits p1 = new Point(1, 0);
            PointVertexTraits p2 = new Point(1, 1);
            PointVertexTraits p3 = new Point(0, 1);
            var f1 = mesh.CreateTriangle(p0, p1, p2);
            var v0 = f1.Vertices.Single(v => v.Traits == p0);
            var v1 = f1.Vertices.Single(v => v.Traits == p1);
            var v2 = f1.Vertices.Single(v => v.Traits == p2);
            mesh.CreateTriangle(v0.HalfedgeTo(v2), p3);

//            var zb = new Zoombox();

            var c = new PointTriangleMeshCanvas();
            c.Width = 300;
            c.Height = 300;
            c.Mesh = mesh;

//            var ch = new CanvasHost();
////            ch.Content = c;
//            var cv = new Canvas();
//            cv.Height = 300;
//            cv.Width = 300;
//            cv.Children.Add(new TextBlock() { Text = "asdasdasdasd"});
//            ch.Canvas = c;

            var w = new Window();
//             w.Content = ch;
            w.ShowDialog();

        }
    }

}

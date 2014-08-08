

namespace HelixWPF
{
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using System.Collections;
    using System.Collections.Generic;
    using HelixToolkit.Wpf;
    using System.IO;
    public class MainViewModel
    {
        List<Point3D> points;
        List<int>[] faces;
        Model3DGroup mg;
        double pointRaduis = 0.03;
        public MainViewModel()
        {
            // Create a model group
            var modelGroup = new Model3DGroup();
            points = new List<Point3D>();
            
            // Create a mesh builder and add a box to it
            var meshBuilder = new MeshBuilder(false, false);
            // -1 -1 -1 
            //1 -1 -1 
            //1 1 -1 
            //-1 1 -1 
            //-1 -1 1 
            //1 -1 1 
            //1 1 1 
            //-1 1 1 
            string path = @"teapot.ply";
            PLYReader reader = new PLYReader();

            using (var s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                mg=reader.Read(s);
            }
           
            this.points = reader.Vertices;
            this.faces = reader.Faces;
           // meshBuilder.Positions.Add(new Point3D(-1, -1, -1));//, 1, 2, 0.5);
           // meshBuilder.Positions.Add(new Point3D(1, -1, -1));
           // meshBuilder.Positions.Add(new Point3D(1, 1, -1));
           // meshBuilder.Positions.Add(new Point3D(-1, 1, -1));
           // meshBuilder.Positions.Add(new Point3D(-1, -1, 1));
           // meshBuilder.Positions.Add(new Point3D(1, -1, 1));
           // meshBuilder.Positions.Add(new Point3D(1, 1, 1));
           // meshBuilder.Positions.Add(new Point3D(-1, 1, 1));
           // meshBuilder.AddPolygon(new int[4] {0,1,2,3 });
           // meshBuilder.AddPolygon(new int[4] { 5,4,7,6 });
           // meshBuilder.AddPolygon(new int[4] { 6,2,1,5 });
           // meshBuilder.AddPolygon(new int[4] { 3,7,4,0 });
           // meshBuilder.AddPolygon(new int[4] { 7,3,2,6 });
           // meshBuilder.AddPolygon(new int[4] { 5,1,0,4 });
            
           // // Create a mesh from the builder (and freeze it)
           // var mesh = meshBuilder.ToMesh(true);

           // // Create some materials
           // var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
           // var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
           // var blueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
           // var insideMaterial = MaterialHelper.CreateMaterial(Colors.Yellow);

           // // Add 3 models to the group (using the same mesh, that's why we had to freeze it)
           // modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Material = greenMaterial, BackMaterial = insideMaterial });
           //// modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(-2, 0, 0), Material = redMaterial, BackMaterial = insideMaterial });
           //// modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(2, 0, 0), Material = blueMaterial, BackMaterial = insideMaterial });

           // // Set the property, which will be bound to the Content property of the ModelVisual3D (see MainWindow.xaml)
            this.Model = mg;
        }
        public void AddGroup(List<int> selectedVertex)
        {
            MeshBuilder mesh = new MeshBuilder(false, false);
            foreach (int index in selectedVertex)
                mesh.AddSphere(this.Points[index], pointRaduis);
            var mesh1 = mesh.ToMesh(true);
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
            var insideMaterial = MaterialHelper.CreateMaterial(Colors.Yellow);
            
            // Add 3 models to the group (using the same mesh, that's why we had to freeze it)
            mg.Children.Add(new GeometryModel3D { Geometry = mesh1, Material = redMaterial, BackMaterial = insideMaterial });
        }
        public void AddGroup(bool canShowFace)
        {
            MeshBuilder mesh = new MeshBuilder(false, false);
            if (canShowFace)
            {
                foreach (Point3D pt in this.Points)
                    mesh.Positions.Add(pt);

                foreach (List<int> pts in this.faces)
                {
                    if (pts != null)
                        mesh.AddPolygon(pts);
                }
            }
            else
            {
                foreach (Point3D pt in this.Points)
                    mesh.AddSphere(pt, pointRaduis);
            }
            var mesh1 = mesh.ToMesh(true);
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Green);
            var insideMaterial = MaterialHelper.CreateMaterial(Colors.Yellow);

            // Add 3 models to the group (using the same mesh, that's why we had to freeze it)
            mg.Children.Add(new GeometryModel3D { Geometry = mesh1, Material = redMaterial, BackMaterial = insideMaterial });
        }
       
        
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public Model3D Model { 
            get; set; 
        }
       
        public List<Point3D> Points
        {
            get { return this.points; }
        }
         public List<int>[] Faces
        {
            get
            {
                return this.faces;
            }
        }
    }
    }

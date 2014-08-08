using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit;
using System.IO;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Globalization;
using System.Windows.Threading;
using System.Windows.Media;
namespace HelixWPF
{
    public class PLYReader : HelixToolkit.Wpf.ModelReader
    {
        private int plyVersion;

        private bool isASCII;

        private List<Point3D> vertex;

        private List<int>[] faces;

        //List<int[]> faces = new List<int[]>();

        private PLYHeader header;

        private MeshBuilder mesh;
        private StreamReader Reader { get; set; }

        Model3DGroup mg;
        public PLYReader(Dispatcher dispatcher = null)
            : base(dispatcher)
        {
            header = new PLYHeader();
            mesh = new MeshBuilder(false, false);
        }

        /// <summary>
        /// Reads the model in ASCII format from the specified stream.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// True if the model was loaded successfully.
        /// </returns>
        private bool TryReadAscii(StreamReader reader)
        {
            bool result = false;
            bool endHeader = false;
            int elementIndex = 0;
            long elementCount = 0;
            this.vertex = new List<Point3D>();
            
            PLYElement currentElement = null;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line != null)
                {

                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("\0") || line.StartsWith("#") || line.StartsWith("!")
                        || line.StartsWith("$"))
                    {
                        continue;
                    }

                    string id, values;


                    if (!endHeader)
                    {
                        SplitLine(line, out id, out values);
                        string[] fields=null;
                        if (values != null)
                        {
                            fields = values.SplitOnWhitespace();
                        }
                        

                        switch (id.ToLowerInvariant())
                        {
                            case "ply":
                                this.header.isValid = true;
                                break;
                            case "format":

                                this.header.Format = fields[0];

                                break;
                            case "element":
                                if (currentElement != null)
                                {
                                    this.header.AddElement(currentElement);
                                    currentElement = null;
                                }
                                currentElement = new PLYElement();
                                currentElement.Name = fields[0];
                                currentElement.Count = long.Parse(fields[1]);
                                break;
                            case "property":
                                PLYProperty currentProperty = new PLYProperty();
                                currentProperty.Name = fields[fields.Length - 1];
                                currentProperty.DataType = fields[fields.Length - 2];
                                if (currentElement != null)
                                {
                                    currentElement.AddProperty(currentProperty);
                                }
                                break;
                            case "end_header":
                                if (currentElement != null)
                                {
                                    this.header.AddElement(currentElement);
                                    currentElement = null;
                                }
                                endHeader = true;
                                break;
                        }
                    }
                    else
                    {
                        if (header.Elements.Count > elementIndex)
                        {
                            if (header.Elements[elementIndex].Count > elementCount)
                            {
                                switch (header.Elements[elementIndex].Name.ToLower())
                                {
                                    case "vertex":
                                        this.AddVertex(line);
                                        break;
                                    case "face":
                                        if(faces==null)
                                        {
                                            faces = new List<int>[header.Elements[elementIndex].Count];
                                        }
                                        this.AddFace(line, elementCount);
                                        break;
                                }
                                elementCount++;
                            }
                            else
                            { 
                                elementIndex++;
                                elementCount = 0;
                            }
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Adds a vertex.
        /// </summary>
        /// <param name="values">
        /// The input values.
        /// </param>
        private void AddVertex(string values)
        {
            var fields = Split(values);
            this.vertex.Add(new Point3D(fields[0], fields[1], fields[2]));
        }

        private void AddFace(string values,long count)
        {
            string s, indices;
            SplitLine(values, out s, out indices);
            var result = GetIndices(indices);
            faces[count]=new List<int>(result);
            //this.mesh.AddPolygon(result);
        }
        private static double DoubleParse(string input)
        {
            return double.Parse(input, CultureInfo.InvariantCulture);
        }

        private static int IntParse(string input)
        {
            return int.Parse(input, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Splits the specified string using whitespace(input) as separators.
        /// </summary>
        /// <param name="input">
        /// The input string.
        /// </param>
        /// <returns>
        /// List of input.
        /// </returns>
        private static IList<double> Split(string input)
        {
            input = input.Trim();
            var fields = input.SplitOnWhitespace();
            var result = new double[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                result[i] = DoubleParse(fields[i]);
            }

            return result;
        }

        private static IList<int> GetIndices(string input)
        {
            input = input.Trim();
            var fields = input.SplitOnWhitespace();
            var result = new int[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                result[i] = IntParse(fields[i]);
            }

            return result;
        }
        /// <summary>
        /// Splits a line in keyword and arguments.
        /// </summary>
        /// <param name="line">
        /// The line.
        /// </param>
        /// <param name="keyword">
        /// The keyword.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        private static void SplitLine(string line, out string keyword, out string arguments)
        {
            int idx = line.IndexOf(' ');
            if (idx < 0)
            {
                keyword = line;
                arguments = null;
                return;
            }

            keyword = line.Substring(0, idx);
            arguments = line.Substring(idx + 1);
        }
        public override Model3DGroup Read(System.IO.Stream s)
        {
            Model3DGroup mg = new Model3DGroup();
            using (this.Reader = new StreamReader(s))
            {
                TryReadAscii(this.Reader);
            }
            // Create a mesh from the builder (and freeze it)
            var mesh1 = mesh.ToMesh(true);

            // Create some materials
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
            var blueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
            var insideMaterial = MaterialHelper.CreateMaterial(Colors.Yellow);

            // Add 3 models to the group (using the same mesh, that's why we had to freeze it)
            mg.Children.Add(new GeometryModel3D { Geometry = mesh1, Material = greenMaterial, BackMaterial = insideMaterial });

            return mg;
        }
        
        public List<Point3D> Vertices
        {
            get { return this.vertex; }
            set { }
        }
        public List<int>[] Faces
        {
            get { return this.faces; }
        }
    }
    
    public class PLYProperty
    {
        private string dataType;

        private string name;

        public bool isList=false;
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public string DataType
        {
            get { return this.dataType; }
            set { this.dataType = value; }
        }
        public bool IsList
        {
            get { return this.isList; }
            set { this.isList = value; }
        }
    }
    public class PLYElement
    {
        private string elemName;

        private long elemNumber;

        private List<PLYProperty> elemProperty;
        public PLYElement()
        {
            elemProperty=new List<PLYProperty>();
        }
        
        public string Name
        {
            get
            {
                return this.elemName;
            }
            set
            {
                this.elemName=value;
            }
        }

        public long Count
        {
            get
            {
                return this.elemNumber;
            }
            set
            {
                this.elemNumber=value;
            }
        }
        public List<PLYProperty> Property
        {
            get { return elemProperty; }
        }
        public bool AddProperty(PLYProperty propertyItem)
        {
            bool result = false;
            try
            {
                this.elemProperty.Add(propertyItem);
                result = true;
            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
    public class PLYHeader
    {
        private bool isPLYHeader=false;

        private string format;

        private double formatVersion;

        private List<PLYElement> elements;
        public PLYHeader()
        {
            elements = new List<PLYElement>();
        }

        public bool isValid
        {
            get
            {
                return isPLYHeader;
            }
            set
            {
                this.isPLYHeader = value;
            }
        }
        public string Format
        {
            get
            { return this.format; }
            set { this.format = value; }
        }
        public double Version
        {
            get { return this.formatVersion; }
            set { this.formatVersion = value; }
        }
        public bool AddElement(PLYElement elementItem)
        {
            bool result = false;
            try
            {
                elements.Add(elementItem);
                result = true;
            }
            catch(Exception ex)
            {

            }
            return result;
        }
        public List<PLYElement> Elements
        {
            get { return this.elements; }
        }
    }
}

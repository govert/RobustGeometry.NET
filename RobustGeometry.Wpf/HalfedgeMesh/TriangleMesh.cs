using System.ComponentModel;
using System.Windows;
using RobustGeometry.HalfedgeMesh;

namespace RobustGeometry.Wpf.HalfedgeMesh
{
    // CONSIDER: Make Struct?
    //           Can we measure the performance difference?
    public class PointVertexTraits : IPoint, IPointUpdate, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public Point Position { get; private set; }

        PointVertexTraits(Point point)
        {
            Position = point;
        }

        public static implicit operator PointVertexTraits(Point point)
        {
            return new PointVertexTraits(point);
        }

        public double X
        {
            get { return Position.X; }
        }

        public double Y
        {
            get { return Position.Y; }
        }

        public void UpdatePosition(double x, double y)
        {
            if (Position.X != x || Position.Y != y)
            {
                Position = new Point(x, y);
                OnPropertyChanged("Position");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class PointTriangleMesh : PointTriangleMesh<NullTraits, NullTraits, NullTraits, PointVertexTraits>
    {
    }
}

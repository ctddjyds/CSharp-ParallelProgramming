using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// This is example code that won't compile
// because it requires additional code that isn't included
namespace Listing6_10
{
    class Circle
    {
        public 
    }

    class Program
    {
        static void Main(string[] args)
        {
            
        }

        public static ComplexGeometry ExecuteStages(Point centerLocation)
        {
            var finalGeometry = new List<Geometry>();
            
            var pointsList = GeomPoint.CreatePoints();
            finalGeometry.Add(pointsList);
            var linesList = GeomLine.CreateLines(pointsList);
            finalGeometry.Add(linesList);
            var ellipse = GeomEllipse.CreateEllipse(linesList);
            finalGeometry.Add(ellipse);
            var circle = GeomCircle.CreateCircle(ellipse);
            finalGeometry.Add(circle);
            var boundingBox = GeomBoundingBox.CreateBoundingBox(ellipse);
            finalGeometry.Add(boundingBox);

            return finalGeometry;
        }
    }
}

﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Drawing;

namespace Urbbox.SlabAssembler.Core
{
    public class EntityGenerator
    {
        public static Polyline3d CreateSquare(SizeF area, double border = 0)
        {
            var pts = new Point3dCollection
            {
                new Point3d(-border, -border, 0),
                new Point3d(-border, area.Height + border, 0),
                new Point3d(area.Width + border, area.Height + border, 0),
                new Point3d(area.Width + border, -border, 0)
            };

            var polyline = new Polyline3d(Poly3dType.SimplePoly, pts, true);
            if (border > 0) polyline.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 0, 0);

            return polyline;
        }

        public static IEnumerable<Line> CreateCrossLines(SizeF size, float border = 0)
        {
            yield return new Line(
                new Point3d(size.Width / 2.0, -border, 0),
                new Point3d(size.Width / 2.0, size.Height + border, 0)
            );
            yield return new Line(
                new Point3d(-border, size.Height / 2.0, 0),
                new Point3d(size.Width + border, size.Height / 2.0, 0)
            );
        }
    }
}

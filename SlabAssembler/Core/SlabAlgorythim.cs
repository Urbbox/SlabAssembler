﻿using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;
using System;
using Autodesk.AutoCAD.Customization;
using System.Collections.Generic;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabAlgorythim
    {
        protected SlabEspecifications Especifications;

        public SlabAlgorythim(SlabEspecifications especifications)
        {
            Especifications = especifications;
        }

        public static Point3d RotatePoint(double x, double y, double z, double angle)
        {
            var pt = new Point3d(x, y, z);
            return RotatePoint(pt, angle);
        }

        public static Point3d RotatePoint(Point3d point, double angle)
        {
            return point.RotateBy((angle * Math.PI) / 180D, Vector3d.ZAxis, Point3d.Origin);
        }

        protected Point3dCollection GetPointMatrix(Vector3d startDesloc, double yIncr, double xIncr, double xOffset = 0, double yOffset = 0)
        {
            Point3dCollection list = new Point3dCollection();
            var startPt = Especifications.StartPoint.Add(startDesloc);
            var angle = (90 - Especifications.Algorythim.OrientationAngle) * Math.PI / 180D;
            var xStartOffset = xOffset * Math.Cos(angle) + yOffset * Math.Sin(angle);
            var yStartOffset = yOffset * Math.Cos(angle) + xOffset * Math.Sin(angle);

            for (double y = startPt.Y + yStartOffset; y < Especifications.MaxPoint.Y; y += yIncr * Math.Cos(angle) + xIncr * Math.Sin(angle))
                for (double x = startPt.X + xStartOffset; x < Especifications.MaxPoint.X; x += xIncr * Math.Cos(angle) + yIncr * Math.Sin(angle))
                        list.Add(new Point3d(x, y, 0));

            return list;
        }

        public Point3dCollection GetCastPointList()
        {
            var result = new Point3dCollection();
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;
            var orientationAngle = 90 - Especifications.Algorythim.OrientationAngle;

            var startDesloc = new Vector3d(selectedLp.Height + spacing, selectedLd.Height / 2.0D, 0);
            var xIncr = selectedLd.Width + 2 * spacing + selectedLp.Height;
            var yIncr = selectedCast.Width;
            var auxPts = GetPointMatrix(startDesloc, yIncr, xIncr);

            foreach (Point3d p in auxPts)
                for (int i = 0; i < Especifications.CastGroupSize; i++)
                    result.Add(RotatePoint(p.X + (i * selectedCast.Width), p.Y, 0, orientationAngle));

            return result;
        }

        public Point3dCollection GetLdPointList()
        {
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;

            var startDesloc = new Vector3d(selectedLp.Height + spacing, 0, 0);
            var xIncr = selectedLd.Width + 2 * spacing + selectedLp.Height;
            var yIncr = selectedCast.Height;
            var points = GetPointMatrix(startDesloc, yIncr, xIncr);
            
            if (Especifications.Algorythim.UseLds)
            {
                var firstPoint = points[0];
                var lastPoint = points[points.Count - 1];
                var result = new Point3dCollection();

                foreach (Point3d point in points)
                    if (!CheckIfIsLDS(point, firstPoint, lastPoint))
                        result.Add(point);

                return result;
            } else
                return points;
        }

        public Point3dCollection GetStartLpPointList()
        {
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedStartLp = Especifications.Algorythim.SelectedStartLp;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;
            var useStartLp = Especifications.Algorythim.UseStartLp;

            var startDesloc = new Vector3d(0, 0, 0);
            var xIncr = selectedLp.Height + selectedLd.Width + spacing * 2;
            var yIncr = selectedLp.Width + Especifications.Algorythim.DistanceBetweenLp;
            var points = GetPointMatrix(startDesloc, yIncr, xIncr, 0, (useStartLp) ? selectedStartLp.StartOffset : selectedLp.StartOffset);
            var startPoint = points[0];
            var result = new Point3dCollection();

            foreach (Point3d p in points)
                if (CheckIfIsStartLP(startPoint, p))
                    result.Add(p);

            return result;
        }

        public Point3dCollection GetLpPointList()
        {
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedStartLp = Especifications.Algorythim.SelectedStartLp;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;
            var useStartLp = Especifications.Algorythim.UseStartLp;

            var startDesloc = new Vector3d(0, 0, 0);
            var xIncr = selectedLp.Height + selectedLd.Width + spacing * 2;
            var yIncr = selectedLp.Width + Especifications.Algorythim.DistanceBetweenLp;
            var points = GetPointMatrix(startDesloc, yIncr, xIncr, 0, (useStartLp) ? selectedStartLp.StartOffset : selectedLp.StartOffset);
            var startPoint = points[0];
            var result = new Point3dCollection();

            if (!Especifications.Algorythim.UseStartLp)
                return points;
            else
            {
                foreach (Point3d p in points)
                    if (!CheckIfIsStartLP(startPoint, p))
                        result.Add(p);
            }

            return result;
        }

        public Point3dCollection GetHeadPointList(Part selectedHead)
        {
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;

            var startDesloc = new Vector3d(-3.0, -selectedLd.Height / 2.0F, 0);
            var xIncr = selectedLp.Height + selectedLd.Width + spacing * 2;
            var yIncr = selectedCast.Height;

            return GetPointMatrix(startDesloc, yIncr, xIncr);
        }

        public Point3dCollection GetLdsPointList()
        {
            var selectedLd = Especifications.Parts.SelectedLd;
            var selectedLp = Especifications.Parts.SelectedLp;
            var selectedCast = Especifications.Parts.SelectedCast;
            var spacing = Especifications.Algorythim.DistanceBetweenLpAndLd;

            var startDesloc = new Vector3d(selectedLp.Height + spacing, 0, 0);
            var xIncr = selectedLd.Width + 2 * spacing + selectedLp.Height;
            var yIncr = selectedCast.Height;
            var points = GetPointMatrix(startDesloc, yIncr, xIncr);

            var firstPoint = points[0];
            var lastPoint = points[points.Count - 1];
            var result = new Point3dCollection();

            foreach (Point3d point in points)
                if (CheckIfIsLDS(point, firstPoint, lastPoint))
                    result.Add(point);

            return result;
        }

        public static Polyline3d CreateSquare(Part part, double border)
        {
            var pts = new Point3dCollection();
            pts.Add(new Point3d(-border, -border, 0));
            pts.Add(new Point3d(-border, part.Height + border, 0));
            pts.Add(new Point3d(part.Width + border, part.Height + border, 0));
            pts.Add(new Point3d(part.Width + border, - border, 0));
            var polyline = new Polyline3d(Poly3dType.SimplePoly, pts, true);
            if (border > 0) polyline.Color = Color.FromRgb(255, 0, 0);

            return polyline;
        }

        public static bool IsInsidePolygon(Polyline polygon, Point3d pt)
        {
            int n = polygon.NumberOfVertices;
            double angle = 0;
            Point2d pt1, pt2;

            for (int i = 0; i < n; i++)
            {
                pt1 = new Point2d(polygon.GetPoint2dAt(i).X - pt.X, polygon.GetPoint2dAt(i).Y - pt.Y);
                pt2 = new Point2d(polygon.GetPoint2dAt((i + 1) % n).X - pt.X, polygon.GetPoint2dAt((i + 1) % n).Y - pt.Y);
                angle += GetAngle2D(pt1.X, pt1.Y, pt2.X, pt2.Y);
            }

            if (Math.Abs(angle) < Math.PI)
                return false;
            else
                return true;
        }

        private static double GetAngle2D(double x1, double y1, double x2, double y2)
        {
            double dtheta, theta1, theta2;

            theta1 = Math.Atan2(y1, x1);
            theta2 = Math.Atan2(y2, x2);
            dtheta = theta2 - theta1;
            while (dtheta > Math.PI)
                dtheta -= (Math.PI * 2);
            while (dtheta < -Math.PI)
                dtheta += (Math.PI * 2);
            return (dtheta);
        }

        public bool CheckIfIsStartLP(Point3d startPoint, Point3d point)
        {
            var orientation = Especifications.Algorythim.SelectedOrientation;
            return ((point.Y <= startPoint.Y && orientation == Orientation.Vertical) 
                || (point.X <= startPoint.X && orientation == Orientation.Horizontal));
        }

        public bool CheckIfIsLDS(Point3d point, Point3d firstPoint, Point3d lastPoint)
        {
            var orientation = Especifications.Algorythim.SelectedOrientation;
            var isAtTheBeginningOrEndingOnVertical = (point.Y <= firstPoint.Y || point.Y >= lastPoint.Y) && (orientation == Orientation.Vertical);
            var isAtTheBeginningEndingOnHorizontal = (point.X <= firstPoint.X || point.X >= lastPoint.X) && (orientation == Orientation.Horizontal);

            return (isAtTheBeginningOrEndingOnVertical || isAtTheBeginningEndingOnHorizontal);
        }

        public bool IsAtTheEnd(Point3d lastPoint, Point3d point)
        {
            var orientation = Especifications.Algorythim.SelectedOrientation;
            return ((point.Y >= lastPoint.Y && orientation == Orientation.Vertical) 
                || (point.X >= lastPoint.X && orientation == Orientation.Horizontal));
        }

        public Point3d? GetBelowLpPoint(Point3dCollection points, Point3d current)
        {
            var dist = Especifications.Parts.SelectedLp.Width + Especifications.Algorythim.DistanceBetweenLp;
            var orientation = Especifications.Algorythim.SelectedOrientation;

            foreach (Point3d point in points)
            {
                if (point != current)
                {
                    var isBellow = (orientation == Orientation.Vertical && point.Y < current.Y)
                        || (orientation == Orientation.Horizontal && point.X > current.X);

                    if (isBellow && current.DistanceTo(point) == dist)
                        return point;
                }
            }

            return null;
        }

        public static IEnumerable<Line> CreateCrossLines(Part part, double border)
        {
            yield return new Line(
                new Point3d(part.Width / 2.0F, -border, 0),
                new Point3d(part.Width / 2.0F, part.Height + border, 0)
            );
            yield return new Line(
                new Point3d(-border, part.Height / 2.0F, 0),
                new Point3d(part.Width + border, part.Height / 2.0F, 0)
            );
        }

        public static Vector3d VectorFrom(double angle)
        {
            return RotatePoint(new Point3d(1, 0, 0), angle) - Point3d.Origin;
        }

        public void FindBetterPartCombination(IEnumerable<Part> firstList, IEnumerable<Part> secondList, double distance, out Part firstPart, out Part secondPart)
        {
            firstPart = null;
            secondPart = null;
            double distanceToInterference = distance - Especifications.Algorythim.OutlineDistance;
            double delta = double.MaxValue;
            double tmpDelta = 0;

            foreach (var part1 in firstList)
            {
                tmpDelta = part1.Width - distanceToInterference;
                if (tmpDelta <= 0 && Math.Abs(tmpDelta) < delta)
                {
                    delta = Math.Abs(tmpDelta);
                    firstPart = part1;
                }

                foreach (var part2 in secondList)
                {
                    tmpDelta = (part1.Width + part2.Width) - distanceToInterference;
                    if (tmpDelta <= 0 && Math.Abs(tmpDelta) < delta)
                    {
                        delta = Math.Abs(tmpDelta);
                        firstPart = part1;
                        secondPart = part2;
                    }
                }
            }

        }

    }
}

﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Urbbox.SlabAssembler.Core
{
    public class HorizontalMeshManager : IDisposable, IMeshManager
    {
        public Task<Point3dCollection> CastList { get; private set; }
        public Task<Point3dCollection> LpList { get; private set; }
        public Task<Dictionary<Point3d, Point3dCollection>> EndLpList { get; private set; }
        public Task<Point3dCollection> HeadList { get; private set; }
        public Task<Point3dCollection> LdsList { get; private set; }
        public Task<Point3dCollection> StartLpList { get; private set; }

        private readonly SlabProperties _properties;
        private readonly double _globalOrientation;
        private readonly Polyline _outline;

        public HorizontalMeshManager(SlabProperties properties, Polyline outline)
        {
            _properties = properties;
            _outline = outline;
            _globalOrientation = -ToRadians(90 - _properties.Algorythim.GlobalOrientationAngle);

            if (!properties.Algorythim.OnlyCimbrament)
                CastList = Task.Factory.StartNew(() => InitializeCastMesh());

            if (properties.Algorythim.SelectedStartLp != null)
                StartLpList = Task.Factory.StartNew(() => InitializeStartLpMesh());

            LpList = Task.Factory.StartNew(() => InitializeLpMesh());
            EndLpList = Task.Factory.StartNew(() => InitializeEndLpMesh());

            if (properties.Algorythim.Options.UseLds)
                LdsList = Task.Factory.StartNew(() => InitializeLdsMesh());

            HeadList = Task.Factory.StartNew(() => InitializeHeadMesh());
        }

        private Point3dCollection InitializeStartLpMesh()
        {
            var list = new Point3dCollection();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var startLp = _properties.Algorythim.SelectedStartLp;
            var cast = _properties.Parts.SelectedCast;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(startLp?.StartOffset ?? _properties.Parts.SelectedLp.StartOffset, 0, 0);
            var startPoint = _properties.StartPoint.Add(startVector);
            var incrVect = new Vector2d(lp.Width + _properties.Algorythim.Options.DistanceBetweenLp, ld.Width + lp.Height + spacing * 2.0);

            double y = 0;
            for (y = startPoint.Y; y < _properties.MaxPoint.Y; y += incrVect.Y)
                list.Add(new Point3d(startPoint.X, y, 0));

            return list;
        }

        private static double ToRadians(float degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private Point3dCollection InitializeLdsMesh()
        {
            var list = new Point3dCollection();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var cast = _properties.Parts.SelectedCast;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(0, lp.Height + spacing, 0);
            var startPoint = _properties.StartPoint.Add(startVector);
            var incrVector = new Vector2d(cast.Width, ld.Width + spacing * 2.0 + lp.Height);

            for (double y = startPoint.Y; y < _properties.MaxPoint.Y; y += incrVector.Y)
            {
                list.Add(new Point3d(startPoint.X, y, 0));
                double x = 0;
                for (x = startPoint.X; x < _properties.MaxPoint.X && SlabAlgorythim.IsInsidePolygon(_outline, new Point3d(x + incrVector.X + 10, y, 0)); x += incrVector.X) ;

                if (SlabAlgorythim.IsInsidePolygon(_outline, new Point3d(x + _properties.Algorythim.Options.OutlineDistance / 2.0 + cast.Width, y, 0)))
                    list.Add(new Point3d(x, y, 0));
                else
                    list.Add(new Point3d(x, y, 0));
            }

            return list;
        }

        private Point3dCollection InitializeHeadMesh()
        {
            var list = new Point3dCollection();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var cast = _properties.Parts.SelectedCast;
            var head = _properties.Parts.SelectedHead;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(-(head.Height - ld.Height)/ 2.0, -head.Width / 2.0 + lp.Height / 2.0, 0);
            var startPoint = _properties.StartPoint.Add(startVector);
            var useStartLp = _properties.Algorythim.SelectedStartLp != null;
            var incrVect = new Vector2d(cast.Width, ld.Width + spacing * 2.0 + lp.Height);

            for (double x = startPoint.X; x < _properties.MaxPoint.X; x += incrVect.X)
            {
                for (double y = startPoint.Y; y < _properties.MaxPoint.Y; y += incrVect.Y)
                {
                    if (_properties.Algorythim.Options.UseEndLp && (x + cast.Width) >= _properties.MaxPoint.X)
                        continue;

                    if (useStartLp && x <= startPoint.X)
                        continue;

                    list.Add(new Point3d(x, y, 0));
                }

            }

            return list;
        }

        private Point3dCollection InitializeLpMesh()
        {
            var list = new Point3dCollection();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var startLp = _properties.Algorythim.SelectedStartLp;
            var useStartLp = startLp != null;
            var cast = _properties.Parts.SelectedCast;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(startLp?.StartOffset ?? _properties.Parts.SelectedLp.StartOffset, 0, 0);
            var startPoint = _properties.StartPoint.Add(startVector);
            var incrVect = new Vector2d(lp.Width + _properties.Algorythim.Options.DistanceBetweenLp, ld.Width + lp.Height + spacing * 2.0);
            //var countY = 0;

            if (useStartLp)
                startPoint = startPoint.Add(new Vector3d(startLp.Width + _properties.Algorythim.Options.DistanceBetweenLp, 0, 0));


            for (double x = startPoint.X; x < _properties.MaxPoint.X; x += incrVect.X)
            {
                for (double y = startPoint.Y; y < _properties.MaxPoint.Y; y += incrVect.Y)
                {
                    list.Add(new Point3d(x, y, 0));
                }
            }

            return list;
        }

        private Dictionary<Point3d, Point3dCollection> InitializeEndLpMesh()
        {
            var scanlines = new Dictionary<Point3d, Point3dCollection>();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var startLp = _properties.Algorythim.SelectedStartLp;
            var useStartLp = startLp != null;
            var cast = _properties.Parts.SelectedCast;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(startLp?.StartOffset ?? _properties.Parts.SelectedLp.StartOffset, 0, 0);
            var startPoint = _properties.StartPoint.Add(startVector);
            var incrVect = new Vector2d(lp.Width + _properties.Algorythim.Options.DistanceBetweenLp, ld.Width + lp.Height + spacing * 2.0);

            if (useStartLp)
                startPoint = startPoint.Add(new Vector3d(startLp.Width + _properties.Algorythim.Options.DistanceBetweenLp, 0, 0));

            for (double y = startPoint.Y; y < _properties.MaxPoint.Y; y += incrVect.Y)
                scanlines.Add(new Point3d(startPoint.X, y, 0), ScanLine.GetOutlineSurroudingPointsX(new Point3d(startPoint.X, y, 0), _properties.MaxPoint, lp.Width + _properties.Algorythim.Options.DistanceBetweenLp, _outline));

            return scanlines;
        }

      
        private Point3dCollection InitializeCastMesh()
        {
            var list = new Point3dCollection();
            var ld = _properties.Parts.SelectedLd;
            var lp = _properties.Parts.SelectedLp;
            var cast = _properties.Parts.SelectedCast;
            var spacing = _properties.Algorythim.Options.DistanceBetweenLpAndLd;
            var startVector = new Vector3d(ld.Height / 2.0, lp.Height + spacing, 0);
            var spaceBetweenGroups = lp.Height + 2.0 * spacing;
            var startPt = _properties.StartPoint.Add(startVector);
            var incrVector = new Vector2d(cast.Width, cast.Height);

            for (var x = startPt.X; x < _properties.MaxPoint.X; x += incrVector.X)
            {
                var countY = 0;
                for (var y = startPt.Y; y < _properties.MaxPoint.Y; y += incrVector.Y)
                {
                    if (countY > 0 && countY % _properties.CastGroupSize == 0)
                        y += spaceBetweenGroups;

                    list.Add(new Point3d(x, y, 0));
                    countY++;
                }
            }

            return list;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CastList?.Dispose();
                    LdsList?.Dispose();
                    StartLpList?.Dispose();
                    LpList?.Dispose();
                    HeadList?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


    }
}

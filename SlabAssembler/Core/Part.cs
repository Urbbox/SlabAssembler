﻿using Autodesk.AutoCAD.DatabaseServices;
using ReactiveUI;
using System;
using System.Collections;
using System.Reactive.Linq;
using System.Xml.Serialization;
using Urbbox.SlabAssembler.Core.Variations;
using Urbbox.SlabAssembler.ViewModels;
using Autodesk.AutoCAD.Geometry;

namespace Urbbox.SlabAssembler.Core
{
    [Serializable]
    public class Part : ReactiveObject
    {
        private string _referenceName;
        public string ReferenceName {
            get { return _referenceName; }
            set { this.RaiseAndSetIfChanged(ref _referenceName, value); }
        }

        private float _width;
        public float Width {
            get { return _width; }
            set { this.RaiseAndSetIfChanged(ref _width, value); }
        }

        private float _height;
        public float Height {
            get { return _height; }
            set { this.RaiseAndSetIfChanged(ref _height, value); }
        }

        private UsageType _usageType;
        public UsageType UsageType {
            get { return _usageType; }
            set { this.RaiseAndSetIfChanged(ref _usageType, value); }
        }

        private string _layer;
        public string Layer {
            get { return _layer; }
            set { this.RaiseAndSetIfChanged(ref _layer, value); }
        }

        private double _pivotFixX;
        public double PivotPointX
        {
            get { return _pivotFixX; }
            set { this.RaiseAndSetIfChanged(ref _pivotFixX, value); }
        }

        private double _pivotFixY;
        public double PivotPointY
        {
            get { return _pivotFixY; }
            set { this.RaiseAndSetIfChanged(ref _pivotFixY, value); }
        }

        [XmlIgnore]
        public Point3d PivotPoint => new Point3d(PivotPointX, PivotPointY, 0);

        private double _startOffset;
        public double StartOffset
        {
            get { return _startOffset; }
            set { this.RaiseAndSetIfChanged(ref _startOffset, value); }
        }

        private string _name;
        [XmlAttribute]
        public string Name {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        private int _modulation;
        [XmlAttribute]
        public int Modulation {
            get { return _modulation; }
            set { this.RaiseAndSetIfChanged(ref _modulation, value); }
        }

        [XmlIgnore]
        public int Id => ReferenceName.GetHashCode();
        [XmlIgnore]
        public float GreatestDimension => (Width >= Height)? Width : Height;
        [XmlIgnore]
        public float SmallestDimension => (Width <= Height) ? Width : Height;
        [XmlIgnore]
        public ReactiveCommand<object> Save { get; protected set; }


        public Part()
        {
            Save = this.WhenAnyValue(x => x.ReferenceName, x => x.Name, x => x.Layer, x => x.Width, x => x.Height, x => x.Modulation)
                .Select(x => !String.IsNullOrEmpty(x.Item1) && !String.IsNullOrEmpty(x.Item2) && !String.IsNullOrEmpty(x.Item3) && x.Item4 > 0 && x.Item5 > 0 && x.Item6 > 0)
                .ToCommand();
        }

    }
}

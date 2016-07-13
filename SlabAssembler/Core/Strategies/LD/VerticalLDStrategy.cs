﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Repositories;

namespace Urbbox.SlabAssembler.Core.Strategies.LD
{
    public class VerticalLDStrategy : LDStrategy
    {
        public VerticalLDStrategy(SlabProperties properties, IPartRepository repo, AcEnvironment env) : base(properties, repo, env)
        {
        }

        protected override float PartOrientationAngle => 0;

        protected override Vector3d StartVector
        {
            get
            {
                var lp = Properties.Parts.SelectedLp;
                var spacing = Properties.Algorythim.Options.DistanceBetweenLpAndLd;
                return new Vector3d(lp.Height + spacing, 0, 0);
            }
        }

        protected override double XIncrement
        {
            get
            {
                var lp = Properties.Parts.SelectedLp;
                var spacing = Properties.Algorythim.Options.DistanceBetweenLpAndLd;
                return MainPart.Width + spacing * 2.0 + lp.Height;
            }
        }

        protected override double YIncrement => Properties.Parts.SelectedCast.Width;

        protected override void ResetJumpToRight(Part part)
        {
            X -= MainPart.Width - part.Width;
        }

        protected override void JumpToRight(Part part)
        {
            X += MainPart.Width - part.Width;
        }
    }
}

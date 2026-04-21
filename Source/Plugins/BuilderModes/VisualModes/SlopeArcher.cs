#region ================== Copyright (c) 2020 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.VisualModes;
using System;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes
{
    internal class SlopeArcher
    {
        BaseVisualMode mode;
        private List<IVisualEventReceiver> sectors;
        private VisualSidedefSlope handle1;
        private VisualSidedefSlope handle2;
        private double baseheightoffset;
        private Line2D handleline;
        private double length;

        public double Theta { get; set; }
        public double OffsetAngle { get; set; }
        public double Scale { get; set; }
        public int Baseheight { get; }
        public double HeightOffset { get; set; }

        public SlopeArcher(BaseVisualMode mode, List<IVisualEventReceiver> sectors, VisualSidedefSlope handle1, VisualSidedefSlope handle2, double theta, double offsetangle, double scale)
        {
            this.mode = mode;
            this.sectors = sectors;
            this.handle1 = handle1;
            this.handle2 = handle2;
            this.Theta = theta;
            this.OffsetAngle = offsetangle;
            this.Scale = scale;
            HeightOffset = 0.0;

            handleline = new Line2D(handle1.GetCenterPoint(), handle2.GetCenterPoint());
            length = handleline.GetLength();

            if (handle1.Level.type == SectorLevelType.Ceiling)
                Baseheight = handle1.Level.extrafloor ? handle1.Level.sector.FloorHeight : handle1.Level.sector.CeilHeight;
            else
                Baseheight = handle1.Level.extrafloor ? handle1.Level.sector.CeilHeight : handle1.Level.sector.FloorHeight;

            baseheightoffset = 0.0;
        }

        /// <summary>
        /// Applies the slopes to the sectors.
        /// 
        /// We have:
        /// - theta
        /// - offset angle ("offset")
        /// - horizontal line length ("length")
        ///
        /// What we need to compute:
        /// - x coordinate where the line starts in the circle ("left", this is cos(theta + offset angle))
        /// - x coordinate where the line ends in the circle ("middle", this is cos(offset angle))
        ///
        /// With this data we can calculate some more required variables:
        /// - radius: length / (middle - left)
        /// - left delimiter: cos(offset + theta) * radius
        /// - right delimiter: cos(rotation) * radius (should be same as left delimiter + length)
        /// - section start, in map units: cos(offset + theta) * radius
        /// - base height offset (where the slope starts)
        ///
        /// Then we can simply use pythagoras to compute the y position for an x position on the length
        /// </summary>
        public void ApplySlope()
        {
            double left = Math.Cos(Theta + OffsetAngle);
            double middle = Math.Cos(OffsetAngle);

            double radius = length / (middle - left);
            double leftdelimiter = Math.Cos(OffsetAngle + Theta);
            double rightdelimiter = Math.Cos(OffsetAngle);

            double sectionstart = Math.Cos(OffsetAngle + Theta) * radius;

            baseheightoffset = Math.Sqrt((radius * radius) - (sectionstart * sectionstart)) * Scale;

            foreach (BaseVisualGeometrySector bvgs in sectors)
            {
                HashSet<Vertex> vertices = new HashSet<Vertex>(bvgs.Sector.Sides.Count * 2);
                double u1 = 1.0;
                double u2 = 0.0;

                foreach (Sidedef sd in bvgs.Sector.Sides.Keys)
                {
                    vertices.Add(sd.Line.Start);
                    vertices.Add(sd.Line.End);
                }

                // Get the two points that are the furthest apart on the line between the slope handles
                foreach (Vertex v in vertices)
                {
                    double intersection = handleline.GetNearestOnLine(v.Position);

                    if (intersection < u1)
                        u1 = intersection;
                    if (intersection > u2)
                        u2 = intersection;
                }

                // Compute the x position and the corrosponding height of the coordinates
                double xpos1 = sectionstart + (u1 * length);
                double xpos2 = sectionstart + (u2 * length);
                double height1 = Math.Sqrt((radius * radius) - (xpos1 * xpos1)) * Scale;
                double height2 = Math.Sqrt((radius * radius) - (xpos2 * xpos2)) * Scale;

                if (double.IsNaN(height1))
                    height1 = 0.0;

                if (double.IsNaN(height2))
                    height2 = 0.0;

                // Adjust the heights
                height1 = height1 - baseheightoffset + Baseheight + HeightOffset;
                height2 = height2 - baseheightoffset + Baseheight + HeightOffset;

                // Get the angle of the slope. We cheat a bit and substitute the y value of the vectors with the height of the points
                double slopeangle = Vector2D.GetAngle(new Vector2D(xpos1, height1), new Vector2D(xpos2, height2));

                // Always let the plane point up, VisualSidedefSlope.ApplySlope will invert it if necessary
                Plane plane = new Plane(new Vector3D(handleline.GetCoordinatesAt(u1), height1), handleline.GetAngle() + Angle2D.PIHALF, slopeangle, true);

                VisualSidedefSlope.ApplySlope(bvgs.Level, plane, mode);

                bvgs.Sector.UpdateSectorGeometry(true);
            }
        }
    }
}

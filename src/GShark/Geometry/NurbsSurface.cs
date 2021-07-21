﻿using GShark.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GShark.Geometry.Interfaces;
using GShark.Operation;

namespace GShark.Geometry
{
    /// <summary>
    /// This class represents a NURBS surface.
    /// </summary>
    /// <example>
    /// [!code-csharp[Example](../../src/GShark.Test.XUnit/Data/NurbsSurfaceCollection.cs?name=example)]
    /// </example>
    public class NurbsSurface : IEquatable<NurbsSurface>, ITransformable<NurbsSurface>
    {
        /// <summary>
        /// Internal constructor used to validate the NURBS surface.
        /// </summary>
        /// <param name="degreeU">The degree in the U direction.</param>
        /// <param name="degreeV">The degree in the V direction.</param>
        /// <param name="knotsU">The knotVector in the U direction.</param>
        /// <param name="knotsV">The knotVector in the V direction.</param>
        /// <param name="pts">Two dimensional array of points.</param>
        /// <param name="weights">Two dimensional array of weight values.</param>
        internal NurbsSurface(int degreeU, int degreeV, KnotVector knotsU, KnotVector knotsV, List<List<Point3>> pts, List<List<double>>? weights = null)
        {
            if (pts == null) throw new ArgumentNullException("Control points array connot be null!");
            if (degreeU < 1) throw new ArgumentException("DegreeU must be greater than 1!");
            if (degreeV < 1) throw new ArgumentException("DegreeV must be greater than 1!");
            if (knotsU == null) throw new ArgumentNullException("KnotU cannot be null!");
            if (knotsV == null) throw new ArgumentNullException("KnotV cannot be null!");
            if (knotsU.Count != pts.Count + degreeU + 1)
                throw new ArgumentException("Points count + degreeU + 1 must equal knotsU count!");
            if (knotsV.Count != pts[0].Count + degreeV + 1)
                throw new ArgumentException("Points count + degreeV + 1 must equal knotsV count!");
            if (!knotsU.IsValid(degreeU, pts.Count))
                throw new ArgumentException("Invalid knotsU!");
            if (!knotsV.IsValid(degreeV, pts[0].Count))
                throw new ArgumentException("Invalid knotsV!");

            DegreeU = degreeU;
            DegreeV = degreeV;
            KnotsU = knotsU;
            KnotsV = knotsV;
            Weights = weights ?? Sets.RepeatData(Sets.RepeatData(1.0, pts.Count), pts[0].Count);
            LocationPoints = pts;
            ControlPoints = LinearAlgebra.PointsHomogeniser2d(pts, weights);
            DomainU = new Interval(this.KnotsU.First(), this.KnotsU.Last());
            DomainV = new Interval(this.KnotsV.First(), this.KnotsV.Last());
        }

        /// <summary>
        /// The degree in U direction.
        /// </summary>
        internal int DegreeU { get; }

        /// <summary>
        /// The degree in V direction.
        /// </summary>
        internal int DegreeV { get; }

        /// <summary>
        /// The knotVector in U direction.
        /// </summary>
        internal KnotVector KnotsU { get; }

        /// <summary>
        /// The knotVector in V direction.
        /// </summary>
        internal KnotVector KnotsV { get; }

        /// <summary>
        /// The interval domain in U direction.
        /// </summary>
        internal Interval DomainU { get; }

        /// <summary>
        /// The interval domain in V direction.
        /// </summary>
        internal Interval DomainV { get; }

        /// <summary>
        /// A 2d collection of weight values.
        /// </summary>
        public List<List<double>> Weights { get; }

        /// <summary>
        /// A 2d collection of points, the vertical U direction increases from bottom to top, the V direction from left to right.
        /// </summary>
        public List<List<Point3>> LocationPoints { get; }

        /// <summary>
        /// A 2d collection of control points, the vertical U direction increases from bottom to top, the V direction from left to right.
        /// </summary>
        internal List<List<Point4>> ControlPoints { get; }

        /// <summary>
        /// Constructs a NURBS surface from four perimeter points in counter-clockwise order.<br/>
        /// The surface is defined with degree 1.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="p3">The third point.</param>
        /// <param name="p4">The fourth point.</param>
        public static NurbsSurface ByFourPoints(Point3 p1, Point3 p2, Point3 p3, Point3 p4)
        {
            List<List<Point3>> pts = new List<List<Point3>>
            {
                new List<Point3>{p1, p4},
                new List<Point3>{p2, p3},
            };

            KnotVector knotU = new KnotVector { 0, 0, 1, 1 };
            KnotVector knotV = new KnotVector { 0, 0, 1, 1 };

            return new NurbsSurface(1, 1, knotU, knotV, pts);
        }

        /// <summary>
        /// Evaluates a point at a given U and V parameters.
        /// </summary>
        /// <param name="u">Evaluation U parameter.</param>
        /// <param name="v">Evaluation V parameter.</param>
        /// <returns>A evaluated point.</returns>
        public Point3 PointAt(double u, double v) => Evaluation.SurfacePointAt(this, u, v);

        /// <summary>
        /// Obtain the surface normal at the given u and v parameters
        /// </summary>
        /// <param name="u">u parameter</param>
        /// <param name="v">v parameter</param>
        /// <returns></returns>
        //public Vector3d Normal(double u, double v) => Evaluation.RationalSurfaceNormal(this, u, v).Unitize();

        /// <summary>
        /// Obtain the surface tangent at the given u and v parameters in the u direction
        /// </summary>
        /// <param name="u">u parameter</param>
        /// <param name="v">v parameter</param>
        /// <returns></returns>
        //public Vector3d TangentAtU(double u, double v) => Evaluation.RationalSurfaceDerivatives(this, u, v)[1][0].Unitize();

        /// <summary>
        /// Obtain the surface tangent at the given u and v parameters in the v direction
        /// </summary>
        /// <param name="u">u parameter</param>
        /// <param name="v">v parameter</param>
        /// <returns></returns>
        //public Vector3d TangentAtV(double u, double v) => Evaluation.RationalSurfaceDerivatives(this, u, v)[0][1].Unitize();

        /// <summary>
        /// Transforms a NURBS surface with the given transformation matrix.
        /// </summary>
        /// <param name="transformation">The transformation matrix.</param>
        /// <returns>A new NURBS surface transformed.</returns>
        public NurbsSurface Transform(Transform transformation)
        {
            List<List<Point3>> otherPts = LocationPoints;
            otherPts.ForEach(pts => pts.ForEach(pt => pt.Transform(transformation)));
            return new NurbsSurface(DegreeU, DegreeV, KnotsU, KnotsV, otherPts, Weights);
        }

        /// <summary>
        /// Compares if two NURBS surfaces are the same.<br/>
        /// Two NURBS curves are equal when the have same degrees, same control points order and dimension, and same knots.
        /// </summary>
        /// <param name="other">The NURBS surface.</param>
        /// <returns>Return true if the NURBS surface are equal.</returns>
        public bool Equals(NurbsSurface? other)
        {
            List<List<Point3>> otherPts = other?.LocationPoints;

            if (other == null)
            {
                return false;
            }

            if (LocationPoints.Count != otherPts?.Count)
            {
                return false;
            }

            if (!LocationPoints.All(otherPts.Contains))
            {
                return false;
            }

            if (KnotsU.Count != other.KnotsU.Count || KnotsV.Count != other.KnotsU.Count)
            {
                return false;
            }

            return DegreeU == other.DegreeU && DegreeV == other.DegreeV && Weights.All(other.Weights.Contains);
        }

        /// <summary>
        /// Implements the override method to string.
        /// </summary>
        /// <returns>The representation of a NURBS surface in string.</returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            string controlPts = string.Join("\n", LocationPoints.Select(first => $"({string.Join(",", first)})"));
            string degreeU = $"DegreeU = {DegreeU}";
            string degreeV = $"DegreeV = {DegreeV}";

            stringBuilder.AppendLine(controlPts);
            stringBuilder.AppendLine(degreeU);
            stringBuilder.AppendLine(degreeV);

            return stringBuilder.ToString();
        }
    }
}
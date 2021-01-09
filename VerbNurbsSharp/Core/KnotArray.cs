﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerbNurbsSharp.Core
{
    /// <summary>
    /// A KnotArray is a non-decreasing sequence of doubles. Use the methods in <see cref="VerbNurbsSharp.Evaluation.Check"/>/> to validate KnotArray's.
    /// </summary>
    public class KnotArray : List<double>
    {
        public KnotArray(){}

        public KnotArray(int degree, int numberOfControlPts, bool clamped = true)
        {
            Generate(degree, numberOfControlPts, clamped);
        }

        public KnotArray(IEnumerable<double> values)
        {
            this.AddRange(values);
        }

        /// <summary>
        /// Confirm the relations between degree (p), number of control points(n+1), and the number of knots (m+1).
        /// </summary>
        /// <param name="degree">Curve degree.</param>
        /// <param name="numControlPts">Number of control points.</param>
        /// <returns>Whether the relation is confirmed.</returns>
        public bool AsValidRelations(int degree, int numControlPts) => numControlPts + degree + 1 - this.Count == 0;

        /// <summary>
        /// Generates an equally spaced knot vector.
        /// Clamp curve is tangent to the first and the last legs at the first and last control points.
        /// </summary>
        /// <param name="degree">Degree.</param>
        /// <param name="numberOfControlPts">Number of control points.</param>
        /// <param name="clamped">Flag to choose from clamped or unclamped knot vector options, default true.</param>
        public void Generate(int degree, int numberOfControlPts, bool clamped)
        {
            if (degree == 0 || numberOfControlPts == 0)
                throw new Exception("Input values should be different than zero.");

            // Number of repetitions at the start and end of the array.
            var numOfRepeat = degree;
            // Number of knots in the middle.
            var numOfSegments = numberOfControlPts - (degree + 1);

            if (!clamped)
            {
                // No repetitions at the start and end.
                numOfRepeat = 0;
                // Should conform the rule m = n+p+1
                numOfSegments = degree + numberOfControlPts - 1;
            }

            var knotVector = new List<double>();
            knotVector.AddRange(Sets.RepeatData(0.0, numOfRepeat));
            knotVector.AddRange(Sets.LinearSpace(new Interval(0.0,1.0), numOfSegments+2));
            knotVector.AddRange(Sets.RepeatData(1.0, numOfRepeat));

            this.AddRange(knotVector);
        }

        public override string ToString()
        {
            return $"{{{string.Join(",", this)}}}";
        }
    }
}

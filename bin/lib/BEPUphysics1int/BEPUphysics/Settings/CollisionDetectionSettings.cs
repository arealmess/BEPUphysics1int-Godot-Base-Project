﻿using BEPUutilities;
using Deterministic.FixedPoint;
using FixMath.NET;
using System;
namespace BEPUphysics.Settings
{
    ///<summary>
    /// Settings class containing global information about collision detection.
    ///</summary>
    public static class CollisionDetectionSettings
    {


        internal static fp ContactInvalidationLengthSquared = (fp).01m;

        /// <summary>
        /// For persistent manifolds, contacts are represented by an offset in local space of two colliding bodies.
        /// The distance between these offsets transformed into world space and projected onto a plane defined by the contact normal squared is compared against this value.
        /// If this value is exceeded, the contact is removed from the contact manifold.
        /// 
        /// If the world is smaller or larger than 'normal' for the engine, adjusting this value proportionally can improve contact caching behavior.
        /// The default value of .1f works well for worlds that operate on the order of 1 unit.
        /// </summary>
        public static fp ContactInvalidationLength
        {
            get
            {
                return fixmath.Sqrt(ContactInvalidationLengthSquared);
            }
            set
            {
                ContactInvalidationLengthSquared = value * value;
            }
        }


        internal static fp ContactMinimumSeparationDistanceSquared = (fp).0009m;
        /// <summary>
        /// In persistent manifolds, if two contacts are too close together, then 
        /// the system will not use one of them.  This avoids redundant constraints.
        /// Defaults to .03f.
        /// </summary>
        public static fp ContactMinimumSeparationDistance
        {
            get
            {
                return fixmath.Sqrt(ContactMinimumSeparationDistanceSquared);
            }
            set
            {
                ContactMinimumSeparationDistanceSquared = value * value;
            }
        }

        internal static fp nonconvexNormalDotMinimum = (fp).99m;
        /// <summary>
        /// In regular convex manifolds, two contacts are considered redundant if their positions are too close together.  
        /// In nonconvex manifolds, the normal must also be tested, since a contact in the same location could have a different normal.
        /// This property is the minimum angle in radians between normals below which contacts are considered redundant.
        /// </summary>
        public static fp NonconvexNormalAngleDifferenceMinimum
        {
            get
            {
                return fixmath.Acos(nonconvexNormalDotMinimum);
            }
            set
            {
                nonconvexNormalDotMinimum = fixmath.Cos(value);
            }
        }

        /// <summary>
        /// The default amount of allowed penetration into the margin before position correcting impulses will be applied.
        /// Defaults to .01f.
        /// </summary>
        public static fp AllowedPenetration = (fp).01m;

        /// <summary>
        /// Default collision margin around objects.  Margins help prevent objects from interpenetrating and improve stability.
        /// Defaults to .04f.
        /// </summary>
        public static fp DefaultMargin = (fp).04m;

        internal static fp maximumContactDistance = (fp).1m;
        /// <summary>
        /// Maximum distance between the surfaces defining a contact point allowed before removing the contact.
        /// Defaults to .1f.
        /// </summary>
        public static fp MaximumContactDistance
        {
            get
            {
                return maximumContactDistance;
            }
            set
            {
                if (value >= F64.C0)
                    maximumContactDistance = value;
                else
                    throw new ArgumentException("Distance must be nonnegative.");
            }
        }


        
    }
}

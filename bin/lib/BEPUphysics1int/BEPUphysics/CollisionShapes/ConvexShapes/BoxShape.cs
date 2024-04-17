using System;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;

using BEPUutilities;
using FixMath.NET;

using Deterministic.FixedPoint;

namespace BEPUphysics.CollisionShapes.ConvexShapes
{
	///<summary>
	/// Convex shape with width, length, and height.
	///</summary>
	public class BoxShape : ConvexShape
	{
		internal fp halfWidth;
		internal fp halfHeight;
		internal fp halfLength;


		/// <summary>
		/// Width of the box divided by two.
		/// </summary>
		public fp HalfWidth
		{
			get { return halfWidth; }
			set { halfWidth = value; OnShapeChanged(); }
		}

		/// <summary>
		/// Height of the box divided by two.
		/// </summary>
		public fp HalfHeight
		{
			get { return halfHeight; }
			set { halfHeight = value; OnShapeChanged(); }
		}

		/// <summary>
		/// Length of the box divided by two.
		/// </summary>
		public fp HalfLength
		{
			get { return halfLength; }
			set { halfLength = value; OnShapeChanged(); }
		}

		/// <summary>
		/// Width of the box.
		/// </summary>
		public fp Width
		{
			get { return halfWidth * F64.C2; }
			set { halfWidth = value * F64.C0p5; OnShapeChanged(); }
		}

		/// <summary>
		/// Height of the box.
		/// </summary>
		public fp Height
		{
			get { return halfHeight * F64.C2; }
			set { halfHeight = value * F64.C0p5; OnShapeChanged(); }
		}

		/// <summary>
		/// Length of the box.
		/// </summary>
		public fp Length
		{
			get { return halfLength * F64.C2; }
			set { halfLength = value * F64.C0p5; OnShapeChanged(); }
		}


		///<summary>
		/// Constructs a new box shape.
		///</summary>
		///<param name="width">Width of the box.</param>
		///<param name="height">Height of the box.</param>
		///<param name="length">Length of the box.</param>
		public BoxShape(fp width, fp height, fp length)
		{
			halfWidth = width * F64.C0p5;
			halfHeight = height * F64.C0p5;
			halfLength = length * F64.C0p5;

			UpdateConvexShapeInfo(ComputeDescription(width, height, length, collisionMargin));
		}

		///<summary>
		/// Constructs a new box shape from cached information.
		///</summary>
		///<param name="width">Width of the box.</param>
		///<param name="height">Height of the box.</param>
		///<param name="length">Length of the box.</param>
		/// <param name="description">Cached information about the shape. Assumed to be correct; no extra processing or validation is performed.</param>
		public BoxShape(fp width, fp height, fp length, ConvexShapeDescription description)
		{
			halfWidth = width * F64.C0p5;
			halfHeight = height * F64.C0p5;
			halfLength = length * F64.C0p5;

			UpdateConvexShapeInfo(description);
		}

		protected override void OnShapeChanged()
		{
			UpdateConvexShapeInfo(ComputeDescription(halfWidth, halfHeight, halfLength, collisionMargin));
			base.OnShapeChanged();
		}

		/// <summary>
		/// Computes a convex shape description for a BoxShape.
		/// </summary>
		///<param name="width">Width of the box.</param>
		///<param name="height">Height of the box.</param>
		///<param name="length">Length of the box.</param>
		/// <param name="collisionMargin">Collision margin of the shape.</param>
		/// <returns>Description required to define a convex shape.</returns>
		public static ConvexShapeDescription ComputeDescription(fp width, fp height, fp length, fp collisionMargin)
		{
			ConvexShapeDescription description;
			description.EntityShapeVolume.Volume = width * height * length;

			fp widthSquared = width * width;
			fp heightSquared = height * height;
			fp lengthSquared = length * length;
			fp inv12 = F64.OneTwelfth;

			description.EntityShapeVolume.VolumeDistribution = new Matrix3x3();
			description.EntityShapeVolume.VolumeDistribution.M11 = (heightSquared + lengthSquared) * inv12;
			description.EntityShapeVolume.VolumeDistribution.M22 = (widthSquared + lengthSquared) * inv12;
			description.EntityShapeVolume.VolumeDistribution.M33 = (widthSquared + heightSquared) * inv12;

			description.MaximumRadius = F64.C0p5 * fixmath.Sqrt(width * width + height * height + length * length);
			description.MinimumRadius = F64.C0p5 * MathHelper.Min(width, MathHelper.Min(height, length));

			description.CollisionMargin = collisionMargin;
			return description;
		}





		/// <summary>
		/// Gets the bounding box of the shape given a transform.
		/// </summary>
		/// <param name="shapeTransform">Transform to use.</param>
		/// <param name="boundingBox">Bounding box of the transformed shape.</param>
		public override void GetBoundingBox(ref RigidTransform shapeTransform, out BoundingBox boundingBox)
		{
#if !WINDOWS
			boundingBox = new BoundingBox();
#endif

			Matrix3x3 o;
			Matrix3x3.CreateFromQuaternion(ref shapeTransform.Orientation, out o);
			//Sample the local directions from the orientation matrix, implicitly transposed.
			//Notice only three directions are used.  Due to box symmetry, 'left' is just -right.
			var right = new Vector3(fixmath.Sign(o.M11) * halfWidth, fixmath.Sign(o.M21) * halfHeight, fixmath.Sign(o.M31) * halfLength);

			var up = new Vector3(fixmath.Sign(o.M12) * halfWidth, fixmath.Sign(o.M22) * halfHeight, fixmath.Sign(o.M32) * halfLength);

			var backward = new Vector3(fixmath.Sign(o.M13) * halfWidth, fixmath.Sign(o.M23) * halfHeight, fixmath.Sign(o.M33) * halfLength);


			//Rather than transforming each axis independently (and doing three times as many operations as required), just get the 3 required values directly.
			Vector3 offset;
			TransformLocalExtremePoints(ref right, ref up, ref backward, ref o, out offset);

			//The positive and negative vectors represent the X, Y and Z coordinates of the extreme points in world space along the world space axes.
			Vector3.Add(ref shapeTransform.Position, ref offset, out boundingBox.Max);
			Vector3.Subtract(ref shapeTransform.Position, ref offset, out boundingBox.Min);

		}


		///<summary>
		/// Gets the extreme point of the shape in local space in a given direction.
		///</summary>
		///<param name="direction">Direction to find the extreme point in.</param>
		///<param name="extremePoint">Extreme point on the shape.</param>
		public override void GetLocalExtremePointWithoutMargin(ref Vector3 direction, out Vector3 extremePoint)
		{
			extremePoint = new Vector3(fixmath.Sign(direction.X) * (halfWidth - collisionMargin), fixmath.Sign(direction.Y) * (halfHeight - collisionMargin), fixmath.Sign(direction.Z) * (halfLength - collisionMargin));
		}




		/// <summary>
		/// Gets the intersection between the box and the ray.
		/// </summary>
		/// <param name="ray">Ray to test against the box.</param>
		/// <param name="transform">Transform of the shape.</param>
		/// <param name="maximumLength">Maximum distance to travel in units of the direction vector's length.</param>
		/// <param name="hit">Hit data for the raycast, if any.</param>
		/// <returns>Whether or not the ray hit the target.</returns>
		/// 
		//public override bool RayTest(ref Ray ray, ref RigidTransform transform, fp maximumLength, out RayHit hit)
		//{
		//	hit = new RayHit();

		//	Quaternion conjugate;
		//	Quaternion.Conjugate(ref transform.Orientation, out conjugate);
		//	Vector3 localOrigin;
		//	Vector3.Subtract(ref ray.Position, ref transform.Position, out localOrigin);
		//	Quaternion.Transform(ref localOrigin, ref conjugate, out localOrigin);
		//	Vector3 localDirection;
		//	Quaternion.Transform(ref ray.Direction, ref conjugate, out localDirection);
		//	Vector3 normal = Toolbox.ZeroVector;
		//	fp temp, tmin = F64.C0, tmax = maximumLength;

		//	if (fixmath.Abs(localDirection.X) < Toolbox.Epsilon && (localOrigin.X < -halfWidth || localOrigin.X > halfWidth))
		//		return false;
		//	fp inverseDirection =
		//	
		//	
		//	
		//	
		//	
		//	/ localDirection.X;
		//	// inverseDirection might be Infinity (fp.max), so use SafeMul here to handle overflow
		//	fp t1 = Fix64.SafeMul((-halfWidth - localOrigin.X), inverseDirection);
		//	fp t2 = Fix64.SafeMul((halfWidth - localOrigin.X), inverseDirection);
		//	var tempNormal = new Vector3(-1, F64.C0, F64.C0);
		//	if (t1 > t2)
		//	{
		//		temp = t1;
		//		t1 = t2;
		//		t2 = temp;
		//		tempNormal *= -1;
		//	}
		//	temp = tmin;
		//	tmin = MathHelper.Max(tmin, t1);
		//	if (temp != tmin)
		//		normal = tempNormal;
		//	tmax = MathHelper.Min(tmax, t2);
		//	if (tmin > tmax)
		//		return false;
		//	if (fixmath.Abs(localDirection.Y) < Toolbox.Epsilon && (localOrigin.Y < -halfHeight || localOrigin.Y > halfHeight))
		//		return false;
		//	inverseDirection = fp._1 / localDirection.Y;
		//	t1 = Fix64.SafeMul((-halfHeight - localOrigin.Y), inverseDirection);
		//	t2 = Fix64.SafeMul((halfHeight - localOrigin.Y), inverseDirection);
		//	tempNormal = new Vector3(F64.C0, -1, F64.C0);
		//	if (t1 > t2)
		//	{
		//		temp = t1;
		//		t1 = t2;
		//		t2 = temp;
		//		tempNormal *= -1;
		//	}
		//	temp = tmin;
		//	tmin = MathHelper.Max(tmin, t1);
		//	if (temp != tmin)
		//		normal = tempNormal;
		//	tmax = MathHelper.Min(tmax, t2);
		//	if (tmin > tmax)
		//		return false;
		//	if (fixmath.Abs(localDirection.Z) < Toolbox.Epsilon && (localOrigin.Z < -halfLength || localOrigin.Z > halfLength))
		//		return false;
		//	inverseDirection = fp._1 / localDirection.Z;
		//	t1 = Fix64.SafeMul((-halfLength - localOrigin.Z), inverseDirection);
		//	t2 = Fix64.SafeMul((halfLength - localOrigin.Z), inverseDirection);
		//	tempNormal = new Vector3(F64.C0, F64.C0, -1);
		//	if (t1 > t2)
		//	{
		//		temp = t1;
		//		t1 = t2;
		//		t2 = temp;
		//		tempNormal *= -1;
		//	}
		//	temp = tmin;
		//	tmin = MathHelper.Max(tmin, t1);
		//	if (temp != tmin)
		//		normal = tempNormal;
		//	tmax = MathHelper.Min(tmax, t2);
		//	if (tmin > tmax)
		//		return false;
		//	hit.T = tmin;
		//	Vector3.Multiply(ref ray.Direction, tmin, out hit.Location);
		//	Vector3.Add(ref hit.Location, ref ray.Position, out hit.Location);
		//	Quaternion.Transform(ref normal, ref transform.Orientation, out normal);
		//	hit.Normal = normal;
		//	return true;
		//}

		/// <summary>
		/// Retrieves an instance of an EntityCollidable that uses this EntityShape.  Mainly used by compound bodies.
		/// </summary>
		/// <returns>EntityCollidable that uses this shape.</returns>
		public override EntityCollidable GetCollidableInstance()
		{
			return new ConvexCollidable<BoxShape>(this);
		}

	}
}

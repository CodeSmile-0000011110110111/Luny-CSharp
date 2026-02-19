using System;
using System.Collections.Generic;

namespace Luny.Engine.Bridge
{

	/// <summary>
	/// Engine-agnostic proxy for native transform types (UnityEngine.Transform, Godot.Node3D, etc.).
	/// </summary>
	public abstract class LunyTransform
	{
		public abstract LunyVector3 Position { get; set; }
		public abstract LunyQuaternion Rotation { get; set; }
		public abstract LunyVector3 EulerAngles { get; set; }

		public abstract LunyVector3 LocalPosition { get; set; }
		public abstract LunyQuaternion LocalRotation { get; set; }
		public abstract LunyVector3 LocalEulerAngles { get; set; }
		public abstract LunyVector3 LocalScale { get; set; }

		public abstract LunyVector3 Forward { get; }
		public abstract LunyVector3 Back { get; }
		public abstract LunyVector3 Up { get; }
		public abstract LunyVector3 Down { get; }
		public abstract LunyVector3 Right { get; }
		public abstract LunyVector3 Left { get; }

		public abstract LunyTransform Parent { get; set; }
		public abstract LunyTransform Root { get; }
		public abstract Int32 ChildCount { get; }
		public abstract LunyTransform GetChild(Int32 index);
		public abstract IEnumerable<LunyTransform> Children { get; }
		public abstract void SetParent(LunyTransform parent, Boolean worldPositionStays = true);
		public abstract Boolean IsChildOf(LunyTransform parent);
		public abstract Int32 GetSiblingIndex();
		public abstract void SetSiblingIndex(Int32 index);
		public abstract void SetAsFirstSibling();
		public abstract void SetAsLastSibling();
		public abstract void DetachChildren();

		public abstract LunyVector3 TransformPoint(LunyVector3 point);
		public abstract LunyVector3 InverseTransformPoint(LunyVector3 point);
		public abstract LunyVector3 TransformDirection(LunyVector3 direction);
		public abstract LunyVector3 InverseTransformDirection(LunyVector3 direction);
		public abstract LunyVector3 TransformVector(LunyVector3 vector);
		public abstract LunyVector3 InverseTransformVector(LunyVector3 vector);

		public abstract void LookAt(LunyVector3 worldPosition);
		public abstract void LookAt(LunyVector3 worldPosition, LunyVector3 worldUp);
		public abstract void LookAt(ILunyObject target);
		public abstract void LookAt(ILunyObject target, LunyVector3 worldUp);
		public abstract void Rotate(LunyVector3 eulerAngles, LunySpace space = LunySpace.Self);
		public abstract void Rotate(LunyVector3 axis, Single angle, LunySpace space = LunySpace.Self);
		public abstract void OrbitAround(LunyVector3 worldPoint, LunyVector3 axis, Single angle);
		public abstract void Translate(LunyVector2 translation, LunySpace space = LunySpace.Self);
		public abstract void Translate(LunyVector3 translation, LunySpace space = LunySpace.Self);
	}
}

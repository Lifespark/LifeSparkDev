using UnityEngine;
using System.Collections;

namespace AGE
{
	
	[EventCategory("Utility")]
	public class CameraProperty : TickEvent
	{
		public override bool SupportEditMode ()
		{
			return true;
		}

		[ObjectTemplate]
		public int targetId = 0;

		public enum ProjectionType
		{
			Perspective,
			Orthographic,
		};
		public ProjectionType Projection = ProjectionType.Perspective;

		public float Size = 5;
		public float FOV = 60;
		public float NearPlane = 0.3f;
		public float FarPlane = 1000;
		public float Depth = -1;


		public override void Process (Action _action, Track _track)
		{
			GameObject targetObj = _action.GetGameObject(targetId);
			if (targetObj == null) 
				return;
			if (targetObj.camera == null)
				return;
			bool isOrtho = (Projection == ProjectionType.Orthographic);
			targetObj.camera.fieldOfView = FOV;
//			targetObj.camera.depth = Depth;
//			targetObj.camera.nearClipPlane = NearPlane;
//			targetObj.camera.farClipPlane = FarPlane;
			targetObj.camera.orthographicSize = Size;
			targetObj.camera.orthographic = isOrtho;
			targetObj.camera.isOrthoGraphic = isOrtho;
		}

		public override void ProcessBlend(Action _action, Track _track, TickEvent _prevEvent, float _blendWeight)
		{
			GameObject targetObj = _action.GetGameObject(targetId);
			if (targetObj == null || targetObj.camera == null || _prevEvent == null) 
				return;
			float minusBW = 1.0f - _blendWeight;
			targetObj.camera.fieldOfView 		= FOV       * _blendWeight + (_prevEvent as CameraProperty).FOV       * minusBW;
//			targetObj.camera.depth 				= Depth     * _blendWeight + (_prevEvent as CameraProperty).Depth     * minusBW;
//			targetObj.camera.nearClipPlane 		= NearPlane * _blendWeight + (_prevEvent as CameraProperty).NearPlane * minusBW;
//			targetObj.camera.farClipPlane 		= FarPlane  * _blendWeight + (_prevEvent as CameraProperty).FarPlane  * minusBW;
			targetObj.camera.orthographicSize 	= Size      * _blendWeight + (_prevEvent as CameraProperty).Size      * minusBW;
		}

	}

}

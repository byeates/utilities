/*
MIT License

Copyright (c) 2023 Bennett Yeates

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using Code.Common.Dispatchers;
using UnityEngine;

namespace Code.Common
{
	/// <summary>
	/// Scales a recttransform based a reference rect transforms width and height. I'm not super impressed with
	/// this script, and it feels a little janky. May need to revisit this idea a bit more, and actually focus
	/// on the components
	/// </summary>
	[ExecuteInEditMode]
	public class ScaleTransformByReference : MonoBehaviour
	{
		//====================
		// PRIVATE
		//====================
		[SerializeField] private RectTransform reference;
		[SerializeField] private RectTransform current;
		
		[Header("Scaling properties")]
		[Tooltip("Scales a percentage of the final scale. e.g. set to 0.9 to be 90% of the scaling")]
		[SerializeField] public float scalePercent = 1.0f;

		[Tooltip("When set to true will verify we do not exceed the caps on the opposite axis")]
		[SerializeField] public bool capOppositeAxis;
		
		[Tooltip("Will check the resulting scale of the opposite axis, and if it's larger than the cap we will adjust downward (only works for match modes width/height)")]
		[SerializeField] public float scalePercentCapOppositeAxis = 1.0f;
		[SerializeField] public bool runOnAwake = true;
		[SerializeField] public bool keepAspectRatio = true;

		[Tooltip("When set to will detect when a parent is set, and automatically start scaling")]
		[SerializeField] public bool autoDetectParent;
		
		[Tooltip("When set to true will adjust size deltas also. This is useful if you need the size delta to change for layout groups")]
		[SerializeField] public bool adjustSizeDeltas;
		
		[Tooltip("Stretch type to use. Smart stretch picks whichever axis has the largest delta to fill")]
		[SerializeField] public StretchType scaleType = StretchType.MATCH_WIDTH;
		
		[Tooltip("When set to true will adjust the position after scaling to maintain center")]
		[SerializeField] public bool run;

		private Vector3 originalScale;
		private Vector3 originalPosition;
		private Vector2 referenceSize;
		private Rect originalSize;
		private bool hasSetValues;
		
		public enum StretchType
		{
			MATCH_WIDTH,
			MATCH_HEIGHT,
			MATCH_ALL,
			SMART
		}

		public void SetScaleType(StretchType type)
		{
			scaleType = type;
		}
		
		public void SetScalePercent(float percent)
		{
			scalePercent = percent;
		}
		
		/// <summary>
		/// Opposite axis capping
		/// </summary>
		/// <param name="percent"></param>
		public void SetScalePercentCapAxis(float percent)
		{
			scalePercentCapOppositeAxis = percent;
		}

		/// <summary>
		/// Set a new reference assignment
		/// </summary>
		/// <param name="referenceTransform"></param>
		public void SetReference(RectTransform referenceTransform)
		{
			reference = referenceTransform;
		}

		/// <summary>
		/// Initialize the reference
		/// </summary>
		public void Init()
		{
			if (hasSetValues)
			{
				return;
			}

			if (current == null)
			{
				current = GetComponent<RectTransform>();
			}
			
			if (autoDetectParent)
			{
				reference = transform.parent as RectTransform;
			}
			
			originalSize = current.rect;
			originalScale = current.localScale;
			originalPosition = current.anchoredPosition;

			hasSetValues = true;
		}

		/// <summary>
		/// Will run the scaling logic
		/// </summary>
		public void Run()
		{
			Init();
			
			if (reference == null || current == null)
			{
				return;
			}
			
			referenceSize = new Vector2(reference.rect.width, reference.rect.height);
			Scale();
		}
		
		private void Awake()
		{
			ResolutionMonitor.RegisterHandler(OnResolutionChanged);

			if (runOnAwake && Application.isPlaying)
			{
				Init();
			}
		}

		private void OnDestroy()
		{
			ResolutionMonitor.UnregisterHandler(OnResolutionChanged);
		}

		protected void OnResolutionChanged(Vector2 deltas)
		{
			Reset();
		}

		/// <summary>
		/// Resets the scale to the original, and then reapplies new scaling
		/// </summary>
		public void Reset()
		{
			Init();
			current.localScale = originalScale;
			Run();
		}

		/// <summary>
		/// Will restore original scale, size deltas, and position. Then will run the scaling process
		/// with the most current values. This is basically a complete start over with whatever has changed
		/// </summary>
		public void Reinitialize()
		{
			if (hasSetValues)
			{
				hasSetValues = false;
				current.localScale = originalScale;
				current.anchoredPosition = originalPosition;
			}
			Run();
		}

		private void Update()
		{
			if (run)
			{
				run = false;
				Reinitialize();
			}

			if (!hasSetValues || reference == null)
			{
				return;
			}

			if (scaleType == StretchType.MATCH_WIDTH && referenceSize.x != reference.rect.width)
			{
				Reset();
			}
			else if (scaleType == StretchType.MATCH_HEIGHT && referenceSize.y != reference.rect.height)
			{
				Reset();
			}
			else if 
			(
				(referenceSize.y != reference.rect.height ||
				 referenceSize.x != reference.rect.width)
            )
			{
				Reset();
			} 
		}

		/*================================================================================
		SCALING METHODS		
		=================================================================================*/
		/// <summary>
		/// Grabs the new width and height, and funnels it to the appropriate methods
		/// </summary>
		private void Scale()
		{
			float deltaWidth = reference.rect.width / (originalSize.width * originalScale.x);
			float deltaHeight = reference.rect.height / (originalSize.height * originalScale.y);

			if (scaleType == StretchType.MATCH_WIDTH)
			{
				ScaleWidth(deltaWidth, deltaHeight, scalePercent);
			}
			else if (scaleType == StretchType.MATCH_HEIGHT)
			{
				ScaleHeight(deltaWidth, deltaHeight, scalePercent);
			}
			else if (scaleType == StretchType.MATCH_ALL)
			{
				ScaleAll(deltaWidth, deltaHeight, scalePercent);
			}
			else if (scaleType == StretchType.SMART)
			{
				if (deltaWidth > deltaHeight)
				{
					ScaleWidth(deltaWidth, deltaHeight, scalePercent);
				}
				else
				{
					ScaleHeight(deltaWidth, deltaHeight, scalePercent);
				}
			}
		}
		
		/// <summary>
		/// Scales the width from <see cref="StretchType"/>
		/// </summary>
		/// <param name="dw">the new calculated change in width</param>
		/// <param name="dh">the new calculated change in height</param>
		/// <param name="scaleBy">percent to scale by, typically <see cref="scalePercent"/></param>
		private void ScaleWidth(float dw, float dh, float scaleBy = 1f)
		{
			Vector3 scale = new Vector3();
			Vector3 pos = originalPosition;
			
			scale.x = dw;
			float dt = scale.x - originalScale.x;
				
			if (keepAspectRatio)
			{
				scale.y = (originalScale.y + dt);
			}
			
			if (capOppositeAxis)
			{
				float newHeight = originalSize.height * scale.y;
				if (newHeight > reference.rect.height * scalePercentCapOppositeAxis)
				{
					float ch = reference.rect.height * scalePercentCapOppositeAxis / newHeight;
					float dy = scale.y - (scale.y * ch);
					scale.y = ch * scale.y;
					
					if (keepAspectRatio)
					{
						scale.x -= dy;
					}
				}
			}

			ApplyFinalValues(scale, scaleBy, adjustSizeDeltas);
		}

		/// <summary>
		/// Scales the height from <see cref="StretchType"/>. See <see cref="Scale"/>
		/// </summary>
		/// <param name="dw">the new calculated change in width</param>
		/// <param name="dh">the new calculated change in height</param>
		/// <param name="scaleBy">percent to scale by, typically <see cref="scalePercent"/></param>
		private void ScaleHeight(float dw, float dh, float scaleBy = 1f)
		{
			Vector3 scale = new Vector3();
			Vector3 pos = originalPosition;
			
			scale.y = dh;
			float dt = scale.y - originalScale.y;
				
			if (keepAspectRatio)
			{
				scale.x = originalScale.x + dt;
			}
			
			if (capOppositeAxis)
			{
				float newWidth = originalSize.width * scale.x;
				if (newWidth > reference.rect.width * scalePercentCapOppositeAxis)
				{
					float cw = reference.rect.width * scalePercentCapOppositeAxis / newWidth;
					float dx = scale.x - (scale.x * cw);
					scale.y = cw * scale.x;
					
					if (keepAspectRatio)
					{
						scale.x -= dx;
					}
				}
			}
			
			ApplyFinalValues(scale, scaleBy, adjustSizeDeltas);
		}

		/// <summary>
		/// Scales all values from <see cref="StretchType"/>. See <see cref="Scale"/>
		/// </summary>
		/// <param name="dw">the new calculated change in width</param>
		/// <param name="dh">the new calculated change in height</param>
		/// <param name="scaleBy">percent to scale by, typically <see cref="scalePercent"/></param>
		private void ScaleAll(float dw, float dh, float scaleBy = 1f)
		{
			Vector3 scale = new Vector3(dw, dh);
			Vector3 pos = originalPosition;
			
			ApplyFinalValues(scale, scaleBy, adjustSizeDeltas);
		}

		/// <summary>
		/// Applies the final values to the transform
		/// </summary>
		/// <param name="scale">new scale</param>
		/// <param name="scaleBy">amount to scale by (typically <see cref="scalePercent"/></param>
		/// <param name="setDeltas">set the sizedelta values of the transform</param>
		private void ApplyFinalValues
		(
			Vector2 scale,
			float scaleBy = 1f,
			bool setDeltas = false
		)
		{
			scale.x *= scaleBy;
			scale.y *= scaleBy;

			current.localScale = scale;

			if (setDeltas)
			{
				current.sizeDelta = new Vector2(current.sizeDelta.x * scale.x, current.sizeDelta.y * scale.y);
			}
		}
	}
}
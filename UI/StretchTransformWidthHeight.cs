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
using UnityEngine;
using System.Collections;
using Code.Common.Dispatchers;

/// <summary>
/// Stretches a transform to match screen width/height or canvas (if linked), or scale based on the delta change in resolution
/// </summary>
[ExecuteInEditMode]
public class StretchTransformWidthHeight : MonoBehaviour
{
	//====================
	// PUBLIC
	//====================
	public enum StretchType
	{
		MATCH_WIDTH,
		MATCH_HEIGHT,
		MATCH_ALL,
		MATCH_MIN,
		MATCH_MAX
	}

	//====================
	// PRIVATE
	//====================
	[Tooltip("Target transform")]
	[SerializeField] private RectTransform stretchTarget;
	
	[Tooltip("Stretch type to use")]
	[SerializeField] private StretchType scaleType;

	[Tooltip("When set to true will operate on the localScale instead of the size of the transform")]
	[SerializeField] private bool useScaleInsteadOfSize;
	
	[Tooltip("When set to true, if stretch type is width or height it will keep the aspect ratio when scaled")]
	[SerializeField] private bool keepAspectRatio;
	
	[Tooltip("Set a canvas to scale towards instead of the screen width/height")]
	[SerializeField] private RectTransform canvas;

	[Tooltip("Will automatically search parent objects until it finds a root canvas")]
	[SerializeField] private bool autoDetectParentCanvas;

	[Tooltip("When set to true, after scaling is applied will rescale to fit the canvas")]
	[SerializeField] private bool scaleToFit;

	[Tooltip("Enter a value to offset (subtract or add) the resize by.")]
	[SerializeField] private Vector2 offsetBy = Vector2.zero;
	
	[SerializeField] private bool run;

	private Vector2 lastCanvasSize;

	private void Awake()
	{
		if (stretchTarget == null)
		{
			stretchTarget = (RectTransform)gameObject.transform;
		}
		ResolutionMonitor.RegisterHandler(OnResolutionChanged);
		run = true;
	}

	private void OnDestroy()
	{
		ResolutionMonitor.UnregisterHandler(OnResolutionChanged);
	}

	private void Update()
	{
		if (run)
		{
			run = false;
			OnResolutionChanged(Vector2.zero);
		}
		else if (canvas != null && canvas.sizeDelta != lastCanvasSize)
		{
			lastCanvasSize = canvas.sizeDelta;
			Run();
		}
	}
	
	public void Run()
	{
		FindAndSetCanvas();
		ApplyScaling();
		
		if (canvas != null)
		{
			lastCanvasSize = canvas.sizeDelta;
		}
	}

	/// <summary>
	/// Handler from resolution change monitor
	/// </summary>
	/// <param name="deltas"></param>
	protected void OnResolutionChanged(Vector2 deltas)
	{
		Run();
	}

	/// <summary>
	/// Applies scaling as needed
	/// </summary>
	protected void ApplyScaling()
	{
		if (scaleType == StretchType.MATCH_HEIGHT)
		{
			StretchHeight(TargetHeight);
		}
		else if (scaleType == StretchType.MATCH_WIDTH)
		{
			StretchWidth(TargetWidth);
		}
		else if (scaleType == StretchType.MATCH_MIN)
		{
			if (TargetWidth < TargetHeight)
			{
				StretchWidth(TargetWidth);
			}
			else
			{
				StretchHeight(TargetHeight);
			}
		}
		else if (scaleType == StretchType.MATCH_MAX)
		{
			if (TargetWidth < TargetHeight)
			{
				StretchHeight(TargetHeight);
			}
			else
			{
				StretchWidth(TargetWidth);
			}
		}
		else
		{
			StretchWidthAndHeight();
		}

		// check for "fill" type scaling, when set to match all this would be irrelevant
		if (scaleToFit && scaleType != StretchType.MATCH_ALL)
		{
			float sizeX = useScaleInsteadOfSize ? stretchTarget.localScale.x : stretchTarget.sizeDelta.x;
			float sizeY = useScaleInsteadOfSize ? stretchTarget.localScale.y : stretchTarget.sizeDelta.y;
			if (Mathf.RoundToInt(sizeX) < Mathf.RoundToInt(TargetWidth))
			{
				StretchWidth(TargetWidth);
			}
			else if (Mathf.RoundToInt(sizeY) < Mathf.RoundToInt(TargetHeight))
			{
				StretchHeight(TargetHeight);
			}
		}
	}

	protected void StretchWidth(float sizeX)
	{
		float sizeY = useScaleInsteadOfSize ? stretchTarget.localScale.y : stretchTarget.sizeDelta.y;
		
		if (keepAspectRatio)
		{
			float p = sizeX / (useScaleInsteadOfSize ? stretchTarget.localScale.x : stretchTarget.sizeDelta.x);
			sizeY *= p;
		}

		ApplyScalarValues(sizeX, sizeY);
	}

	protected void StretchHeight(float sizeY)
	{
		float sizeX = useScaleInsteadOfSize ? stretchTarget.localScale.x : stretchTarget.sizeDelta.x;
		
		if (keepAspectRatio)
		{
			float p = sizeY / (useScaleInsteadOfSize ? stretchTarget.localScale.y : stretchTarget.sizeDelta.y);
			sizeX *= p;
		}

		ApplyScalarValues(sizeX, sizeY);
	}

	private void StretchWidthAndHeight()
	{
		if (keepAspectRatio)
		{
			float targetSizeX = useScaleInsteadOfSize ? stretchTarget.localScale.x : stretchTarget.sizeDelta.x;
			float targetSizeY = useScaleInsteadOfSize ? stretchTarget.localScale.y : stretchTarget.sizeDelta.y;
			
			// need to take the larger delta between scaling directions, and apply the transformation
			// and then reapply it to opposite vector
			float deltaX = Mathf.Abs(targetSizeX - TargetWidth);
			float deltaY = Mathf.Abs(targetSizeY - TargetHeight);

			if (deltaY > deltaX)
			{
				float p = TargetHeight / targetSizeY;
				
				ApplyScalarValues(p * targetSizeX, TargetHeight);
			}
			else
			{
				float p = TargetWidth / targetSizeX;
				
				ApplyScalarValues(TargetWidth, p * targetSizeY);
			}
		}
		else
		{
			if (useScaleInsteadOfSize)
			{
				stretchTarget.localScale = new Vector3(TargetWidth+offsetBy.x, TargetHeight+offsetBy.y, stretchTarget.localScale.z);
			}
			else
			{
				stretchTarget.sizeDelta = new Vector2(TargetWidth+offsetBy.x, TargetHeight+offsetBy.y);
			}
		}
	}

	/*================================================================================
	ANCILLARY
	=================================================================================*/
	private void ApplyScalarValues(float x, float y)
	{
		if (useScaleInsteadOfSize)
		{
			stretchTarget.localScale = new Vector3(x+offsetBy.x, y + offsetBy.y, 1);
		}
		else
		{
			stretchTarget.sizeDelta = new Vector2(x+offsetBy.x, y+offsetBy.y);
		}
	}
	
	
	private void FindAndSetCanvas()
	{
		if (autoDetectParentCanvas && canvas == null)
		{
			Canvas foundCanvas = CommonUtils.FindCanvasFromGameObject(gameObject);

			if (foundCanvas != null)
			{
				canvas = foundCanvas.transform as RectTransform;
			}
		}
	}
	
	protected float TargetWidth
	{
		get
		{
			if (canvas == null)
			{
				return Screen.width;
			}

			return canvas.sizeDelta.x;
		}
	}
	
	protected float TargetHeight
	{
		get
		{
			if (canvas == null)
			{
				return Screen.height;
			}

			return canvas.sizeDelta.y;
		}
	}
}
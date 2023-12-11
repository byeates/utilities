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
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Stretches a transform to match the reference transform sizes, based on the image texture being used. This factors
/// in the the RectTransform scaling, and grabs the image automatically
/// </summary>
[ExecuteInEditMode]
public class StretchTransformByImage : MonoBehaviour
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

	[Tooltip("When set to true, if stretch type is width or height it will keep the aspect ratio when scaled")]
	[SerializeField] private bool keepAspectRatio;
	
	[Tooltip("Object reference we are comparing to for stretching")]
	[SerializeField] private RectTransform reference;

	[Tooltip("Will automatically search parent objects until it finds a root canvas")]
	[SerializeField] private bool autoDetectParentCanvas;

	[Tooltip("When set to true, after scaling is applied will rescale to fit the canvas")]
	[SerializeField] private bool scaleToFit;

	[Tooltip("Enter a value to offset (subtract or add) the resize by.")]
	[SerializeField] private Vector2 offsetBy = Vector2.zero;

	[SerializeField] private bool run;

	/// <summary>
	/// Last known size of the reference transform
	/// </summary>
	private Vector2 lastReferenceSize;

	/// <summary>
	/// Image we are comparing
	/// </summary>
	private Image image;

	/// <summary>
	/// Sprite reference by the image
	/// </summary>
	private Sprite sprite;

	private void Awake()
	{
		if (stretchTarget == null)
		{
			stretchTarget = (RectTransform)gameObject.transform;
		}

		image = stretchTarget.GetComponent<Image>();
		sprite = image.sprite;
		
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
		else if (reference != null && (reference.sizeDelta != lastReferenceSize || sprite != image.sprite))
		{
			lastReferenceSize = reference.sizeDelta;
			sprite = image.sprite;
			Run();
		}
	}
	
	public void Run()
	{
		if (image == null)
		{
			return;
		}
		
		FindAndSetCanvas();
		ApplyScaling();
		
		if (reference != null)
		{
			lastReferenceSize = reference.sizeDelta;
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
			float sizeX = ImageSize.x;
			float sizeY = ImageSize.y;
			
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
		Vector2 imageSize = ImageSize;
		float sizeY = imageSize.y;
		
		if (keepAspectRatio)
		{
			float p = sizeX / imageSize.x;
			sizeY *= p;
		}

		ApplyScalarValues(sizeX, sizeY);
	}

	protected void StretchHeight(float sizeY)
	{
		Vector2 imageSize = ImageSize;
		float sizeX =imageSize.x;
		
		if (keepAspectRatio)
		{
			float p = sizeY / imageSize.y;
			sizeX *= p;
		}

		ApplyScalarValues(sizeX, sizeY);
	}

	private void StretchWidthAndHeight()
	{
		Vector2 imageSize = ImageSize;
		
		if (keepAspectRatio)
		{
			float targetSizeX = imageSize.x;
			float targetSizeY = imageSize.y;
			
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
			stretchTarget.sizeDelta = new Vector2(TargetWidth+offsetBy.x, TargetHeight+offsetBy.y);
		}
	}

	/*================================================================================
	ANCILLARY
	=================================================================================*/
	private void ApplyScalarValues(float x, float y)
	{
		stretchTarget.sizeDelta = new Vector2(x+offsetBy.x, y+offsetBy.y);
	}

	private void FindAndSetCanvas()
	{
		if (autoDetectParentCanvas && reference == null)
		{
			Canvas foundCanvas = CommonUtils.FindCanvasFromGameObject(gameObject);

			if (foundCanvas != null)
			{
				reference = foundCanvas.transform as RectTransform;
			}
		}
	}

	protected Vector2 ImageSize => new Vector2(image.mainTexture.width * CurrentScale.x, image.mainTexture.height * CurrentScale.y);
	protected Vector2 CurrentScale => stretchTarget.localScale;
	
	protected float TargetWidth
	{
		get
		{
			if (reference == null)
			{
				return Screen.width;
			}

			return reference.sizeDelta.x;
		}
	}
	
	protected float TargetHeight
	{
		get
		{
			if (reference == null)
			{
				return Screen.height;
			}

			return reference.sizeDelta.y;
		}
	}
}
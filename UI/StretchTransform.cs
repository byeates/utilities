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
/// Stretches a transform to match screen width/height, or scale based on the delta change in resolution
/// </summary>
[ExecuteInEditMode]
public class StretchTransform : MonoBehaviour
{
	[SerializeField] private Transform stretchTarget;
	
	[Tooltip("When set to false, matches 1:1 to screen width/height pixels. Set to true scales based on percent changes")]
	[SerializeField] private bool usePercentScaling;
	[SerializeField] private StretchType scaleType;
	[SerializeField] private bool run;

	private Vector2 debugScreenSize = Vector2.zero;

	public enum StretchType
	{
		MATCH_WIDTH,
		MATCH_HEIGHT,
		MATCH_ALL
	}

	private void Awake()
	{
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
			if (debugScreenSize == Vector2.zero)
			{
				debugScreenSize = new Vector2(Screen.width, Screen.height);
			}
			run = false;

			Vector2 deltas = new Vector2(Screen.width / debugScreenSize.x, Screen.height / debugScreenSize.y);
			Vector3 localScale = stretchTarget.localScale;
			Vector3 newScale = new Vector3(localScale.x * deltas.x, localScale.y * deltas.y, localScale.z);

			ApplyScaling(newScale);
			debugScreenSize.x = Screen.width;
			debugScreenSize.y = Screen.height;
		}
	}

	protected void OnResolutionChanged(Vector2 deltas)
	{
		Vector3 newScale = Vector3.one;
		deltas.x = deltas.x == 0 ? 1 : deltas.x;
		deltas.x = deltas.y == 0 ? 1 : deltas.y;
		
		if (usePercentScaling)
		{
			Vector3 localScale = stretchTarget.localScale;
			newScale = new Vector3(localScale.x * deltas.x, localScale.y * deltas.y, localScale.z);
		}
		else
		{
			Vector3 localScale = stretchTarget.localScale;
			newScale = new Vector3(localScale.x * deltas.x, localScale.y * deltas.y, localScale.z);
		}

		ApplyScaling(newScale);
	}

	protected void ApplyScaling(Vector3 scale)
	{
		if (scaleType == StretchType.MATCH_HEIGHT)
		{
			StretchHeight(scale.y);
		}
		else if (scaleType == StretchType.MATCH_WIDTH)
		{
			StretchWidth(scale.x);
		}
		else
		{
			StretchWidth(scale.x);
			StretchHeight(scale.y);
		}
	}

	protected void StretchWidth(float xValue)
	{
		stretchTarget.localScale = new Vector3(xValue, stretchTarget.localScale.y, 1);
	}

	protected void StretchHeight(float yValue)
	{
		stretchTarget.localScale = new Vector3(stretchTarget.localScale.x, yValue, 1);
	}
}
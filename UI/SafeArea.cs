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

namespace Code.Common.UI
{
	/// <summary>
	/// Safe area anchoring/reposition for transforms based on device setting
	/// </summary>
	public class SafeArea : MonoBehaviour
	{
		//====================
		// PRIVATE
		//====================
		private RectTransform rectTransform;
		private Rect screenSafeArea;
		private Vector2 minAnchor;
		private Vector2 maxAnchor;

		private void Awake()
		{
			rectTransform = transform.GetComponent<RectTransform>();
			screenSafeArea = Screen.safeArea;
			minAnchor = screenSafeArea.position;
			maxAnchor = minAnchor + screenSafeArea.size;

			minAnchor.x /= Screen.width;
			minAnchor.y /= Screen.height;
			
			maxAnchor.x /= Screen.width;
			maxAnchor.y /= Screen.height;

			if (rectTransform != null)
			{
				rectTransform.anchorMin = minAnchor;
				rectTransform.anchorMax = maxAnchor;
			}
		}
	}
}
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
using UnityEngine.UI;

namespace Code.Common
{
	[RequireComponent(typeof(BoxCollider))]
	public class StretchBoxColliderToImage : MonoBehaviour
	{
		[SerializeField] private Image image;
		[SerializeField] private BoxCollider collider;
		
		[Tooltip("Applies a padded amount on top of the size")]
		[SerializeField] private float padding;

		private void Awake()
		{
			UpdateCollider();
		}

		public void UpdateCollider()
		{
			if (collider.size.x != image.rectTransform.sizeDelta.x || 
			    collider.size.y != image.rectTransform.sizeDelta.y)
			{
				if (padding <= 0f)
				{
					collider.size = image.rectTransform.sizeDelta;
				}
				else
				{
					Vector3 size = image.rectTransform.sizeDelta;
					size.x += padding;
					size.y += padding;
					collider.size = size;
				}
			}
		}
	}
}
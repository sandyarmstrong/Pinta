//
// EmbossEffect.cs
//  
// Author:
//       Marco Rolappe <m_rolappe@gmx.net>
// 
// Copyright (c) 2010 Marco Rolappe
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Cairo;
using Pinta.Gui.Widgets;


namespace Pinta.Core
{
	public class EmbossEffect : BaseEffect
	{
		public override string Icon {
			get { return "Menu.Effects.Stylize.Emboss.png"; }
		}

		public override string Text {
			get { return Mono.Unix.Catalog.GetString ("Emboss"); }
		}

		public override bool IsConfigurable {
			get { return true; }
		}

		public EmbossData Data {
			get { return EffectData as EmbossData; }
		}

		public EmbossEffect () {
			EffectData = new EmbossData ();
		}

		public override bool LaunchConfiguration () {
			return EffectHelper.LaunchSimpleEffectDialog (this);
		}

		#region Algorithm Code Ported From PDN
		unsafe public override void RenderEffect (ImageSurface src, ImageSurface dst, Gdk.Rectangle[] rois) {
			double[,] weights = Weights;

			var srcWidth = src.Width;
			var srcHeight = src.Height;

			ColorBgra* src_data_ptr = (ColorBgra*)src.DataPtr;
			
			foreach (var rect in rois) {
				// loop through each line of target rectangle
				for (int y = rect.Top; y < rect.Bottom; ++y) {
					int fyStart = 0;
					int fyEnd = 3;

					if (y == 0)
						fyStart = 1;

					if (y == srcHeight - 1)
						fyEnd = 2;
					
					// loop through each point in the line 
					ColorBgra* dstPtr = dst.GetPointAddress (rect.Left, y);

					for (int x = rect.Left; x < rect.Right; ++x) {
						int fxStart = 0;
						int fxEnd = 3;

						if (x == 0)
							fxStart = 1;

						if (x == srcWidth - 1)
							fxEnd = 2;

						// loop through each weight
						double sum = 0.0;

						for (int fy = fyStart; fy < fyEnd; ++fy) {
							for (int fx = fxStart; fx < fxEnd; ++fx) {
								double weight = weights[fy, fx];
								ColorBgra c = src.GetPointUnchecked (src_data_ptr, srcWidth, x - 1 + fx, y - 1 + fy);
								double intensity = (double)c.GetIntensityByte ();
								sum += weight * intensity;
							}
						}

						int iSum = (int)sum;
						iSum += 128;

						if (iSum > 255)
							iSum = 255;

						if (iSum < 0)
							iSum = 0;

						*dstPtr = ColorBgra.FromBgra ((byte)iSum, (byte)iSum, (byte)iSum, 255);

						++dstPtr;
					}
				}
			}
		}


		public double[,] Weights {
			get {
				// adjust and convert angle to radians
				double r = (double)Data.Angle * 2.0 * Math.PI / 360.0;

				// angle delta for each weight
				double dr = Math.PI / 4.0;

				// for r = 0 this builds an emboss filter pointing straight left
				double[,] weights = new double[3, 3];

				weights[0, 0] = Math.Cos (r + dr);
				weights[0, 1] = Math.Cos (r + 2.0 * dr);
				weights[0, 2] = Math.Cos (r + 3.0 * dr);

				weights[1, 0] = Math.Cos (r);
				weights[1, 1] = 0;
				weights[1, 2] = Math.Cos (r + 4.0 * dr);

				weights[2, 0] = Math.Cos (r - dr);
				weights[2, 1] = Math.Cos (r - 2.0 * dr);
				weights[2, 2] = Math.Cos (r - 3.0 * dr);

				return weights;
			}
		}
		#endregion


		public class EmbossData : EffectData
		{
			public double Angle = 0;
		}
	}
}

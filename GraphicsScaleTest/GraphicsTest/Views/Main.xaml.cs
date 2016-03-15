using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Windows.Graphics.Display;
using System.Numerics;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;

namespace GraphicsTest.Views
{
	public sealed partial class Main : Page
	{
		bool loadingBackgroundRecourcesComplete = false;
		bool loadingForegroundRecourcesComplete = false;
		CanvasImageBrush opacityMask;
		CanvasBitmap backgroundImage;
		CanvasImageBrush foregroundImage;
		double imageScalefactor;
		double imageContainerHeight;
		double imageContainerWidth;
		double displayScaling;

		public Main()
		{
			this.InitializeComponent();
		}

		private void CanvasControlBackground_Draw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			if (loadingBackgroundRecourcesComplete)
			{
				// Background
				args.DrawingSession.Units = CanvasUnits.Pixels;

				var scaleEffect = new ScaleEffect();
				scaleEffect.Source = backgroundImage;
				scaleEffect.Scale = new Vector2()
				{
					X = (float)imageScalefactor,
					Y = (float)imageScalefactor
				};

				var blurEffect = new GaussianBlurEffect();
				blurEffect.Source = scaleEffect;
				blurEffect.BlurAmount = 10f * (float)displayScaling;

				float backgroundVerticalAdjustment = 46f * (float)displayScaling; ;

				args.DrawingSession.DrawImage(blurEffect, 0.0f, (backgroundVerticalAdjustment * -1), new Rect(0, 0, imageContainerWidth, imageContainerHeight + backgroundVerticalAdjustment), 0.8f);
			}
		}

		private async void CanvasControlBackground_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			//Load image background
			backgroundImage = await CanvasBitmap.LoadAsync(sender.Device, new Uri("ms-appx:///Assets/test/card_picture_test1.jpg"));

			//Setting scalefactor
			displayScaling = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
			imageContainerWidth = sender.ActualWidth * displayScaling;
			imageContainerHeight = sender.ActualHeight * displayScaling;
			imageScalefactor = imageContainerWidth / backgroundImage.Size.Width;

			loadingBackgroundRecourcesComplete = true;

			sender.Invalidate();
		}

		private void CanvasControlForeground_Draw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			if (loadingForegroundRecourcesComplete)
			{
				// Foreground
				args.DrawingSession.FillGeometry(CanvasGeometry.CreateRectangle(sender, 0, 0, (float)imageContainerWidth, (float)imageContainerHeight), foregroundImage, opacityMask);
			}
		}

		private async void CanvasControlForeground_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			//Load image background
			backgroundImage = await CanvasBitmap.LoadAsync(sender.Device, new Uri("ms-appx:///Assets/test/card_picture_test1.jpg"));

			//Setting scalefactor
			displayScaling = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
			imageContainerWidth = sender.ActualWidth * displayScaling;
			imageContainerHeight = sender.ActualHeight * displayScaling;
			imageScalefactor = imageContainerWidth / backgroundImage.Size.Width;

			//Load image foreground
			foregroundImage = new CanvasImageBrush(sender.Device, backgroundImage)
			{
				SourceRectangle = new Rect(0, 0, UseSize(imageContainerWidth), UseSize(imageContainerHeight)),
				//Transform = Matrix3x2.CreateScale((float)imageScalefactor * 0.65f) * Matrix3x2.CreateTranslation(130f * (float)imageScalefactor, 70f * (float)imageScalefactor) // Scale and position of center picture
				//4" WVGA
				//Transform = Matrix3x2.CreateScale((float)imageScalefactor * 0.43f) * Matrix3x2.CreateTranslation(65f * (float)imageScalefactor, 45f * (float)imageScalefactor) // Scale and position of center picture

				//6" 1080p
				//Transform = Matrix3x2.CreateScale((float)imageScalefactor * 0.27f) * Matrix3x2.CreateTranslation(40f * (float)imageScalefactor, 25f * (float)imageScalefactor) // Scale and position of center picture

				//5,2" QHD
				//Transform = Matrix3x2.CreateScale((float)imageScalefactor * 0.17f) * Matrix3x2.CreateTranslation(27f * (float)imageScalefactor, 10f * (float)imageScalefactor) // Scale and position of center picture
				
				//Adjusted
				Transform = Matrix3x2.CreateScale((float)sender.ActualWidth * 0.0011f) * Matrix3x2.CreateTranslation(0.1697f * (float)sender.ActualWidth, 0.13f * (float)sender.ActualWidth)
			};

			//Create Opacity mask for foreground
			var circle = new CanvasCommandList(sender);

			using (var ds = circle.CreateDrawingSession())
			{
				//Small
				//ds.FillEllipse(180, 160, 80, 100, Colors.White); // Max width 360 // Position and size of center circle
				//Large
				//ds.FillEllipse(230, 180, 110, 110, Colors.White); // Max width 360 // Position and size of center circle
				//Combined by actualwidth
				//ds.FillEllipse((float)sender.ActualWidth * 0.532f, (float)sender.ActualWidth * 0.454f, (float)sender.ActualWidth * 0.242f, (float)sender.ActualWidth * 0.282f, Colors.White); // Max width 360 // Position and size of center circle
				ds.FillEllipse((float)sender.ActualWidth * 0.482f, (float)sender.ActualWidth * 0.454f, (float)sender.ActualWidth * 0.242f, (float)sender.ActualWidth * 0.282f, Colors.White); // Max width 360 // Position and size of center circle
			}

			var blurredCircle = new GaussianBlurEffect
			{
				Source = circle,
				BlurAmount = 3f * ((float)sender.ActualWidth/100)
			};

			opacityMask = new CanvasImageBrush(sender, blurredCircle)
			{
				SourceRectangle = new Rect(0, 0, UseSize(imageContainerWidth), UseSize(imageContainerHeight))
			};

			loadingForegroundRecourcesComplete = true;

			sender.Invalidate();
		}

		private double UseSize(double size)
		{
			int minSize = 640;
			return size < minSize ? minSize : size;
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			canvasControlBackground.RemoveFromVisualTree();
			canvasControlBackground = null;
			canvasControlForeground.RemoveFromVisualTree();
			canvasControlForeground = null;
		}
	}
}

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
		CanvasImageBrush foregroundImageCanvas;

		public Main()
		{
			this.InitializeComponent();
		}

		private void CanvasControlBackground_Draw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			if (loadingBackgroundRecourcesComplete)
			{
				// Draw background
				args.DrawingSession.Units = CanvasUnits.Dips;

				var scaleEffect = new ScaleEffect();
				scaleEffect.Source = backgroundImage;

				var blurEffect = new GaussianBlurEffect();
				blurEffect.Source = scaleEffect;
				blurEffect.BlurAmount = 15f;

				var sourceRect = new Rect(0, 0, backgroundImage.Size.Width, backgroundImage.Size.Height);

				double widthMinusHeight = sender.Size.Width - sender.Size.Height;

				var destinationRect = new Rect(0, 0, sender.Size.Width, sender.Size.Height + widthMinusHeight);

				args.DrawingSession.DrawImage(blurEffect, destinationRect, sourceRect);
			}
		}
		
		private async void CanvasControlBackground_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			//Load image background
			backgroundImage = await CanvasBitmap.LoadAsync(sender.Device, new Uri("ms-appx:///Assets/test/card_picture_test1.jpg"));

			loadingBackgroundRecourcesComplete = true;

			sender.Invalidate();
		}

		private void CanvasControlForeground_Draw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			args.DrawingSession.Units = CanvasUnits.Dips;

			if (loadingForegroundRecourcesComplete)
			{
				// Draw foreground
				args.DrawingSession.FillGeometry(CanvasGeometry.CreateRectangle(sender, 0, 0, (float)sender.Size.Width, (float)sender.Size.Height), foregroundImageCanvas, opacityMask); //remove opacityMask to see the foreground image fully
			}
		}

		private async void CanvasControlForeground_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			//Load image background
			var foregroundImage = await CanvasBitmap.LoadAsync(sender.Device, new Uri("ms-appx:///Assets/test/card_picture_test1.jpg"));

			var imageSizeFactor = 780;

			var imageSize = (float) sender.Size.Width / imageSizeFactor;
			var screenWidth = sender.Size.Width / imageSize;
			var screenHeight = sender.Size.Height / imageSize;

			var xStartPosition = ((screenWidth / 2) - ((foregroundImage.Size.Width) / 2)) * -1; // Position center
			var yStartPosition = (screenHeight - foregroundImage.Size.Height + 45) * -1; // Postion bottom + 45

			//Load image foreground
			foregroundImageCanvas = new CanvasImageBrush(sender.Device, foregroundImage)
			{
				SourceRectangle = new Rect(xStartPosition, yStartPosition, screenWidth, screenHeight),
				Transform =  Matrix3x2.CreateScale(imageSize) * Matrix3x2.CreateTranslation(0.0f, 0.0f) // Scale and position of center picture
			};

			//Create Opacity mask for foreground
			var circle = new CanvasCommandList(sender);

			using (var ds = circle.CreateDrawingSession())
			{
				ds.FillEllipse((float)sender.Size.Width * 0.5f, (float)sender.Size.Height * 0.7f, (float)sender.Size.Width * 0.24f, (float)sender.Size.Height * 0.46f, Colors.White); // Max width 360 // Position and size of center circle
			}

			var blurredCircle = new GaussianBlurEffect
			{
				Source = circle,
				BlurAmount = 15f
			};

			opacityMask = new CanvasImageBrush(sender, blurredCircle)
			{
				SourceRectangle = new Rect(0, 0, sender.Size.Width, sender.Size.Height)
			};

			loadingForegroundRecourcesComplete = true;

			sender.Invalidate();
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

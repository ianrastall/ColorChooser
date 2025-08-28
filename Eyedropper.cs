using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfPoint = System.Windows.Point;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace ColorPicker
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("gdi32.dll")]
        internal static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        internal static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
            IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        internal static extern int GetSystemMetrics(int nIndex);

        internal const int SM_CXSCREEN = 0;
        internal const int SM_CYSCREEN = 1;
        internal const uint SRCCOPY = 0x00CC0020;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT { public int X; public int Y; }

    internal class Eyedropper
    {
        public System.Windows.Media.Color? PickColor()
        {
            // Take a screenshot of the desktop first
            var screenshot = CaptureDesktop();
            if (screenshot == null)
            {
                // Fallback to the old method if screenshot fails
                var overlay = new EyedropperOverlay();
                return overlay.ShowDialog() == true ? overlay.SelectedColor : null;
            }

            var screenshotOverlay = new ScreenshotEyedropperOverlay(screenshot);
            return screenshotOverlay.ShowDialog() == true ? screenshotOverlay.SelectedColor : null;
        }

        private static BitmapSource? CaptureDesktop()
        {
            try
            {
                int screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
                int screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);

                IntPtr hdcScreen = NativeMethods.GetDC(IntPtr.Zero);
                IntPtr hdcMemDC = NativeMethods.CreateCompatibleDC(hdcScreen);
                IntPtr hBitmap = NativeMethods.CreateCompatibleBitmap(hdcScreen, screenWidth, screenHeight);
                IntPtr hOld = NativeMethods.SelectObject(hdcMemDC, hBitmap);

                NativeMethods.BitBlt(hdcMemDC, 0, 0, screenWidth, screenHeight, hdcScreen, 0, 0, NativeMethods.SRCCOPY);

                var bitmap = System.Drawing.Bitmap.FromHbitmap(hBitmap);
                var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                // Cleanup
                bitmap.Dispose();
                NativeMethods.SelectObject(hdcMemDC, hOld);
                NativeMethods.DeleteObject(hBitmap);
                NativeMethods.DeleteDC(hdcMemDC);
                NativeMethods.ReleaseDC(IntPtr.Zero, hdcScreen);

                return bitmapSource;
            }
            catch
            {
                return null;
            }
        }
    }

    internal class ScreenshotEyedropperOverlay : Window
    {
        public System.Windows.Media.Color? SelectedColor { get; private set; }
        private readonly BitmapSource _screenshot;
        private readonly DispatcherTimer _timer;
        private readonly Border _colorPreview;
        private readonly TextBlock _colorHex;
        private readonly TextBlock _instructions;
        private readonly Grid _previewContainer;
        private readonly TranslateTransform _previewTranslate = new();

        public ScreenshotEyedropperOverlay(BitmapSource screenshot)
        {
            _screenshot = screenshot;

            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Cursor = System.Windows.Input.Cursors.Cross;
            Topmost = true;

            // Create the main grid with screenshot background
            var mainGrid = new Grid();
            
            // Set the screenshot as background
            var screenshotBrush = new ImageBrush(_screenshot)
            {
                Stretch = Stretch.Fill
            };
            mainGrid.Background = screenshotBrush;

            // Add a subtle overlay to indicate it's interactive
            var overlayBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(10, 0, 0, 0));
            var overlayBorder = new Border { Background = overlayBrush };
            mainGrid.Children.Add(overlayBorder);

            // Create color preview with magnified area
            _colorPreview = new Border
            {
                Width = 150,
                Height = 150,
                BorderBrush = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(3),
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Visibility = Visibility.Collapsed,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    ShadowDepth = 5,
                    BlurRadius = 10
                }
            };

            // Color hex display
            _colorHex = new TextBlock
            {
                Foreground = System.Windows.Media.Brushes.White,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 0, 0, 0)),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                FontFamily = new System.Windows.Media.FontFamily("Consolas, Courier New"),
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };

            // Instructions
            _instructions = new TextBlock
            {
                Text = "🖱️ Click to select color • 🔍 Move mouse to preview • ⎋ ESC to cancel",
                Foreground = System.Windows.Media.Brushes.White,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0)),
                Padding = new Thickness(16, 8, 16, 8),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                FontSize = 16,
                Margin = new Thickness(0, 20, 0, 0)
            };

            // Container for moveable elements
            _previewContainer = new Grid { RenderTransform = _previewTranslate };
            _previewContainer.Children.Add(_colorPreview);
            _previewContainer.Children.Add(_colorHex);

            mainGrid.Children.Add(_previewContainer);
            mainGrid.Children.Add(_instructions);
            Content = mainGrid;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // ~60 FPS
            _timer.Tick += Timer_Tick;
            _timer.Start();

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            KeyDown += OnKeyDown;
            MouseMove += OnMouseMove;
        }

        private void OnMouseMove(object sender, WpfMouseEventArgs e)
        {
            // Show preview when mouse moves
            if (_colorPreview.Visibility == Visibility.Collapsed)
            {
                _colorPreview.Visibility = Visibility.Visible;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (NativeMethods.GetCursorPos(out POINT point))
            {
                // Convert screen coordinates to relative coordinates
                var relativePoint = PointFromScreen(new WpfPoint(point.X, point.Y));
                
                // Get color from screenshot at cursor position
                var color = GetColorFromScreenshot((int)relativePoint.X, (int)relativePoint.Y);
                
                // Create magnified view
                var magnifiedBrush = CreateMagnifiedBrush((int)relativePoint.X, (int)relativePoint.Y);
                _colorPreview.Background = magnifiedBrush;
                
                _colorHex.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}\nRGB({color.R}, {color.G}, {color.B})";
                
                // Position preview to avoid cursor
                var margin = 30;
                var previewX = point.X + margin;
                var previewY = point.Y + margin;
                
                // Keep preview on screen
                if (previewX + 150 > SystemParameters.PrimaryScreenWidth)
                    previewX = point.X - 150 - margin;
                if (previewY + 150 > SystemParameters.PrimaryScreenHeight)
                    previewY = point.Y - 150 - margin;
                
                _previewTranslate.X = previewX;
                _previewTranslate.Y = previewY;
            }
        }

        private System.Windows.Media.Color GetColorFromScreenshot(int x, int y)
        {
            try
            {
                // Ensure coordinates are within bounds
                x = Math.Max(0, Math.Min(_screenshot.PixelWidth - 1, x));
                y = Math.Max(0, Math.Min(_screenshot.PixelHeight - 1, y));

                // Get pixel data
                var stride = _screenshot.PixelWidth * 4; // 4 bytes per pixel (BGRA)
                var pixels = new byte[stride * _screenshot.PixelHeight];
                _screenshot.CopyPixels(pixels, stride, 0);

                var index = y * stride + x * 4;
                var b = pixels[index];
                var g = pixels[index + 1];
                var r = pixels[index + 2];

                return System.Windows.Media.Color.FromRgb(r, g, b);
            }
            catch
            {
                return Colors.White;
            }
        }

        private ImageBrush CreateMagnifiedBrush(int centerX, int centerY)
        {
            try
            {
                // Create a magnified version of the area around the cursor
                var magnificationFactor = 8;
                var captureSize = 19; // Capture 19x19 pixels for magnification
                var outputSize = captureSize * magnificationFactor;

                // Calculate capture bounds
                var left = Math.Max(0, centerX - captureSize / 2);
                var top = Math.Max(0, centerY - captureSize / 2);
                var right = Math.Min(_screenshot.PixelWidth, left + captureSize);
                var bottom = Math.Min(_screenshot.PixelHeight, top + captureSize);

                // Adjust if we're near edges
                left = Math.Max(0, right - captureSize);
                top = Math.Max(0, bottom - captureSize);

                var rect = new Int32Rect(left, top, right - left, bottom - top);
                var croppedBitmap = new CroppedBitmap(_screenshot, rect);

                // Create magnified version
                var magnifiedBitmap = new TransformedBitmap(croppedBitmap, 
                    new ScaleTransform(magnificationFactor, magnificationFactor));

                var brush = new ImageBrush(magnifiedBitmap)
                {
                    Stretch = Stretch.None,
                    TileMode = TileMode.None
                };

                return brush;
            }
            catch
            {
                // Fallback to solid color brush
                var color = GetColorFromScreenshot(centerX, centerY);
                return new ImageBrush() { ImageSource = null };
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);
            SelectedColor = GetColorFromScreenshot((int)position.X, (int)position.Y);
            DialogResult = true;
            Close();
        }

        private void OnKeyDown(object? sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            base.OnClosed(e);
        }
    }

    // Keep the old overlay as fallback
    internal class EyedropperOverlay : Window
    {
        public System.Windows.Media.Color? SelectedColor { get; private set; }
        private readonly DispatcherTimer _timer;
        private readonly Border _colorPreview;
        private readonly TextBlock _colorHex;
        private readonly Grid _previewContainer;
        private readonly TranslateTransform _previewTranslate = new();

        public EyedropperOverlay()
        {
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            Background = System.Windows.Media.Brushes.Transparent;
            WindowState = WindowState.Maximized;
            Cursor = System.Windows.Input.Cursors.Cross;

            var overlayBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0));
            var grid = new Grid { Background = overlayBrush };

            _colorPreview = new Border
            {
                Width = 100,
                Height = 50,
                BorderBrush = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(2),
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Visibility = Visibility.Collapsed
            };

            _colorHex = new TextBlock
            {
                Foreground = System.Windows.Media.Brushes.White,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 0, 0, 0)),
                Padding = new Thickness(5),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom
            };

            _previewContainer = new Grid { RenderTransform = _previewTranslate };
            _previewContainer.Children.Add(_colorPreview);
            _previewContainer.Children.Add(_colorHex);
            grid.Children.Add(_previewContainer);
            Content = grid;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            KeyDown += OnKeyDown;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (NativeMethods.GetCursorPos(out POINT point))
            {
                var color = GetPixelColor(point.X, point.Y);
                _colorPreview.Background = new SolidColorBrush(color);
                _colorHex.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                _previewTranslate.X = point.X + 20;
                _previewTranslate.Y = point.Y + 20;
                if (_colorPreview.Visibility == Visibility.Collapsed)
                {
                    _colorPreview.Visibility = Visibility.Visible;
                }
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (NativeMethods.GetCursorPos(out POINT point))
            {
                SelectedColor = GetPixelColor(point.X, point.Y);
            }
            DialogResult = true;
            Close();
        }

        private void OnKeyDown(object? sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private static System.Windows.Media.Color GetPixelColor(int x, int y)
        {
            IntPtr hdc = NativeMethods.GetDC(IntPtr.Zero);
            uint pixel = NativeMethods.GetPixel(hdc, x, y);
            NativeMethods.ReleaseDC(IntPtr.Zero, hdc);
            return System.Windows.Media.Color.FromRgb((byte)(pixel & 0xFF), (byte)((pixel >> 8) & 0xFF), (byte)((pixel >> 16) & 0xFF));
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            base.OnClosed(e);
        }
    }
}
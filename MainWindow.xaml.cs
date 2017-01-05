using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Drawing.Image;
using PixelFormat = System.Windows.Media.PixelFormat;
using Rectangle = System.Drawing.Rectangle;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<ImagePoint> _srcPoints = new List<ImagePoint>();
        private List<ImagePoint> _dstPoints = new List<ImagePoint>();
        private BitmapImage _srcImage;
        private BitmapImage _dstImage;

        public MainWindow()
        {
            InitializeComponent();
            InfoLabel.IsVisibleChanged += InfoLabel_IsVisibleChanged;
        }

        private void InfoLabel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (InfoLabel.IsVisible)
            {
                Task task = new Task(() =>
                {
                    System.Threading.Thread.Sleep(10000);
                    Application.Current.Dispatcher.Invoke(() => { InfoLabel.Visibility = Visibility.Hidden; });
                });
                task.Start();
            }
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


        private void BtnExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnLoadSrc_OnClick(object sender, RoutedEventArgs e)
        {
            _srcImage = OpenImage();
            //var image = ResizeImage(BitmapImage2Bitmap(_srcImage), (int) CanvasSrc.ActualWidth,
            //    (int) CanvasSrc.ActualHeight);
            ImgSrc.Source = _srcImage;
            ImgSrc.Width = CanvasSrc.ActualWidth;
            //ImageBrush brush = new ImageBrush();
            //brush.ImageSource = _srcImage;
            //CanvasSrc.Background = brush;
        }

        private void BtnLoadDst_OnClick(object sender, RoutedEventArgs e)
        {
            _dstImage = OpenImage();
            ImgDst.Source = _dstImage;
        }

        private BitmapImage OpenImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Images (*.bmp)|*.bmp"
            };
            if (openFileDialog.ShowDialog() != true)
                return null;
            return new BitmapImage(new Uri(openFileDialog.FileName));
        }

        private void ImgDst_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse ellipse = new Ellipse
            {
                Fill = Brushes.DarkRed,
                Width = 7,
                Height = 7,
                StrokeThickness = 2
            };

            CanvasDst.Children.Add(ellipse);

            Canvas.SetLeft(ellipse, e.GetPosition(ImgDst).X);
            Canvas.SetTop(ellipse, e.GetPosition(ImgDst).Y);
            _dstPoints.Add(new ImagePoint
            {
                X = (int)e.GetPosition(ImgDst).X,
                Y = (int)e.GetPosition(ImgDst).Y
            });

            LabelDst.Content = GetImagePointString(_dstPoints);
        }

        private string GetImagePointString(List<ImagePoint> points)
        {
            var res = "(";
            List<string> pointList = points.Select(imagePoint => "[" + imagePoint.X + ", " + imagePoint.Y + "]").ToList();
            res += string.Join(", ", pointList);
            res += ")";

            //var res = string.Join(",", points.Aggregate("(", (a, b) => a + "[" + b.X + ", " + b.Y + "]"));
            //var res = points.Aggregate("(", (current, point) => current + ("[" + point.X + ", " + point.Y + "]"));
            return res;
        }

        private void BtnOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckPreconditions(0))
            {
                return;
            }

            Bitmap source1 = BitmapImage2Bitmap(_srcImage); // your source images - assuming they're the same size
            Bitmap source2 = BitmapImage2Bitmap(_dstImage);
            var target = new Bitmap(source1.Width, source1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb); // Format32bppArgb
            var graphics = Graphics.FromImage(target);
            graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

            graphics.DrawImage(source1, 0, 0);

            source2 = ChangeOpacity(source2, 0.4f);
            graphics.DrawImage(source2, 0, 0);

            target.Save(AppDomain.CurrentDomain.BaseDirectory + "merged.png", ImageFormat.Png);
            BitmapImage myBitmap = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "merged.png"));
            ImgBoth.Source = myBitmap;
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public Bitmap ChangeOpacity(System.Drawing.Image img, float opacityvalue)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height); // Determining Width and Height of Source Image
            Graphics graphics = Graphics.FromImage(bmp);
            ColorMatrix colormatrix = new ColorMatrix
            {
                Matrix33 = opacityvalue
            };
            ImageAttributes imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
            graphics.Dispose();   // Releasing all resource used by graphics 
            return bmp;
        }

        private void ShowErrorInLabel(string message)
        {
            InfoLabel.Content = message;
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Foreground = Brushes.Red;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method">0: Only check for open images<br />1: Check for 1 Points<br />2: Check for 2 Points</param>
        /// <returns></returns>
        private bool CheckPreconditions(int method)
        {
            if (_srcImage == null || _dstImage == null)
            {
                ShowErrorInLabel("Please open a source and desination image.");
                return false;
            }
            if (method == 1 && (_srcPoints.Count < 1 || _dstPoints.Count < 1))
            {
                ShowErrorInLabel("Please set at lest 1 point in the source and 1 point in the destination image.");
                return false;
            }
            if (method == 2 && (_srcPoints.Count < 2 || _dstPoints.Count < 2))
            {
                ShowErrorInLabel("Please set at lest 2 points in the source and 2 points in the destination image.");
                return false;
            }
            if (method == 3 && (_srcPoints.Count < 3 || _dstPoints.Count < 3))
            {
                ShowErrorInLabel("Please set at lest 3 points in the source and 3 points in the destination image.");
                return false;
            }
            return true;
        }

        private void BtnTranslation_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckPreconditions(1))
            {
                return;
            }
            InfoLabel.Content = "Translation Not implemented";
            InfoLabel.Visibility = Visibility;
        }

        private void BtnRigid_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckPreconditions(2))
            {
                return;
            }
            InfoLabel.Content = "Rigid Not implemented";
            InfoLabel.Visibility = Visibility;
        }

        private void BtnAffine_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckPreconditions(3))
            {
                return;
            }
            InfoLabel.Content = "Affine Not implemented";
            InfoLabel.Visibility = Visibility;
        }

        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            _srcPoints.Clear();
            _dstPoints.Clear();

            LabelSrc.Content = "";
            LabelDst.Content = "";

            CanvasSrc.Children.RemoveRange(1, CanvasSrc.Children.Count - 1);
            CanvasDst.Children.RemoveRange(1, CanvasDst.Children.Count - 1);
        }

        private void CanvasSrc_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            Ellipse ellipse = new Ellipse
            {
                Fill = Brushes.DarkRed,
                Width = 7,
                Height = 7,
                StrokeThickness = 2
            };

            CanvasSrc.Children.Add(ellipse);

            Canvas.SetLeft(ellipse, e.GetPosition(CanvasSrc).X);
            Canvas.SetTop(ellipse, e.GetPosition(CanvasSrc).Y);
            _srcPoints.Add(new ImagePoint
            {
                X = (int)e.GetPosition(CanvasSrc).X,
                Y = (int)e.GetPosition(CanvasSrc).Y
            });

            LabelSrc.Content = GetImagePointString(_srcPoints);
        }

        private void ImgSrc_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}

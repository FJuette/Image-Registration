using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using itk.simple;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;
using Pen = System.Drawing.Pen;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;


namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Properties and Constructor

        private readonly List<ImagePoint> _srcPoints = new List<ImagePoint>();
        private readonly List<ImagePoint> _dstPoints = new List<ImagePoint>();
        private BitmapImage _srcImage;
        private BitmapImage _dstImage;

        private itk.simple.Image _soruceImageItk;
        private itk.simple.Image _destImageItk;

        public MainWindow()
        {
            InitializeComponent();
            InfoLabel.IsVisibleChanged += InfoLabel_IsVisibleChanged;
        }

        #endregion

        #region Image Actions (Template, Affine, SimpleElastix)

        private void BtnTemplate_OnClick(object sender, RoutedEventArgs e)
        {
            var bmp = BitmapImage2Bitmap(_srcImage);
            var bmpDst = BitmapImage2Bitmap(_dstImage);

            int x = 0;
            int y = 0;
            
            if (RbArgb.IsChecked != null && RbArgb.IsChecked.Value)
            {
                TemplateRegistration registrationArgb = new TemplateRegistration();
                registrationArgb.RunArgb(bmp, bmpDst);
                x = registrationArgb.X;
                y = registrationArgb.Y;
                Debug.WriteLine($"Mapping Argb\tMaximum: ({registrationArgb.X}, {registrationArgb.Y}, Value: {registrationArgb.Maximum})");
            }
            else if (RbHsb.IsChecked != null && RbHsb.IsChecked.Value)
            {
                TemplateRegistration registrationHsb = new TemplateRegistration();
                registrationHsb.RunHsb(bmp, bmpDst);
                x = registrationHsb.X;
                y = registrationHsb.Y;
                Debug.WriteLine($"Mapping Hue\tMaximum: ({registrationHsb.X}, {registrationHsb.Y}, Value: {registrationHsb.Maximum}");
            }
            
            ImgBoth.Source = DrawRectangle(BitmapImage2Bitmap(_dstImage), x - 1, y - 1, bmp.Width + 1, bmp.Height + 1);
            TabControl.SelectedIndex = 2;
            ShowInfoInLabel("Done");
        }

        private void BtnAffine_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckPreconditions(3))
                return;

            Transformation t = new Transformation();

            Point s1 = new Point(_dstPoints[0].X, _dstPoints[0].Y);
            Point s2 = new Point(_dstPoints[1].X, _dstPoints[1].Y);
            Point s3 = new Point(_dstPoints[2].X, _dstPoints[2].Y);

            Point d1 = new Point(_srcPoints[0].X, _srcPoints[0].Y);
            Point d2 = new Point(_srcPoints[1].X, _srcPoints[1].Y);
            Point d3 = new Point(_srcPoints[2].X, _srcPoints[2].Y);

            var res = t.Affine(_dstImage, s1, s2, s3, d1, d2, d3);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)_srcImage.Width,
                (int)_srcImage.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(res);
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }
            _dstImage = bitmapImage;
            BtnOverlay_OnClick(this, e);
        }
        
        private void BtnSimpleElastix_OnClick(object sender, RoutedEventArgs e)
        {
            SimpleElastix se = new SimpleElastix();

            se.SetFixedImage(_soruceImageItk);
            se.SetMovingImage(_destImageItk);
            se.SetParameterMap(SimpleITK.GetDefaultParameterMap("affine"));
            
            se.Execute();
            var result = se.GetResultImage();
            SimpleITK.WriteImage(result, AppDomain.CurrentDomain.BaseDirectory + @"resultBrain.nii");
            ShowInfoInLabel("Image: " + AppDomain.CurrentDomain.BaseDirectory + @"resultBrain.nii");
            Clipboard.SetText(AppDomain.CurrentDomain.BaseDirectory);
        }

        #endregion

        #region Standard Button Actions

        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            _srcPoints.Clear();
            _dstPoints.Clear();

            CanvasSrc.Children.RemoveRange(1, CanvasSrc.Children.Count - 1);
            CanvasDst.Children.RemoveRange(1, CanvasDst.Children.Count - 1);
        }

        private void BtnOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckPreconditions(0))
            {
                return;
            }

            Bitmap source1 = BitmapImage2Bitmap(_srcImage); // your source images - assuming they're the same size
            Bitmap source2 = BitmapImage2Bitmap(_dstImage);
            var target = new Bitmap(source1.Width, source1.Height, PixelFormat.Format32bppArgb); // Format32bppArgb
            var graphics = Graphics.FromImage(target);
            graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

            graphics.DrawImage(source1, 0, 0);

            source2 = ChangeOpacity(source2, 0.5f);
            graphics.DrawImage(source2, 0, 0);

            target.Save(AppDomain.CurrentDomain.BaseDirectory + "merged.png", ImageFormat.Png);
            BitmapImage myBitmap = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "merged.png"));
            ImgBoth.Source = myBitmap;
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

            ShowInfoInLabel(GetImagePointString(_srcPoints));
        }

        private void CanvasDst_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
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

            ShowInfoInLabel(GetImagePointString(_dstPoints));
        }

        private void BtnLoadSrc_OnClick(object sender, RoutedEventArgs e)
        {
            var path = OpenImage();
            var img = new BitmapImage(new Uri(path));
            _srcImage = img;
            ImgSrc.Source = _srcImage;
            _soruceImageItk = SimpleITK.ReadImage(path, PixelIDValueEnum.sitkFloat32);
        }

        private void BtnLoadDst_OnClick(object sender, RoutedEventArgs e)
        {
            var path = OpenImage();
            var img = new BitmapImage(new Uri(path));
            _dstImage = img;
            ImgDst.Source = _dstImage;
            _destImageItk = SimpleITK.ReadImage(path, PixelIDValueEnum.sitkFloat32);
        }

        private void BtnExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
        
        # region Helping Functions
        
        /// <summary>
        /// Draws a rectangle on a given image. Used by the template registraion
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private BitmapImage DrawRectangle(Image image, int x, int y, int width, int height)
        {
            var graphics = Graphics.FromImage(image);
            graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

            graphics.DrawRectangle(new Pen(Color.Red, 4f), x, y, width, height);

            Image result = new Bitmap(image);
            graphics.DrawImage(result, 0, 0);

            Random rnd = new Random();
            var num = rnd.Next(1, 1000);
            result.Save(AppDomain.CurrentDomain.BaseDirectory + num + "rectangle.png", ImageFormat.Png);
            BitmapImage myBitmap = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + num + "rectangle.png"));
            return myBitmap;
        }

        private string OpenImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*"
            };
            return openFileDialog.ShowDialog() != true ? null : openFileDialog.FileName;
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                var bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public Bitmap ChangeOpacity(Image img, float opacityvalue)
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

        private string GetImagePointString(List<ImagePoint> points)
        {
            var res = "(";
            List<string> pointList = points.Select(imagePoint => imagePoint.ToString()).ToList();
            res += string.Join(", ", pointList);
            res += ")";

            //var res = string.Join(",", points.Aggregate("(", (a, b) => a + "[" + b.X + ", " + b.Y + "]"));
            //var res = points.Aggregate("(", (current, point) => current + ("[" + point.X + ", " + point.Y + "]"));
            return res;
        }

        #endregion
        
        #region Info Functions
        
        private void InfoLabel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (InfoLabel.IsVisible)
            {
                Task task = new Task(() =>
                {
                    System.Threading.Thread.Sleep(15000);
                    Application.Current.Dispatcher.Invoke(() => { InfoLabel.Visibility = Visibility.Hidden; });
                });
                task.Start();
            }
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

        private void ShowErrorInLabel(string message)
        {
            InfoLabel.Content = message;
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Foreground = Brushes.Red;
        }

        private void ShowInfoInLabel(string message)
        {
            InfoLabel.Content = message;
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Foreground = Brushes.Black;
        }

        #endregion

        
    }
}

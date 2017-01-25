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
using itk.simple;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;


namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties and Constructor

        private List<ImagePoint> _srcPoints = new List<ImagePoint>();
        private List<ImagePoint> _dstPoints = new List<ImagePoint>();
        private BitmapImage _srcImage;
        private BitmapImage _dstImage;

        public MainWindow()
        {
            InitializeComponent();
            InfoLabel.IsVisibleChanged += InfoLabel_IsVisibleChanged;
        }

        #endregion

        #region Image Actions (Translate, Rotate and Scale)

        private void BtnTemplate_OnClick(object sender, RoutedEventArgs e)
        {
            var bmp = BitmapImage2Bitmap(_srcImage);
            var rgbMatrixTemplate = GetImageMatrix(bmp);

            var bmpDst = BitmapImage2Bitmap(_dstImage);
            var rgbMatrixMapping = GetImageMatrix(bmpDst);
            var test = "";

            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    var cfg = GetCfg(rgbMatrixMapping, rgbMatrixTemplate, i, j);
                    Debug.WriteLine(cfg);
                }
                
            }
            
        }

        private long GetCfg(Pixel[][] rgbMapping, Pixel[][] rgbTemplate, int currentRow, int currentCol)
        {
            long result = 0;
            for (int i = 0; i < rgbTemplate.Length; i++)
            {
                for (int j = 0; j < rgbTemplate[i].Length; j++)
                {
                    if (currentRow < rgbMapping.Length && currentCol < rgbMapping[i].Length)
                    {
                        result += Math.Abs(rgbTemplate[i][j].Argb) * Math.Abs(rgbMapping[i + currentRow][j + currentCol].Argb);
                    }
                }
            }
            return result;
        }

        private Pixel[][] GetImageMatrix(Bitmap image)
        {
            int hight = image.Height;
            int width = image.Width;

            Pixel[][] pixelMatrix = new Pixel[width][];
            for (int i = 0; i < width; i++)
            {
                pixelMatrix[i] = new Pixel[hight];
                for (int j = 0; j < hight; j++)
                {
                    Pixel p = new Pixel
                    {
                        X = i,
                        Y = j,
                        Argb = image.GetPixel(i, j).ToArgb()
                    };
                    pixelMatrix[i][j] = p;
                }
            }
            return pixelMatrix;
        }

        private void BtnTranslation_OnClick(object sender, RoutedEventArgs e)
        {
            if (CheckPreconditions(1))
            {
                Transformation transform = new Transformation();

                ImageOperations imageOps = new ImageOperations(BitmapImage2Bitmap(_srcImage));
                //imageOps.SetTranslate(_dstPoints[0].X - _srcPoints[0].X, _dstPoints[0].Y - _srcPoints[0].Y);
                imageOps.SetRotate(80);
                var result = imageOps.ApplyTransform();
                result.Save(@"G:\test.png");
                return;

                var sourceVector = new MyVector(_srcPoints[0].X, _srcPoints[0].Y);
                var destVector = new MyVector(_dstPoints[0].X, _dstPoints[0].Y);
                var matrix = transform.GetTranslationsMatrix(sourceVector, destVector);

                InfoLabel.Content = transform.MatrixToString(matrix);

                var inverse = transform.InvertMatrix(matrix);
                InfoLabel2.Content = transform.MatrixToString(inverse);

                var vector = transform.MultiplyMatrixWithVector(inverse, destVector);
                InfoLabel3.Content = vector.X + "\n" + vector.Y;
            }
            //InfoLabel.Content = "Translation Not implemented";
            //InfoLabel.Visibility = Visibility;
        }

        private void BtnRigid_OnClick(object sender, RoutedEventArgs e)
        {
            ImageOperations imageOps = new ImageOperations(BitmapImage2Bitmap(_dstImage));
            imageOps.SetRotate(90); //TODO this must be dynamic -> How to mess similarity?
            // Die Farbinsentität an 3 Punkten muss dem des Ausgangsbildes entsprechen - Klappt das?ö
            var result = imageOps.ApplyTransform();
            result.Save(@"G:\test.png");
            BitmapImage img = new BitmapImage(new Uri(@"G:\test.png"));
            ImgDst.Source = img;
            _dstImage = img;
            return;
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

        private void SimpleElastixTest()
        {
            // Currently not working
            SimpleElastix se = new SimpleElastix();

            se.SetFixedImage(SimpleITK.ReadImage(@"F:\BrainProtonDensity.bmp"));
            se.SetMovingImage(SimpleITK.ReadImage(@"F:\BrainProtonDensityTranslatedR1013x17y.bmp"));
            se.SetParameterMap(SimpleITK.GetDefaultParameterMap("rigid"));
            se.Execute();
            SimpleITK.WriteImage(se.GetResultImage(), @"F:\result.bmp");
        }

        private bool IsSimilar(Bitmap dstBitmap)
        {
            var x = _srcPoints[0].X;
            var y = _srcPoints[0].Y;
            var bitmap = BitmapImage2Bitmap(_srcImage);
            // For each point in src and in dst
            var color = bitmap.GetPixel(x, y);
            var srcArgb = color.ToArgb();

            x = _dstPoints[0].X; // These cant work -> draw it on paper
            y = _dstPoints[0].Y;
            color = dstBitmap.GetPixel(x, y);
            var dstArgb = color.ToArgb();

            if (srcArgb == dstArgb)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Standard Button Actions
        
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            _srcPoints.Clear();
            _dstPoints.Clear();

            InfoLabel.Content = "";
            InfoLabel.Content = "";

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
            var target = new Bitmap(source1.Width, source1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb); // Format32bppArgb
            var graphics = Graphics.FromImage(target);
            graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

            graphics.DrawImage(source1, 0, 0);

            source2 = ChangeOpacity(source2, 0.5f);
            graphics.DrawImage(source2, 0, 0); //TODO here I can set x and y position if the second image, i can use this for translation

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

            InfoLabel.Content = GetImagePointString(_srcPoints);
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

            InfoLabel.Content = GetImagePointString(_dstPoints);
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

        private void BtnExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
        
        # region Helping Functions

        private BitmapImage OpenImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*|Images (*.bmp)|*.bmp"
            };
            if (openFileDialog.ShowDialog() != true)
                return null;
            return new BitmapImage(new Uri(openFileDialog.FileName));
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

        #endregion
        
        #region Info Functions
        
        private void InfoLabel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (InfoLabel.IsVisible)
            {
                Task task = new Task(() =>
                {
                    System.Threading.Thread.Sleep(10000);
                    Application.Current.Dispatcher.Invoke(() => { InfoLabel.Visibility = Visibility.Hidden; });
                });
                //task.Start();
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

        #endregion

        
    }
}

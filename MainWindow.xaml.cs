using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace SvgEditorSample
{
    public partial class MainWindow : Window
    {
        private Shape? _selectedShape;
        private Point _dragStart;
        private Point _shapeStart;

        public MainWindow()
        {
            InitializeComponent();
        }

        // 矩形追加
        private void AddRect_Click(object sender, RoutedEventArgs e)
        {
            var rect = new Rectangle
            {
                Width = 100,
                Height = 60,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.DarkBlue,
                StrokeThickness = 2
            };

            Canvas.SetLeft(rect, 50);
            Canvas.SetTop(rect, 50);

            AttachShapeEvents(rect);
            DrawCanvas.Children.Add(rect);
        }

        // 円追加
        private void AddEllipse_Click(object sender, RoutedEventArgs e)
        {
            var ellipse = new Ellipse
            {
                Width = 80,
                Height = 80,
                Fill = Brushes.LightPink,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };

            Canvas.SetLeft(ellipse, 200);
            Canvas.SetTop(ellipse, 80);

            AttachShapeEvents(ellipse);
            DrawCanvas.Children.Add(ellipse);
        }

        // 図形にマウスイベントを付与
        private void AttachShapeEvents(Shape shape)
        {
            shape.MouseLeftButtonDown += Shape_MouseLeftButtonDown;
        }

        private void Shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _selectedShape = sender as Shape;
            if (_selectedShape == null) return;

            _dragStart = e.GetPosition(DrawCanvas);
            _shapeStart = new Point(
                Canvas.GetLeft(_selectedShape),
                Canvas.GetTop(_selectedShape));

            _selectedShape.CaptureMouse();
            e.Handled = true;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // キャンバスクリックで選択解除
            if (_selectedShape != null)
            {
                _selectedShape.ReleaseMouseCapture();
                _selectedShape = null;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_selectedShape != null)
            {
                _selectedShape.ReleaseMouseCapture();
                _selectedShape = null;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_selectedShape == null || !_selectedShape.IsMouseCaptured) return;

            var pos = e.GetPosition(DrawCanvas);
            var dx = pos.X - _dragStart.X;
            var dy = pos.Y - _dragStart.Y;

            Canvas.SetLeft(_selectedShape, _shapeStart.X + dx);
            Canvas.SetTop(_selectedShape, _shapeStart.Y + dy);
        }

        // SVG保存
        private void SaveSvg_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "SVG Files (*.svg)|*.svg",
                DefaultExt = "svg"
            };

            if (dlg.ShowDialog() == true)
            {
                var svg = GenerateSvgFromCanvas();
                File.WriteAllText(dlg.FileName, svg, Encoding.UTF8);
                MessageBox.Show("SVGとして保存しました。", "保存", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        // Canvas上のRectangle / EllipseをSVGに変換
        private string GenerateSvgFromCanvas()
        {
            var sb = new StringBuilder();

            double width = DrawCanvas.Width;
            double height = DrawCanvas.Height;

            sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" " +
                          $"width=\"{width.ToString(CultureInfo.InvariantCulture)}\" " +
                          $"height=\"{height.ToString(CultureInfo.InvariantCulture)}\" " +
                          $"viewBox=\"0 0 {width.ToString(CultureInfo.InvariantCulture)} {height.ToString(CultureInfo.InvariantCulture)}\">");

            foreach (var child in DrawCanvas.Children)
            {
                if (child is Rectangle rect)
                {
                    double x = Canvas.GetLeft(rect);
                    double y = Canvas.GetTop(rect);

                    string fill = ((SolidColorBrush)rect.Fill).Color.ToString();
                    string stroke = ((SolidColorBrush)rect.Stroke).Color.ToString();
                    double strokeWidth = rect.StrokeThickness;

                    sb.AppendLine(
                        $"  <rect x=\"{x.ToString(CultureInfo.InvariantCulture)}\" " +
                        $"y=\"{y.ToString(CultureInfo.InvariantCulture)}\" " +
                        $"width=\"{rect.Width.ToString(CultureInfo.InvariantCulture)}\" " +
                        $"height=\"{rect.Height.ToString(CultureInfo.InvariantCulture)}\" " +
                        $"fill=\"{fill}\" stroke=\"{stroke}\" " +
                        $"stroke-width=\"{strokeWidth.ToString(CultureInfo.InvariantCulture)}\" />");
                }
                else if (child is Ellipse ellipse)
                {
                    double x = Canvas.GetLeft(ellipse);
                    double y = Canvas.GetTop(ellipse);

                    double cx = x + ellipse.Width / 2.0;
                    double cy = y + ellipse.Height / 2.0;
                    double rx = ellipse.Width / 2.0;
                    double ry = ellipse.Height / 2.0;

                    string fill = ((SolidColorBrush)ellipse.Fill).Color.ToString();
                    string stroke = ((SolidColorBrush)ellipse.Stroke).Color.ToString();
                    double strokeWidth = ellipse.StrokeThickness;

                    sb.AppendLine(
                        $"  <ellipse cx=\"{cx.ToString(CultureInfo.InvariantCulture)}\" " +
                        $"cy=\"{cy.ToString(CultureInfo.InvariantCulture)}\" " +
                        $"rx=\"{rx.ToString(CultureInfo.InvariantCulture)}\" " +
                        $"ry=\"{ry.ToString(CultureInfo.InvariantCulture)}\" " +
                        $"fill=\"{fill}\" stroke=\"{stroke}\" " +
                        $"stroke-width=\"{strokeWidth.ToString(CultureInfo.InvariantCulture)}\" />");
                }
            }

            sb.AppendLine("</svg>");
            return sb.ToString();
        }
    }
}

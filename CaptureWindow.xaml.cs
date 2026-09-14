using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Point = System.Windows.Point;

namespace AudioToMicWPF
{
    public partial class CaptureWindow : Window
    {
        private Point? _firstPoint;
        private Point? _currentPoint;
        private System.Drawing.Rectangle? _capturedRect;
        private bool _isFinalized;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        public System.Drawing.Rectangle? CapturedRect => _isFinalized ? _capturedRect : null;

        public CaptureWindow()
        {
            InitializeComponent();

            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = 0;
            Top = 0;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            ButtonPanel.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            ButtonPanel.Arrange(new Rect(0, 0, ButtonPanel.DesiredSize.Width, ButtonPanel.DesiredSize.Height));
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
            {
                CancelSelection();
            }
        }

        private void CanvasRoot_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isFinalized) return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var clickPoint = e.GetPosition(this);

                if (!_firstPoint.HasValue)
                {
                    _firstPoint = clickPoint;
                    SelectionRect.Visibility = Visibility.Visible;
                    UpdateSelectionRect(clickPoint, clickPoint);
                }
                else
                {
                    _currentPoint = clickPoint;
                    FinalizeSelection();
                    ShowConfirmationButtons();
                    _isFinalized = true;
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                ResetSelection();
            }
        }

        private void CanvasRoot_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isFinalized) return;

            if (_firstPoint.HasValue && !_currentPoint.HasValue)
            {
                var currentPos = e.GetPosition(this);
                UpdateSelectionRect(_firstPoint.Value, currentPos);
            }
        }

        private void CanvasRoot_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isFinalized) return;

            if (_firstPoint.HasValue && !_currentPoint.HasValue)
            {
                var currentPos = e.GetPosition(this);
                if (Math.Abs(currentPos.X - _firstPoint.Value.X) > 8 &&
                    Math.Abs(currentPos.Y - _firstPoint.Value.Y) > 8)
                {
                    _currentPoint = currentPos;
                    FinalizeSelection();
                    ShowConfirmationButtons();
                    _isFinalized = true;
                }
            }
        }

        private void UpdateSelectionRect(Point start, Point end)
        {
            var (x1, y1, x2, y2) = NormalizePoints(start, end);

            Canvas.SetLeft(SelectionRect, x1);
            Canvas.SetTop(SelectionRect, y1);
            SelectionRect.Width = Math.Max(1, x2 - x1);
            SelectionRect.Height = Math.Max(1, y2 - y1);
        }

        private (double x1, double y1, double x2, double y2) NormalizePoints(Point p1, Point p2)
        {
            return (
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Max(p1.X, p2.X),
                Math.Max(p1.Y, p2.Y)
            );
        }

        private void ShowConfirmationButtons()
        {
            if (!_firstPoint.HasValue || !_currentPoint.HasValue) return;

            var (_, _, x2, y2) = NormalizePoints(_firstPoint.Value, _currentPoint.Value);

            ButtonPanel.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            double panelW = ButtonPanel.DesiredSize.Width > 0 ? ButtonPanel.DesiredSize.Width : 220;
            double panelH = ButtonPanel.DesiredSize.Height > 0 ? ButtonPanel.DesiredSize.Height : 50;

            double targetX = x2 - panelW;
            if (targetX < 12) targetX = 12;
            double maxX = ActualWidth - panelW - 12;
            if (targetX > maxX) targetX = maxX;

            double targetY = y2 + 10;
            if (targetY + panelH > ActualHeight - 12)
            {
                targetY = y2 - panelH - 10;
            }
            if (targetY < 12) targetY = 12;

            Canvas.SetLeft(ButtonPanel, targetX);
            Canvas.SetTop(ButtonPanel, targetY);

            ButtonPanel.Visibility = Visibility.Visible;
            Panel.SetZIndex(ButtonPanel, 9999);
        }

        private void FinalizeSelection()
        {
            if (!_firstPoint.HasValue || !_currentPoint.HasValue) return;

            var start = PointToScreen(_firstPoint.Value);
            var end = PointToScreen(_currentPoint.Value);

            _capturedRect = new System.Drawing.Rectangle(
                (int)Math.Min(start.X, end.X),
                (int)Math.Min(start.Y, end.Y),
                (int)Math.Abs(end.X - start.X),
                (int)Math.Abs(end.Y - start.Y)
            );
        }

        private void ResetSelection()
        {
            _firstPoint = null;
            _currentPoint = null;
            _capturedRect = null;
            _isFinalized = false;
            SelectionRect.Visibility = Visibility.Collapsed;
            ButtonPanel.Visibility = Visibility.Collapsed;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetSelection();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelSelection();
        }

        private void CancelSelection()
        {
            ResetSelection();
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (!_isFinalized)
            {
                _capturedRect = null;
            }
            base.OnClosed(e);
            Application.Current.MainWindow?.Activate();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace WpfApp04.Controls
{
    /// <summary>
    /// Interaction logic for InclineEditor.xaml
    /// </summary>
    public partial class InclineEditor 
    {
        public string Title => "Уклоны";
        public string ContentId => "InclineEditor";

        public MainWindow mainWindow;
        private bool _lockHandlers;

        private readonly DrawingVisual _kmVisual = new DrawingVisual();
        private readonly DrawingVisual _inclineVisual = new DrawingVisual();
        private readonly Rectangle _inclineRectange = new Rectangle
        {
            IsHitTestVisible = false,
            Height = ControlHeight,
            Fill = new SolidColorBrush(Color.FromArgb(50, 50,255,255))
        };


        public List<Segment> segments;
        public List<Kilometer> kilometers;
        public List<PointOnTrack> pointOnTracks;
        public List<Incline> inclines;

        public Segment _segment;
        private Incline _incline;
        private double _coordinate = -1;
        private EditMode _editMode;

        private bool _isMoving;
        private int _movingInclineIndex = -1;
        private double _movingInclineOffset;
        private const double PointAccuracy = 10;
        private const double MaxIncline = 40;
        private enum EditMode
        {
            Horizontal,
            Vertical,
            Manual
        }

        private double _pixelsInMeter = 0.1;
        private double _metersInPixel = 1;


        
        private readonly Pen _kmPen = new Pen(Brushes.Green, 2d);
        private readonly Pen _pkPen = new Pen(Brushes.DimGray, 1d);
        private readonly Pen _inclinePen = new Pen(Brushes.Red, 2d);
        private const double InclineHeight = 160;
        private const double ControlHeight = 216;
        private const double KmTextHeight = InclineHeight + 26;
        private const double PkTextHeight = InclineHeight + 5;
        private const double InclineTextHeight = 5;

        public InclineEditor(List<Segment> _segments, 
            List<Kilometer> _kilometers, 
            List<PointOnTrack> _pointOnTracks,
            List<Incline> _inclines
            )
        {
            InitializeComponent();

            inclines = _inclines;
            pointOnTracks = _pointOnTracks;
            kilometers = _kilometers;
            segments = _segments;

            _segment = _segments[0];


            _kmPen.Freeze();
            _pkPen.Freeze();
            _inclinePen.Freeze();

            Panel.SetZIndex(_inclineRectange, -1);
            DrawingCanvas.Children.Add(_inclineRectange);
            DrawingCanvas.AddVisual(_kmVisual);
            DrawingCanvas.AddVisual(_inclineVisual);
            DrawingCanvas.Height = ControlHeight;


            


            SetScale();
            this.kilometers = _kilometers;


        }

        private void UpdateIncline()
        {
            UpdateGraphics();
        }

        private void UpdateGraphics()
        {
            DrawKilometers();
            DrawInclines();
            DrawSelectedIncline();
        }

        
        private void DrawKilometers()
        {
            using var dc = _kmVisual.RenderOpen();


            // dc.DrawLine(_kmPen, new Point(0,0), new Point(segments[0].SegmentLength/2,100));



            if (_segment == null) return;
            if (kilometers.Count == 0)
            {
                //DrawKilometersStartToEnd(dc, _segment.length);
            }
            else
            {
               // DrawKilometersStartToEnd(dc, kilometers[0].pointOnTrack.coordinate);

                var pkStep = 100d * _pixelsInMeter;


                for (var i = 0; i < kilometers.Count; i++)
                {


                    var km = kilometers[i];
                    var x0 = kilometers[i].Start.RouteCoordinate * _pixelsInMeter;
                    var x1 = kilometers[i].End.RouteCoordinate * _pixelsInMeter;

                    int startSegmentID = segments.FindLastIndex(x =>(x.SegmentID == km.Start.SegmentID ));

                    //var x1 = i + 1 < kilometers.Count
                    //    ? kilometers[i + 1].End.RouteCoordinate * _pixelsInMeter
                    //    : _segment.length * _pixelsInMeter;

                    FormattedText text;

                    var prevCoordinate = x0;                   

                    dc.DrawLine(_kmPen, new Point(x0, InclineHeight), new Point(x0, ControlHeight));


                    if (segments[startSegmentID].PredefinedRouteSegmentFromStartToEnd == 1)
                    {
                        double pk = km.Start.PointOnTrackPk;


                        // первый пикет
                        Rect rect2 = new Rect();
                        rect2.Height = 20;
                        rect2.Width = (100d - km.Start.PointOnTrackM) * _pixelsInMeter;
                        rect2.X = prevCoordinate;
                        rect2.Y = InclineHeight;
                        dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect2);

                        text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                        //if (x1 - prevCoordinate > text.Width) 
                        dc.DrawText(text, new Point(pk >= 10 ? prevCoordinate - 2 : prevCoordinate + 1, PkTextHeight));

                        prevCoordinate += (100d - km.Start.PointOnTrackM) * _pixelsInMeter;
                        pk++;


                        //пикеты
                        for (var coordinate = prevCoordinate + pkStep; coordinate < x1; coordinate += pkStep)
                        {


                            //dc.DrawLine(_pkPen, new Point(coordinate, InclineHeight), new Point(coordinate, ControlHeight));

                            Rect rect = new Rect();
                            rect.Height = 20;
                            rect.Width = pkStep;
                            rect.X = prevCoordinate;
                            rect.Y = InclineHeight;
                            dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect);

                            // пикеты текст
                            text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                                FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                            //if (coordinate - prevCoordinate > text.Width)
                            dc.DrawText(text, new Point(pk >= 10 ? prevCoordinate - 2 : prevCoordinate + 1, PkTextHeight));

                            prevCoordinate = coordinate;
                            pk++;
                        }

                        // последний пикет
                        Rect rect1 = new Rect();
                        rect1.Height = 20;
                        rect1.Width = x1;
                        rect1.X = prevCoordinate;
                        rect1.Y = InclineHeight;
                        dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect1);

                        text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                        //if (x1 - prevCoordinate > text.Width) 
                        dc.DrawText(text, new Point(pk >= 10 ? prevCoordinate - 2 : prevCoordinate + 1, PkTextHeight));
                    }
                    else // убывающий километраж
                    {
                        double pk = km.End.PointOnTrackPk;

                        prevCoordinate = x1;

                        // первый пикет
                        Rect rect2 = new Rect();
                        rect2.Height = 20;
                        rect2.Width = (100d - km.End.PointOnTrackM) * _pixelsInMeter;
                        rect2.X = prevCoordinate - rect2.Width;
                        rect2.Y = InclineHeight;
                        dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect2);

                        text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                        //if (x1 - prevCoordinate > text.Width) 
                        dc.DrawText(text, new Point(pk >= 10 ? prevCoordinate - rect2.Width - 2 : prevCoordinate - rect2.Width + 1, PkTextHeight));

                        prevCoordinate -= (100d - km.End.PointOnTrackM) * _pixelsInMeter;
                        pk++;


                        //пикеты
                        for (var coordinate = prevCoordinate - pkStep; coordinate > x0; coordinate -= pkStep)
                        {


                            //dc.DrawLine(_pkPen, new Point(coordinate, InclineHeight), new Point(coordinate, ControlHeight));

                            Rect rect = new Rect();
                            rect.Height = 20;
                            rect.Width = pkStep;
                            rect.X = prevCoordinate - rect.Width;
                            rect.Y = InclineHeight;
                            dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect);

                            // пикеты текст
                            text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                                FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                            //if (coordinate - prevCoordinate > text.Width)
                            dc.DrawText(text, new Point(pk >= 10 ? prevCoordinate - rect.Width - 2 : prevCoordinate - rect.Width + 1, PkTextHeight));

                            prevCoordinate = coordinate;
                            pk++;
                        }

                        // последний пикет
                        Rect rect1 = new Rect();
                        rect1.Height = 20;
                        rect1.Width = prevCoordinate - x0;
                        rect1.X = x0;
                        rect1.Y = InclineHeight;
                        dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect1);

                        text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                        //if (x1 - prevCoordinate > text.Width) 
                        dc.DrawText(text, new Point(pk >= 10 ? prevCoordinate - rect1.Width - 2 : prevCoordinate - rect1.Width + 1, PkTextHeight));

                    }

                    // км текст
                    text = new FormattedText(km.Km, CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, new Typeface("Verdana"), 20, Brushes.Green);

                    if (x1 - x0 > text.Width) 
                        dc.DrawText(text, new Point( (x1 + x0)*0.5- text.Width*0.5, KmTextHeight));
                }


            }
        }
        private void DrawInclines()
        {
            using (var dc = _inclineVisual.RenderOpen())
            {
                //if (_segment == null) return;
                //dc.DrawRectangle(null, _inclinePen, new Rect(new Size(_segment.length *_pixelsInMeter, InclineHeight)));
                var prevX = 0d;
                foreach (var incline in inclines)
                {
                    var nextX = incline.End.RouteCoordinate * _pixelsInMeter;
                    var t = Math.Min(Math.Abs(incline.Value), MaxIncline) / MaxIncline;
                    t = 1 - t;
                    t *= t;
                    t *= t;
                    t = 1 - t;
                    if (incline.Value < 0) t = -t;
                    var y0 = (1 + t) * InclineHeight * 0.5f;
                    var y1 = (1 - t) * InclineHeight * 0.5f;
                    dc.DrawLine(_inclinePen, new Point(prevX, y0), new Point(nextX, y1));
                    dc.DrawLine(_inclinePen, new Point(nextX, 0d), new Point(nextX, InclineHeight));
                    var text = new FormattedText(incline.Value.ToString("0.00"), CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, new Typeface("Verdana"), 20, Brushes.Black);
                    if (text.Width < nextX - prevX - 4)
                    {
                        dc.DrawText(text, new Point(prevX + 2, InclineTextHeight));
                    }
                    prevX = nextX;
                }

                if (_editMode == EditMode.Horizontal &&
                    _movingInclineIndex >= 0 && _movingInclineIndex < inclines.Count)
                {
                    var x = inclines[_movingInclineIndex].Start.RouteCoordinate * _pixelsInMeter;
                    var pen = new Pen(Brushes.Green, 3);
                    pen.Freeze();
                    dc.DrawLine(pen, new Point(x, 0d), new Point(x, InclineHeight));
                }
            }
        }
        private void DrawSelectedIncline()
        {
            //if (_editMode == EditMode.Horizontal || _incline == null)
            //{
            //    _inclineRectange.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    var x0 = _incline.pointOnTrack0.coordinate * _pixelsInMeter;
            //    var x1 = _incline.pointOnTrack1.coordinate * _pixelsInMeter;
            //    _inclineRectange.Visibility = Visibility.Visible;
            //    _inclineRectange.Width = x1 - x0;
            //    Canvas.SetLeft(_inclineRectange, x0);
            //}
        }
        private void DrawKilometersStartToEnd(DrawingContext dc, double end)
        {
            //FormattedText text;
            //var prevCoordinate = 0d;
            //var pk = _segment.startPointOnTrack.picket;
            //var pkStep = 100d * _pixelsInMeter;
            //var endPk = end * _pixelsInMeter;
            //for (var pkCoordinate = (100d - _segment.startPointOnTrack.meter) * _pixelsInMeter; pkCoordinate < endPk; pkCoordinate += pkStep, pk++)
            //{
            //    dc.DrawLine(_pkPen, new Point(pkCoordinate, InclineHeight), new Point(pkCoordinate, ControlHeight));
            //    text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
            //        FlowDirection.LeftToRight, Preferences.typeface, 20, Brushes.DimGray);
            //    if (text.Width < pkCoordinate - prevCoordinate)
            //    {
            //        dc.DrawText(text, new Point(prevCoordinate, PkTextHeight));
            //    }
            //    prevCoordinate = pkCoordinate;
            //}
            //var endCoordinate = _segment.SegmentLength * _pixelsInMeter;
            //text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
            //    FlowDirection.LeftToRight, Preferences.typeface, 20, Brushes.Black);
            //if (text.Width < endCoordinate - prevCoordinate)
            //{
            //    dc.DrawText(text, new Point(prevCoordinate, PkTextHeight));
            //}
            //text = new FormattedText(_segment.startPointOnTrack.km.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Verdana"), 20, Brushes.Green);
            //if (text.Width < endCoordinate)
            //{
            //    dc.DrawText(text, new Point(0, KmTextHeight));
            //}
        }

        #region Scaling
        private void InitScales()
        {
            //if (_segment == null) return;
            //_lockHandlers = true;
            //var minPixelInMeter = ActualWidth / _segment.length;
            //if (double.IsNaN(minPixelInMeter) || double.IsInfinity(minPixelInMeter))
            //{
            //    ScaleSlider.Minimum = ScaleSlider.Maximum = 1d;
            //    _pixelsInMeter = 1d;
            //}
            //else
            //{
            //    var maxPixelInMeter = Math.Max(1d, minPixelInMeter);
            //    ScaleSlider.Minimum = minPixelInMeter;
            //    ScaleSlider.Maximum = maxPixelInMeter;
            //    if (!_segment.inclineScaleScrollHasSet)
            //    {
            //        _segment.inclineScale = minPixelInMeter;
            //        _segment.inclineScroll = 0d;
            //        _segment.inclineScaleScrollHasSet = true;
            //    }
            //    _pixelsInMeter = Utilities.Clamp(_segment.inclineScale, minPixelInMeter, maxPixelInMeter);
            //    ScaleSlider.Value = _pixelsInMeter;
            //}
            //_lockHandlers = false;
            //SetScale();
        }
        private void OnSizeChanged(object sender, RoutedEventArgs e)
        {
            //if (mainWindow?.dataBase == null) return;
            InitScales();
        }
        private void SetScale()
        {


            //_metersInPixel = 1d / _pixelsInMeter;
            ////ScaleTextBlock.Text = $"Масштаб: {_pixelsInMeter:0.00}";
            //DrawingCanvas.Width = (_segment?.length ?? 1) * _pixelsInMeter;
            DrawingCanvas.Width = pointOnTracks[pointOnTracks.Count - 1].RouteCoordinate * _pixelsInMeter;
            //if (_segment != null)
            //{
            //    _segment.inclineScale = _pixelsInMeter;
            //    ScrollViewer.ScrollToHorizontalOffset(_segment.inclineScroll);
            //}
            UpdateGraphics();
        }

        private void ScaleSliderValueChanged(object sender, RoutedEventArgs e)
        {
            //if (_lockHandlers || _segment == null) return;
            //var center = ScrollViewer.ContentHorizontalOffset + ScrollViewer.ActualWidth * 0.5;
            //center /= _pixelsInMeter;
            //_pixelsInMeter = ScaleSlider.Value;
            //center *= _pixelsInMeter;
            //_segment.inclineScroll = Math.Max(center - ScrollViewer.ActualWidth * 0.5, 0);
            //SetScale();
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //if (_lockHandlers) return;
            //if (_segment != null) _segment.inclineScroll = ScrollViewer.HorizontalOffset;
        }

        #endregion

        #region Update UI
        private void UpdateUi()
        {
            //SegmentTextBlock.Text = _segment == null ? string.Empty : $"Отрезок {_segment.name}; Уклонов {_segment.inclines.Count}";
            UpdateInclineUi();
            UpdatePointUi();
        }
        
        private void UpdateInclineUi()
        {
            _lockHandlers = true;
            //if (_editMode != EditMode.Horizontal && _incline != null)
            //{
            //    InclineGroupBox.Visibility = Visibility.Visible;
            //    InclineTextBox.Text = _incline.incline.ToString("0.00");
            //    InclineTextBox.Foreground = Brushes.Black;
            //    StartInclineTextBlock.Text =
            //        $"Начало: {_incline.pointOnTrack0.km} км {_incline.pointOnTrack0.picket} пк {_incline.pointOnTrack0.meter:0.00}м";
            //    EndInclineTextBlock.Text =
            //        $"Конец : {_incline.pointOnTrack1.km} км {_incline.pointOnTrack1.picket} пк {_incline.pointOnTrack1.meter:0.00}м";
            //}
            //else
            //{
            //    InclineGroupBox.Visibility = Visibility.Hidden;
            //}

            _lockHandlers = false;
        }
        private void UpdatePointUi()
        {
            //if (_segment == null || _coordinate < 0 || _coordinate > _segment.length)
            //{
            //    PointCoordinateTextBlock.Text = string.Empty;
            //    PointRailWayTextBlock.Text = string.Empty;
            //    PointInclineTextBlock.Text = string.Empty;
            //}
            //else
            //{
            //    var p = new PointOnTrack { segment = _segment, coordinate = _coordinate, kind = PointOnTrackKind.BrakeCheckPlace};
            //    p.ComputeRailwayCoordinates();
            //    PointCoordinateTextBlock.Text = _coordinate.ToString("0.00");
            //    PointRailWayTextBlock.Text = $"{p.km} км {p.picket} пк {p.meter:0.00} м";
            //    PointInclineTextBlock.Text = _segment.GetIncline(_coordinate)?.incline.ToString("0.00") ?? "<Нет уклонов>";
            //}
        }
        #endregion

    }
}

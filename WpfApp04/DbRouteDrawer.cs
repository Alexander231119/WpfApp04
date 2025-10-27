using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfApp04.Controls;


namespace WpfApp04
{
    public class DbRouteDrawer
    {
        // Масштаб и положение элементов (можно сделать свойствами или передавать как параметры)
        public double widtscale { get; set; } = 0.1;
        public double heighscale { get; set; } = 1;
        public double maxSpeed { get; set; } = 300;

        // Позиции элементов на канвасе
        public double segmentsBottom { get; set; } = 0;
        public double segmentsHeight { get; set; } = 20;
        public double kilometersBottom { get; set; } = 20;
        public double kilometersHeight { get; set; } = 30;
        public double pkLineBottom { get; set; } = 50;
        public double pkLineHeight { get; set; } = 10;
        public double inclineControlBottom { get; set; } = 60;
        public double floorBottom { get; set; } = 100;
        public double trafficSignalsBottom { get; set; } = 255;
        public double trackCircuitsBottom { get; set; } = 300;
        public double trackCircuitsHeight { get; set; } = 15;
        public double stationsBottom { get; set; } = 315;
        public double stationsHeight { get; set; } = 65;
        public double speedBottom { get; set; } = 400;

        // Масштабирование уклонов
        public double kscale { get; set; } = 1;
        public double lscale { get; set; } = 1;

        public void DrawRoute(DrawingCanvas _canvas, DbRoute _route1, DbRoute _toAddRoute)
        {
            CalculateRouteElevationScale(_route1.Inclines);
            CanvasWidthFromSegments(_canvas, _route1.Segments);
            DrawSegments(_canvas, _route1.Segments);
            DrawStations(_canvas, _route1.Stations);
            DrawSpeedrestrictions(_canvas, _route1, false);
            DrawSpeedrestrictions(_canvas, _toAddRoute, true);
            DrawKilometers(_canvas, _route1.Kilometers);
            DrawPiketLine(_canvas, _route1.Kilometers, _route1.Segments);
            DrawInclines(_route1.Inclines, _canvas, kscale, lscale);
            DrawInclines(_toAddRoute.Inclines, _canvas, kscale, lscale);
            DrawTrafficLights(_route1.TrafficLights, _canvas);
            DrawTrafficLights(_toAddRoute.TrafficLights, _canvas);
            DrawCrossingPieses(_route1.CrossingPieces, _canvas);
            DrawTrackCircuits(_canvas, _route1);
            DrawUncodedTracks(_canvas, _route1);
            DrawPlatforms(_canvas, _route1.Platforms);
            DrawPlatforms(_canvas, _toAddRoute.Platforms);
            DrawKtsm(_route1.KtsmList, _canvas);
            DrawKtsm(_toAddRoute.KtsmList, _canvas);
            DrawUksps(_route1.UkspsList, _canvas);
            DrawUksps(_toAddRoute.UkspsList, _canvas);
            DrawCrossings(_route1.Crossings, _canvas);
            DrawCrossings(_toAddRoute.Crossings, _canvas);
            DrawTrafficSignals(_route1.TrafficSignals, _canvas);
            DrawTrafficSignals(_toAddRoute.TrafficSignals, _canvas);
            DrawNeutralSections(_route1.NeutralSections, _canvas);
            DrawNeutralSections(_toAddRoute.NeutralSections, _canvas);
            DrawBrakeCheckPlaces(_canvas, _route1.BrakeCheckPlaces);
            DrawCurrentKindChanges(_canvas, _route1.CurrentKindChanges);
            DrawCurrentKindChanges(_canvas, _toAddRoute.CurrentKindChanges);
        }

        public void DrawRouteWay(DrawingCanvas _canvas, DbRoute _route)
        {
            CalculateRouteElevationScale(_route.Inclines);
            DrawSegments(_canvas, _route.Segments);
            CanvasWidthFromSegments(_canvas, _route.Segments);
            DrawStations(_canvas, _route.Stations);
            //DrawInclines(_route.Inclines, _canvas, kscale, lscale);
            DrawInclinesWithVisual(_route.Inclines, _canvas, kscale, lscale);
            DrawTrafficLights(_route.TrafficLights, _canvas);
            DrawKilometers(_canvas, _route.Kilometers);
            DrawPiketLine(_canvas, _route.Kilometers, _route.Segments);
            DrawTunnels(_route.Tunnels, _canvas);
            DrawRailBridges(_route.RailBridges, _canvas);
            DrawCrossingPieses(_route.CrossingPieces, _canvas);
            DrawTrackCircuits(_canvas, _route);
            DrawPlatforms(_canvas, _route.Platforms);
            DrawKtsm(_route.KtsmList, _canvas);
            DrawUksps(_route.UkspsList, _canvas);
            DrawCrossings(_route.Crossings, _canvas);
            DrawTrafficSignals(_route.TrafficSignals, _canvas);
            DrawNeutralSections(_route.NeutralSections, _canvas);
            DrawSpeedrestrictions(_canvas, _route, false);
            DrawBrakeCheckPlaces(_canvas, _route.BrakeCheckPlaces);
            DrawCurrentKindChanges(_canvas, _route.CurrentKindChanges);
        }

        public void CalculateRouteElevationScale(List<Incline> _Inclines)
        {
            if (_Inclines.Count <= 0) return;

            double maxElev = 0;
            double minElev = 0;
            kscale = 1;
            lscale = 1;

            foreach (var incline in _Inclines)
            {
                maxElev = Math.Max(maxElev, incline.End.Elevation);
                minElev = Math.Min(minElev, incline.End.Elevation);
            }

            if ((maxElev - minElev) != 0)
            {
                kscale = 100 / (maxElev - minElev);
            }

            lscale = 50 - kscale * maxElev;
        }

        public void DrawSegments(Canvas _canvas, List<Segment> _segments)
        {
            double segmentcoordinate = 0;
            for (int i = 0; i < _segments.Count; i++)
            {
                var s = _segments[i];

                userControl userControl1 = new userControl();
                userControl1.rectangle.Width = (s.SegmentLength) * widtscale;
                userControl1.rectangle.Height = segmentsHeight;
                userControl1.Value.Text = "";
                userControl1.name = s.SegmentName;
                userControl1.description = s.SegmentLength.ToString()
                    + "\n " + s.Start?.PointOnTrackKm + " " + s.Start?.PointOnTrackPk + " " + s.Start?.PointOnTrackM
                    + "\n " + s.End?.PointOnTrackKm + " " + s.End?.PointOnTrackPk + " " + s.End?.PointOnTrackM;

                if (userControl1.rectangle.Width > (s.SegmentID.ToString() + " - [" + i.ToString() + "]").Length * 10)
                {
                    userControl1.Value.Text = "[" + s.SegmentID.ToString() + "]";
                }

                userControl1.Name.Text = "";

                Canvas.SetLeft(userControl1, segmentcoordinate * widtscale);
                Canvas.SetBottom(userControl1, segmentsBottom);
                segmentcoordinate += s.SegmentLength;
                _canvas.Children.Add(userControl1);
            }
        }

        public void CanvasWidthFromSegments(Canvas _canvas, List<Segment> _segments)
        {
            double segmentcoordinate = 0;
            for (int i = 0; i < _segments.Count; i++)
            {
                var s = _segments[i];
                segmentcoordinate += s.SegmentLength;
            }
            _canvas.Width = segmentcoordinate * widtscale;
        }

        public void DrawStations(Canvas _canvas, List<Station> _stations)
        {
            foreach (Station s in _stations)
            {
                StationControl stationControl1 = new StationControl();

                stationControl1.rectangle.Width = Math.Abs((s.End.RouteCoordinate - s.Start.RouteCoordinate) * widtscale);
                stationControl1.rectangle.Height = stationsHeight;
                stationControl1.StationName.Text = s.StationName;
                stationControl1.StartKM.Text = s.Start.PointOnTrackKm.ToString() + "-" + s.Start.PointOnTrackPk.ToString();
                stationControl1.EndKM.Text = s.End.PointOnTrackKm.ToString() + "-" + s.End.PointOnTrackPk.ToString();
                stationControl1.StartCoord.Text = s.Start.RouteCoordinate.ToString();
                stationControl1.EndCoord.Text = s.End.RouteCoordinate.ToString();

                Canvas.SetLeft(stationControl1, s.Start.RouteCoordinate * widtscale);
                Canvas.SetBottom(stationControl1, stationsBottom);

                if (stationControl1.StationName.Text.Length * 9 > stationControl1.rectangle.Width)
                {
                    stationControl1.StationName.Width = stationControl1.rectangle.Width;
                }

                _canvas.Children.Add(stationControl1);
            }
        }

        public void DrawSpeedrestrictions(Canvas _canvas, DbRoute _route1, bool toadd)
        {
            foreach (SpeedRestriction s in _route1.SpeedRestrictions)
            {
                SpeedRestrictionControl speedrestrictioncontrol = new SpeedRestrictionControl(heighscale, widtscale, s, _route1.Segments, _route1.PointOnTracks, _route1.SpeedRestrictions, _route1.Tracks, _canvas);
                //SpeedRestrictionControl speedrestrictioncontrol = new SpeedRestrictionControl(heighscale, widtscale, s, _speedRestrictions, _canvas);

                speedrestrictioncontrol.rectangle1.Width = Math.Abs(s.End.RouteCoordinate - s.Start.RouteCoordinate) * widtscale;
                speedrestrictioncontrol.rectangle1.Height = Math.Abs(s.Value) * heighscale;
                speedrestrictioncontrol.rectangle2.Width = speedrestrictioncontrol.rectangle1.Width;
                speedrestrictioncontrol.rectangle2.Height = (maxSpeed - s.Value) * heighscale;
                speedrestrictioncontrol.rectangle3.Width = speedrestrictioncontrol.rectangle1.Width;
                speedrestrictioncontrol.rectangle3.Height = speedrestrictioncontrol.rectangle1.Height;

                if (s.PermRestrictionForEmptyTrain == 1)
                {
                    speedrestrictioncontrol.rectangle1.Fill.Opacity = 0;
                    speedrestrictioncontrol.rectangle3.Fill.Opacity = 1;
                    speedrestrictioncontrol.rectangle2.Opacity = 0;
                    speedrestrictioncontrol.rectangle2.Visibility = Visibility.Collapsed;
                }
                else
                {
                    speedrestrictioncontrol.rectangle1.Fill.Opacity = 1;
                    speedrestrictioncontrol.rectangle3.Fill.Opacity = 0;
                    speedrestrictioncontrol.rectangle2.Opacity = 1;
                    speedrestrictioncontrol.rectangle2.Visibility = Visibility.Visible;
                }

                if (speedrestrictioncontrol.rectangle1.Width > 20)
                { speedrestrictioncontrol.Value.Text = s.Value.ToString(); }
                else { speedrestrictioncontrol.Value.Text = ""; }

                Canvas.SetLeft(speedrestrictioncontrol, s.Start.RouteCoordinate * widtscale);
                Canvas.SetBottom(speedrestrictioncontrol, speedBottom);

                speedrestrictioncontrol.rectangle1.Stroke = null;
                speedrestrictioncontrol.rectangle2.Stroke = null;
                speedrestrictioncontrol.rectangle3.Stroke = null;

                _canvas.Children.Add(speedrestrictioncontrol);

                if (toadd)
                {
                    speedrestrictioncontrol.rectangle3.Fill = new SolidColorBrush(Colors.Goldenrod);
                    speedrestrictioncontrol.rectangle1.Fill = new SolidColorBrush(Colors.Aquamarine);
                }
            }
        }
        public void DrawInclinesWithVisual(List<Incline> _inclines, DrawingCanvas _canvas, double _kscale, double _lscale)
        {
            if (_inclines == null || _inclines.Count == 0) return;

            // Создаем кастомный контрол (аналогично PkLineControl)
            var inclineControl = new PkLineControl
            {
                Height = 240, // Общая высота как в InclineControl1 (200 + 40)
                Width = _canvas.Width
            };

            // Устанавливаем позицию относительно низа
            Canvas.SetBottom(inclineControl, inclineControlBottom);
            Canvas.SetLeft(inclineControl, 0);

            // Очищаем внутренний canvas контрола
            inclineControl.PkCanvas.ClearVisuals();

            // Создаем DrawingVisual для рисования
            DrawingVisual visual = new DrawingVisual();
            inclineControl.PkCanvas.AddVisual(visual);

            using (DrawingContext dc = visual.RenderOpen())
            {
                const double PROFILE_HEIGHT = 200;
                const double INDICATOR_HEIGHT = 40;

                Brush profileBackground = new SolidColorBrush(Color.FromArgb(255, 243, 243, 243));
                Brush indicatorBackground = Brushes.White;
                Pen blackPen = new Pen(Brushes.Black, 1.0);
                blackPen.Freeze();

                foreach (Incline incline in _inclines)
                {
                    double width = Math.Abs(incline.End.RouteCoordinate - incline.Start.RouteCoordinate) * widtscale;
                    double startX = incline.Start.RouteCoordinate * widtscale;

                    // 1. Область профиля (верхняя серая часть)
                    dc.DrawRectangle(profileBackground, null,
                        new Rect(startX, 0, width, PROFILE_HEIGHT));

                    // 2. Линия профиля высот
                    double startY = 100 - _kscale * incline.Start.Elevation - _lscale;
                    double endY = 100 - _kscale * incline.End.Elevation - _lscale;
                    dc.DrawLine(blackPen,
                        new Point(startX, startY),
                        new Point(startX + width, endY));

                    // 3. Область индикатора (нижняя белая часть)
                    double indicatorTop = PROFILE_HEIGHT;
                    dc.DrawRectangle(indicatorBackground, blackPen,
                        new Rect(startX, indicatorTop, width, INDICATOR_HEIGHT));

                    // 4. Наклонная линия в индикаторе
                    double lineY = indicatorTop + INDICATOR_HEIGHT / 2;
                    double lineY1 = lineY;
                    double lineY2 = lineY;

                    if (incline.Value > 0) { lineY1 += 5; lineY2 -= 5; } // Подъем
                    else if (incline.Value < 0) { lineY1 -= 5; lineY2 += 5; } // Спуск

                    dc.DrawLine(blackPen,
                        new Point(startX, lineY1),
                        new Point(startX + width, lineY2));

                    // 5. Текст значения уклона
                    if (width > 25)
                    {
                        var text = new FormattedText(
                            incline.Value.ToString("0.0"),
                            CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Verdana"),
                            9,
                            Brushes.Black);

                        dc.DrawText(text,
                            new Point(startX + 5, indicatorTop + 2));
                    }
                }
            }

            // Добавляем контрол на основной canvas
            _canvas.Children.Add(inclineControl);
        }

        public void DrawInclines(List<Incline> _inclines, Canvas _canvas, double _kscale, double _lscale)
        {
            foreach (Incline s in _inclines)
            {
                InclineControl1 inclineControl = new InclineControl1();

                inclineControl.rectangle3.Width = inclineControl.rectangle.Width = inclineControl.rectangle1.Width =
                    Math.Abs(s.End.RouteCoordinate - s.Start.RouteCoordinate) * widtscale;
                inclineControl.InclineValue.Text = s.Value.ToString();

                inclineControl.Line1.X2 = inclineControl.rectangle.Width;
                if (inclineControl.rectangle.Width < 25) { inclineControl.InclineValue.Text = ""; }

                if (s.Value > 0)
                {
                    inclineControl.Line1.Y1 = 15;
                    inclineControl.Line1.Y2 = 5;
                }
                else if (s.Value < 0)
                {
                    inclineControl.Line1.Y1 = 5;
                    inclineControl.Line1.Y2 = 15;
                }
                else if (s.Value == 0)
                {
                    inclineControl.Line1.Y1 = 10;
                    inclineControl.Line1.Y2 = 10;
                }

                inclineControl.Line2.X1 = 0;
                inclineControl.Line2.X2 = inclineControl.rectangle.Width;

                inclineControl.Line2.Y1 = 100 - _kscale * s.Start.Elevation - _lscale;
                inclineControl.Line2.Y2 = 100 - _kscale * s.End.Elevation - _lscale;

                Canvas.SetLeft(inclineControl, s.Start.RouteCoordinate * widtscale);
                Canvas.SetBottom(inclineControl, inclineControlBottom);
                _canvas.Children.Add(inclineControl);
            }
        }

        public void DrawPiketLine(DrawingCanvas _wrapPanel, List<Kilometer> _Kilometers, List<Segment> _segments)
        {
            PkLineControl pkLineControl = new PkLineControl();
            pkLineControl.PkCanvas.Width = _wrapPanel.ActualWidth;
            pkLineControl.PkCanvas.Height = pkLineHeight;

            Canvas.SetBottom(pkLineControl, pkLineBottom);
            Canvas.SetLeft(pkLineControl, 0);

            double InclineHeight = 0;
            Pen _pkPen = new Pen(Brushes.DimGray, 1d);

            DrawingVisual _kmVisual = new DrawingVisual();

            pkLineControl.PkCanvas.AddVisual(_kmVisual);

            using var dc = _kmVisual.RenderOpen();

            var pkStep = 100d * widtscale;

            foreach (Kilometer k in _Kilometers)
            {
                double textMinus = 4;
                int i = _Kilometers.IndexOf(k);

                var x0 = _Kilometers[i].Start.RouteCoordinate * widtscale;
                var x1 = _Kilometers[i].End.RouteCoordinate * widtscale;

                int startSegmentID = _segments.FindLastIndex(x => (x.SegmentID == k.Start.SegmentID));

                FormattedText text;

                var prevCoordinate = x0;

                if (_segments[startSegmentID].PredefinedRouteSegmentFromStartToEnd == 1)
                {
                    double pk = k.Start.PointOnTrackPk;

                    // первый пикет
                    Rect rect2 = new Rect();
                    rect2.Height = pkLineHeight;
                    rect2.Width = Math.Abs(100d - k.Start.PointOnTrackM) * widtscale;
                    rect2.X = prevCoordinate;
                    rect2.Y = InclineHeight;
                    dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect2);

                    text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                                FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                    if (rect2.Width > text.Width - textMinus)
                        dc.DrawText(text, new System.Windows.Point(pk >= 10 ? prevCoordinate - 2 : prevCoordinate + 1, InclineHeight));

                    prevCoordinate += (100d - k.Start.PointOnTrackM) * widtscale;
                    pk++;

                    //пикеты
                    for (var coordinate = prevCoordinate + pkStep; coordinate < x1; coordinate += pkStep)
                    {
                        Rect rect = new Rect();
                        rect.Height = pkLineHeight;
                        rect.Width = pkStep;
                        rect.X = prevCoordinate;
                        rect.Y = InclineHeight;
                        dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect);

                        text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                        if (rect.Width > text.Width - textMinus)
                            dc.DrawText(text, new System.Windows.Point(pk >= 10 ? prevCoordinate - 2 : prevCoordinate + 1, InclineHeight));

                        prevCoordinate = coordinate;
                        pk++;
                    }

                    // последний пикет
                    Rect rect1 = new Rect();
                    rect1.Height = pkLineHeight;
                    rect1.Width = x1;
                    rect1.X = prevCoordinate;
                    rect1.Y = InclineHeight;
                    dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect1);

                    text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                    if (rect1.Width > text.Width - textMinus)
                        dc.DrawText(text, new System.Windows.Point(pk >= 10 ? prevCoordinate - 2 : prevCoordinate + 1, InclineHeight));
                }
                else // убывающий километраж
                {
                    double pk = k.End.PointOnTrackPk;

                    prevCoordinate = x1;

                    // первый пикет
                    Rect rect2 = new Rect();
                    rect2.Height = pkLineHeight;
                    rect2.Width = Math.Abs(100d - k.End.PointOnTrackM) * widtscale;
                    rect2.X = prevCoordinate - rect2.Width;
                    rect2.Y = InclineHeight;
                    dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect2);

                    text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                    if (rect2.Width > text.Width - textMinus)
                        dc.DrawText(text, new System.Windows.Point(pk >= 10 ? prevCoordinate - rect2.Width - 2 : prevCoordinate - rect2.Width + 1, InclineHeight));

                    prevCoordinate -= (100d - k.End.PointOnTrackM) * widtscale;
                    pk++;

                    //пикеты
                    for (var coordinate = prevCoordinate - pkStep; coordinate > x0; coordinate -= pkStep)
                    {
                        Rect rect = new Rect();
                        rect.Height = pkLineHeight;
                        rect.Width = pkStep;
                        rect.X = prevCoordinate - rect.Width;
                        rect.Y = InclineHeight;
                        dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect);

                        text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                        if (rect.Width > text.Width - textMinus)
                            dc.DrawText(text, new System.Windows.Point(pk >= 10 ? prevCoordinate - rect.Width - 2 : prevCoordinate - rect.Width + 1, InclineHeight));

                        prevCoordinate = coordinate;
                        pk++;
                    }

                    // последний пикет
                    Rect rect1 = new Rect();
                    rect1.Height = pkLineHeight;
                    rect1.Width = Math.Abs(prevCoordinate - x0);
                    rect1.X = x0;
                    rect1.Y = InclineHeight;
                    dc.DrawRectangle(pk % 2 != 0 ? Brushes.DimGray : Brushes.White, _pkPen, rect1);

                    text = new FormattedText(pk.ToString(), CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, new Typeface("Verdana"), 9, pk % 2 == 0 ? Brushes.DimGray : Brushes.White);
                    if (rect1.Width > text.Width - textMinus)
                        dc.DrawText(text, new System.Windows.Point(pk >= 10 ? prevCoordinate - rect1.Width - 2 : prevCoordinate - rect1.Width + 1, InclineHeight));
                }
            }
            _wrapPanel.Children.Add(pkLineControl);
        }

        public void DrawKilometers(DrawingCanvas _wrapPanel, List<Kilometer> _Kilometers)
        {
            foreach (Kilometer k in _Kilometers)
            {
                KmControl kmControl = new KmControl(k);
                kmControl.rectangle.Width = Math.Abs(k.End.RouteCoordinate - k.Start.RouteCoordinate) * widtscale;
                kmControl.Height = kilometersHeight;
                if (kmControl.rectangle.Width > 25)
                { kmControl.KmName.Text = k.Km; }
                else
                { kmControl.KmName.Text = ""; }

                Canvas.SetBottom(kmControl, kilometersBottom);
                Canvas.SetLeft(kmControl, k.Start.RouteCoordinate * widtscale);
                _wrapPanel.Children.Add(kmControl);
            }
        }

        public void DrawTrafficLights(List<TrafficLight> tlist, Canvas canvas)
        {
            foreach (TrafficLight k in tlist)
            {
                TrafficLightControl tControl = new TrafficLightControl(k);

                tControl.TrafficLightName.Text = k.TrafficLightName;
                tControl.rectangle1.Width = k.TrafficLightName.Length * 8;

                if (k.DicTrafficLightKindID == 5 || k.DicTrafficLightKindID == 6)
                {
                    tControl.trafficLighrBorder.Visibility = Visibility.Hidden;
                    tControl.ellipse1.Visibility = Visibility.Hidden;
                }
                else
                {
                    tControl.zagradRectangle.Visibility = Visibility.Hidden;
                }

                Canvas.SetBottom(tControl, stationsBottom);
                Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 19));
                canvas.Children.Add(tControl);
            }
        }

        public void DrawUksps(List<Uksps> tlist, Canvas canvas)
        {
            foreach (Uksps k in tlist)
            {
                UkspsControl tControl = new UkspsControl(k);
                Canvas.SetBottom(tControl, floorBottom);
                Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                canvas.Children.Add(tControl);
            }
        }

        public void DrawKtsm(List<Ktsm> tlist, Canvas canvas)
        {
            foreach (Ktsm k in tlist)
            {
                userControl tControl = new userControl();
                tControl.Value.Text = "ктсм";
                tControl.rectangle.Stroke = System.Windows.Media.Brushes.DarkGray;
                tControl.description = "КТСМ \n" + k.StartPointOnTrackKm + "-" + k.StartPointOnTrackPk + "-" + k.StartPointOnTrackM;
                tControl.Name.Text = "";
                tControl.rectangle.Height = 17;
                tControl.rectangle.Width = 26;
                tControl.rectangle.Fill = System.Windows.Media.Brushes.LightGray;

                Canvas.SetBottom(tControl, floorBottom);
                Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                canvas.Children.Add(tControl);
            }
        }

        public void DrawCurrentKindChanges(Canvas canvas, List<CurrentKindChange> tlist)
        {
            foreach (CurrentKindChange k in tlist)
            {
                userControl tControl = new userControl();
                tControl.Value.Text = "=/~";
                tControl.rectangle.Stroke = System.Windows.Media.Brushes.DarkGray;
                tControl.description = "смена рода тока \n" + k.CurrentKindNameLeft + "-" + k.CurrentKindNameRight + "\n"
                      + k.StartPointOnTrackKm + " - " + k.StartPointOnTrackPk + " - " + k.StartPointOnTrackM + "\n"
                      + k.TrackObjectID ;

                tControl.Name.Text = "";
                tControl.rectangle.Height = 17;
                tControl.rectangle.Width = 26;
                tControl.rectangle.Fill = System.Windows.Media.Brushes.LightGray;

                Canvas.SetBottom(tControl, floorBottom);
                Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                canvas.Children.Add(tControl);
            }
        }

        public void DrawTrafficSignals(List<TrafficSignal> tlist, Canvas canvas)
        {
            foreach (TrafficSignal k in tlist)
            {
                if (k.DicTrafficSignalKindID == 16)
                {
                    // знак с
                    Scontrol1 tControl = new Scontrol1();
                    tControl.t = k;
                    Canvas.SetBottom(tControl, trafficSignalsBottom);
                    Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                    canvas.Children.Add(tControl);
                }

                if (k.DicTrafficSignalKindID == 1)
                {
                    userControl tControl = new userControl();
                    tControl.Value.Text = "!газ";

                    tControl.rectangle.Stroke = System.Windows.Media.Brushes.DarkGray;
                    tControl.description = "газ\n"
                                           + k.StartPointOnTrackKm + " - " + k.StartPointOnTrackPk + " - " + k.StartPointOnTrackM + "\n"
                                           + k.TrackObjectID;

                    tControl.Name.Text = "";
                    tControl.rectangle.Height = 17;
                    tControl.rectangle.Width = 26;
                    tControl.rectangle.Fill = System.Windows.Media.Brushes.White;

                    Canvas.SetBottom(tControl, trafficSignalsBottom-17);
                    Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                    canvas.Children.Add(tControl);
                }
                if (k.DicTrafficSignalKindID == 2)
                {
                    userControl tControl = new userControl();
                    tControl.Value.Text = "нефть";

                    tControl.rectangle.Stroke = System.Windows.Media.Brushes.DarkGray;
                    tControl.description = "нефть\n"
                                           + k.StartPointOnTrackKm + " - " + k.StartPointOnTrackPk + " - " + k.StartPointOnTrackM + "\n"
                                           + k.TrackObjectID;

                    tControl.Name.Text = "";
                    tControl.rectangle.Height = 17;
                    tControl.rectangle.Width = 34;
                    tControl.rectangle.Fill = System.Windows.Media.Brushes.White;

                    Canvas.SetBottom(tControl, trafficSignalsBottom-17);
                    Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                    canvas.Children.Add(tControl);
                }
                if (k.DicTrafficSignalKindID == 28)
                {
                    userControl tControl = new userControl();
                    tControl.Value.Text = "";

                    tControl.rectangle.Stroke = System.Windows.Media.Brushes.DarkGray;
                    tControl.description = "жёлтый щит\n"
                                           + k.StartPointOnTrackKm + " - " + k.StartPointOnTrackPk + " - " + k.StartPointOnTrackM + "\n"
                                           + k.TrackObjectID;

                    tControl.Name.Text = "";
                    tControl.rectangle.Height = 16;
                    tControl.rectangle.Width = 16;
                    tControl.rectangle.Fill = System.Windows.Media.Brushes.Yellow;

                    Canvas.SetBottom(tControl, trafficSignalsBottom - 34);
                    Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                    canvas.Children.Add(tControl);
                }
                if (k.DicTrafficSignalKindID == 29)
                {
                    userControl tControl = new userControl();
                    tControl.Value.Text = "";

                    tControl.rectangle.Stroke = System.Windows.Media.Brushes.DarkGray;
                    tControl.description = "зелёный щит\n"
                                           + k.StartPointOnTrackKm + " - " + k.StartPointOnTrackPk + " - " + k.StartPointOnTrackM + "\n"
                                           + k.TrackObjectID;

                    tControl.Name.Text = "";
                    tControl.rectangle.Height = 16;
                    tControl.rectangle.Width = 16;
                    tControl.rectangle.Fill = System.Windows.Media.Brushes.Green;

                    Canvas.SetBottom(tControl, trafficSignalsBottom - 34);
                    Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                    canvas.Children.Add(tControl);
                }
                if (k.DicTrafficSignalKindID == 14 || k.DicTrafficSignalKindID == 15)
                {
                    // опасное место знак
                    RoundSignControl tControl = new RoundSignControl();
                    tControl.controlGrid.Width = 13;
                    tControl.controlGrid.Height = 13;


                    tControl.description = $"{(k.DicTrafficSignalKindID == 15 ? "конец" : "начало")} опасного места\n"
                                           + k.StartPointOnTrackKm + " - " + k.StartPointOnTrackPk + " - " + k.StartPointOnTrackM + "\n"
                                           + k.TrackObjectID;

                    if (k.DicTrafficSignalKindID == 14)
                    {
                        tControl.rectangle1.Visibility= Visibility.Visible;
                        tControl.rectangle2.Visibility = Visibility.Collapsed;
                    }
                    else if (k.DicTrafficSignalKindID == 15)
                    {
                        tControl.rectangle2.Visibility = Visibility.Visible;
                        tControl.rectangle1.Visibility = Visibility.Collapsed;
                    }


                    Canvas.SetBottom(tControl, trafficSignalsBottom - 34);
                    Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                    canvas.Children.Add(tControl);
                }
                if (k.DicTrafficSignalKindID == 30 || k.DicTrafficSignalKindID == 31)
                {

                    // Жёлтый диск, зелёный диск
                    RoundSignControl tControl = new RoundSignControl();
                    tControl.controlGrid.Width = 13;
                    tControl.controlGrid.Height = 13;


                    tControl.description = $"{(k.DicTrafficSignalKindID == 30 ? "жёлтый" : "зелёный")} диск\n"
                                           + k.StartPointOnTrackKm + " - " + k.StartPointOnTrackPk + " - " + k.StartPointOnTrackM + "\n"
                                           + k.TrackObjectID;

                    tControl.rectangle1.Visibility = Visibility.Collapsed;
                    tControl.rectangle2.Visibility = Visibility.Collapsed;

                    tControl.ellipse2.Margin= new Thickness(1, 1, 1, 1);
                    tControl.ellipse1.Fill = System.Windows.Media.Brushes.DarkGray;

                    if (k.DicTrafficSignalKindID == 30)
                    {
                        tControl.ellipse2.Fill= System.Windows.Media.Brushes.Yellow;
                    }
                    else if (k.DicTrafficSignalKindID == 31)
                    {
                        tControl.ellipse2.Fill = System.Windows.Media.Brushes.Green;
                    }


                    Canvas.SetBottom(tControl, trafficSignalsBottom - 34);
                    Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                    canvas.Children.Add(tControl);
                }

            }
        }

        public void DrawCrossings(List<Crossing> tlist, Canvas canvas)
        {
            foreach (Crossing k in tlist)
            {
                CrossingControl tControl = new CrossingControl(k);
                if (k.DicCrossingKindID == 2)
                {
                    tControl.poly1.Visibility = Visibility.Visible;
                }
                Canvas.SetBottom(tControl, trafficSignalsBottom);
                Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                canvas.Children.Add(tControl);
            }
        }

        public void DrawNeutralSections(List<NeutralSection> tlist, Canvas canvas)
        {
            foreach (NeutralSection k in tlist)
            {
                NeutralSectionControl tControl = new NeutralSectionControl(k);
                Canvas.SetBottom(tControl, trafficSignalsBottom + 20);
                Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 1));
                canvas.Children.Add(tControl);
            }
        }

        public void DrawRailBridges(List<RailBridge> tlist, Canvas canvas)
        {
            foreach (RailBridge k in tlist)
            {
                BridgeControl tControl = new BridgeControl(k);
                Canvas.SetBottom(tControl, floorBottom);
                Canvas.SetBottom(tControl, 290 - 100 + 10 + kscale * k.Start.Elevation + lscale);
                Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale));
                canvas.Children.Add(tControl);
            }
        }

        public void DrawTunnels(List<Tunnel> tlist, Canvas canvas)
        {
            foreach (Tunnel k in tlist)
            {
                TunnelControl tControl = new TunnelControl(k);
                Canvas.SetBottom(tControl, 290 - 100 + 10 + kscale * k.Start.Elevation + lscale);
                Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale));
                canvas.Children.Add(tControl);
            }
        }

        public void DrawCrossingPieses(List<CrossingPiece> tlist, Canvas canvas)
        {
            foreach (CrossingPiece k in tlist)
            {
                CrossingPieceControl tControl = new CrossingPieceControl(k);
                tControl.CrossingPieceControlTextBlock.Text = k.CrossingPieceName;
                Canvas.SetBottom(tControl, floorBottom);
                Canvas.SetLeft(tControl, (k.Start.RouteCoordinate * widtscale - 13));
                canvas.Children.Add(tControl);
            }
        }

        public void DrawUncodedTracks(Canvas _canvas, DbRoute _route)
        {
            foreach (UncodedTrack s in _route.UncodedTracks)
            {
                if (s.Start != null && s.End != null)
                {
                    userControl userControl1 = new userControl();
                    userControl1.rectangle.Width = Math.Abs(s.End.RouteCoordinate - s.Start.RouteCoordinate) * widtscale;
                    userControl1.rectangle.Height = 12;
                    userControl1.rectangle.Fill = System.Windows.Media.Brushes.White;
                    userControl1.Value.Text = "";
                    userControl1.Name.Text = "";
                    Canvas.SetLeft(userControl1, s.Start.RouteCoordinate * widtscale);
                    Canvas.SetBottom(userControl1, stationsBottom - 3);

                    _canvas.Children.Add(userControl1);
                }
            }
        }

        public void DrawBrakeCheckPlaces(Canvas _canvas, List<BrakeCheckPlace> _brakeCheckPlaces)
        {
            foreach (BrakeCheckPlace s in _brakeCheckPlaces)
            {
                if (s.Start != null && s.End != null)
                {
                    userControl userControl1 = new userControl();
                    userControl1.rectangle.Width = 60;
                    userControl1.rectangle.Height = 20;

                    if (s.DicBrakeCheckKindID == 1) userControl1.rectangle.Width = 70;
                    userControl1.rectangle.Fill = System.Windows.Media.Brushes.White;
                    userControl1.Value.Text = s.DicBrakeCheckKindName + " " + s.StartPointOnTrackKm + "-" + s.StartPointOnTrackPk;
                    userControl1.description = s.DicBrakeCheckKindName + " " + s.StartPointOnTrackKm + "-" + s.StartPointOnTrackPk;

                    foreach (var v in s.BrakeCheckNormList)
                    {
                        userControl1.description += "\n  " + v.BrakeCheckNormSpeed + " " + v.BrakeCheckNormPath;
                    }

                    userControl1.Name.Text = "";
                    Canvas.SetLeft(userControl1, s.Start.RouteCoordinate * widtscale);
                    Canvas.SetBottom(userControl1, trafficSignalsBottom - 20);

                    _canvas.Children.Add(userControl1);
                }
            }
        }

        public void DrawPlatforms(Canvas _canvas, List<Platform> _platforms)
        {
            foreach (Platform s in _platforms)
            {
                if (s.Start != null && s.End != null)
                {
                    userControl userControl1 = new userControl();
                    userControl1.rectangle.Width = Math.Abs(s.End.RouteCoordinate - s.Start.RouteCoordinate) * widtscale;
                    userControl1.rectangle.Height = 15;
                    userControl1.rectangle.Fill = System.Windows.Media.Brushes.LightGray;
                    userControl1.rectangle.Stroke = System.Windows.Media.Brushes.Gray;
                    userControl1.Value.Text = "";
                    userControl1.description = "пл " + s.PlatformName + "\n " + s.StartPointOnTrackKm + "-" + s.StartPointOnTrackPk + "-" + s.StartPointOnTrackM
                       + "\n " + s.EndPointOnTrackKm + "-" + s.EndPointOnTrackPk + "-" + s.EndPointOnTrackM;
                    userControl1.Name.Text = "";
                    Canvas.SetLeft(userControl1, s.Start.RouteCoordinate * widtscale);
                    Canvas.SetBottom(userControl1, stationsBottom);

                    _canvas.Children.Add(userControl1);
                }
            }
        }

        public void DrawTrackCircuits(Canvas _canvas, DbRoute _route)
        {
            foreach (TrackCircuit s in _route.TrackCircuits)
            {
                if (s.Start != null && s.End != null)
                {
                    userControl userControl1 = new userControl();
                    userControl1.rectangle.Width = Math.Abs(s.End.RouteCoordinate - s.Start.RouteCoordinate) * widtscale;
                    userControl1.rectangle.Height = trackCircuitsHeight;
                    userControl1.rectangle.Fill = System.Windows.Media.Brushes.YellowGreen;

                    if (_route.TrackCircuits.Count == 1)
                    {
                        userControl1.rectangle.Fill = System.Windows.Media.Brushes.LightGray;
                    }

                    userControl1.Value.Text = "";
                    if ((userControl1.rectangle.Width > 25) && (userControl1.rectangle.Width > s.TrafficLightName.Length * 9))
                    {
                        if (s.TrafficLightName != "")
                        { userControl1.Value.Text = s.TrafficLightName; }
                    }

                    userControl1.Name.Text = "";
                    Canvas.SetLeft(userControl1, s.Start.RouteCoordinate * widtscale);
                    Canvas.SetBottom(userControl1, trackCircuitsBottom);

                    _canvas.Children.Add(userControl1);
                }
            }
        }

    }
}

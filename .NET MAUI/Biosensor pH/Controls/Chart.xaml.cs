using System.Collections.Generic;
using System.Diagnostics;

namespace Biosensor_pH.Controls;

public class DataPoint
{
    public readonly double Value;
    public readonly DateTime Time;

    public DataPoint(double value, DateTime time)
    {
        Value = value;
        Time = time;
    }
}

public partial class Chart : ContentView, IDrawable
{
    #region Bindable Property

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(Chart), string.Empty,
        propertyChanged: OnTitleChanged);

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        Chart chart = (Chart)bindable;
        chart.ChartTitle.Text = (string)newValue;
    }

    public string Title
    {
        get => (string)GetValue(Chart.TitleProperty);
        set => SetValue(Chart.TitleProperty, value);
    }

    public static readonly BindableProperty DataProperty = BindableProperty.Create(nameof(Data), typeof(IEnumerable<DataPoint>), typeof(Chart), null,
        propertyChanged: OnDataChanged);

    private static void OnDataChanged(BindableObject bindable, object oldValue, object newValue)
    {
        //Chart chart = (Chart)bindable;
        //chart._data = (Queue<DataPoint>) newValue;

        //while (chart._data.First().Time < chart._dateTimeStart)
        //{
        //    chart._data.Dequeue();
        //}
    }

    public IEnumerable<DataPoint> Data
    {
        get => (IEnumerable<DataPoint>)GetValue(Chart.DataProperty);
        set => SetValue(Chart.DataProperty, value);
    }

    public static readonly BindableProperty UnitProperty = BindableProperty.Create(nameof(Unit), typeof(string), typeof(Chart), string.Empty);

    public string Unit
    {
        get => (string)GetValue(Chart.UnitProperty);
        set => SetValue(Chart.UnitProperty, value);
    }

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(nameof(Minimum), typeof(double), typeof(Chart), 0.0);

    public double Minimum
    {
        get => (double)GetValue(Chart.MinimumProperty);
        set => SetValue(Chart.MinimumProperty, value);
    }

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Chart), 100.0);

    public double Maximum
    {
        get => (double)GetValue(Chart.MaximumProperty);
        set => SetValue(Chart.MaximumProperty, value);
    }

    public static readonly BindableProperty MainScaleProperty = BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Chart), 10.0);

    public double MainScale
    {
        get => (double)GetValue(Chart.MainScaleProperty);
        set => SetValue(Chart.MainScaleProperty, value);
    }

    public static readonly BindableProperty SecondaryScaleProperty = BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Chart), 5.0);

    public double SecondaryScale
    {
        get => (double)GetValue(Chart.SecondaryScaleProperty);
        set => SetValue(Chart.SecondaryScaleProperty, value);
    }

    public static readonly BindableProperty TimeIntervalProperty = BindableProperty.Create(nameof(TimeInterval), typeof(TimeSpan), typeof(Chart), TimeSpan.FromMinutes(1));

    public TimeSpan TimeInterval
    {
        get => (TimeSpan)GetValue(Chart.TimeIntervalProperty);
        set => SetValue(Chart.TimeIntervalProperty, value);
    }

    public static readonly BindableProperty MainTimeScaleProperty = BindableProperty.Create(nameof(MainTimeScale), typeof(TimeSpan), typeof(Chart), TimeSpan.FromSeconds(10.0));

    public TimeSpan MainTimeScale
    {
        get => (TimeSpan)GetValue(Chart.MainTimeScaleProperty);
        set => SetValue(Chart.MainTimeScaleProperty, value);
    }

    public static readonly BindableProperty SecondaryTimeScaleProperty = BindableProperty.Create(nameof(SecondaryTimeScale), typeof(TimeSpan), typeof(Chart), TimeSpan.FromSeconds(1.0));

    public TimeSpan SecondaryTimeScale
    {
        get => (TimeSpan)GetValue(Chart.SecondaryTimeScaleProperty);
        set => SetValue(Chart.SecondaryTimeScaleProperty, value);
    }

    public static readonly BindableProperty TimeFormatProperty = BindableProperty.Create(nameof(TimeFormat), typeof(string), typeof(Chart), "HH:mm:ss");

    public string TimeFormat
    {
        get => (string)GetValue(Chart.TimeFormatProperty);
        set => SetValue(Chart.TimeFormatProperty, value);
    }

    public static readonly BindableProperty LineColorProperty = BindableProperty.Create(nameof(LineColor), typeof(Color), typeof(Chart), Colors.Gray);

    public Color LineColor
    {
        get => (Color)GetValue(Chart.LineColorProperty);
        set => SetValue(Chart.LineColorProperty, value);
    }

    public static readonly BindableProperty MainLineColorProperty = BindableProperty.Create(nameof(MainLineColor), typeof(Color), typeof(Chart), Colors.Gray);

    public Color MainLineColor
    {
        get => (Color)GetValue(Chart.MainLineColorProperty);
        set => SetValue(Chart.MainLineColorProperty, value);
    }

    public static readonly BindableProperty SecondaryLineColorProperty = BindableProperty.Create(nameof(SecondaryLineColor), typeof(Color), typeof(Chart), Colors.Gray);

    public Color SecondaryLineColor
    {
        get => (Color)GetValue(Chart.SecondaryLineColorProperty);
        set => SetValue(Chart.SecondaryLineColorProperty, value);
    }

    #endregion

    private System.Timers.Timer _invalidateTimer;

    public Chart()
    {
        InitializeComponent();
        GraphicsView.Drawable = this;

        _invalidateTimer = new System.Timers.Timer(100);
        _invalidateTimer.Elapsed += _invalidateTimer_Elapsed;
        _invalidateTimer.AutoReset = true;
        _invalidateTimer.Start();
    }

    private void _invalidateTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        GraphicsView.Invalidate();
    }

    #region IDrawable

    private ICanvas? _canvas;
    private RectF _dirtyRect;
    private RectF _marginRect;

    private DateTime _dateTimeNow;
    private DateTime _dateTimeStart;
    private Queue<DataPoint> _data;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        try
        {
            _canvas = canvas;
            _dirtyRect = dirtyRect;

            if (DeviceInfo.Platform == DevicePlatform.WinUI)
                _marginRect = MarginRect(dirtyRect, 50, 5, 5, 20);
            else
                _marginRect = MarginRect(dirtyRect, 50, 5, 5, 5);

            //DrawDebugRects();

            //Debug.WriteLine($"Data has elements");

            DrawGrid();
            DrawChart();
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private void DrawDebugRects()
    {
        _canvas.FillColor = Color.FromArgb("7FFF0000");
        _canvas.FillRectangle(_dirtyRect);
        _canvas.FillColor = Color.FromArgb("7F00FF00");
        _canvas.FillRectangle(_marginRect);
    }

    public static RectF MarginRect(RectF dirtyRect, float margin)
    {
        return new RectF(margin, margin, dirtyRect.Width - 2 * margin, dirtyRect.Height - 2 * margin);
    }

    public static RectF MarginRect(RectF dirtyRect, float marginLeft, float marginTop, float marginRight, float marginBottom)
    {
        return new RectF(marginLeft, marginTop, dirtyRect.Width - marginLeft - marginRight, dirtyRect.Height - marginTop - marginBottom);
    }

    public float CalcX(DateTime time)
    {
        return (float)(_marginRect.Left + (time - _dateTimeStart) * _marginRect.Width / TimeInterval);
    }

    public float CalcY(double value)
    {
        return (float) (_marginRect.Top + _marginRect.Height - (value - Minimum) * _marginRect.Height / (Maximum - Minimum));
    }

    public void DrawGrid()
    {
        if (_canvas == null)
            return;

        _dateTimeNow = DateTime.Now;
        _dateTimeStart = _dateTimeNow - TimeInterval;

        TimeSpan absoluteTimeSpan = _dateTimeStart - DateTime.MinValue;

        // Linie pomocnicze poziome
        _canvas.StrokeColor = SecondaryLineColor;
        _canvas.StrokeSize = 1.0F;
        double value = Math.Ceiling(Minimum / SecondaryScale) * SecondaryScale;

        while (value < Maximum)
        {
            if (value % MainScale != 0)
                DrawHorizontalLine(value);

            value += SecondaryScale;
        }

        // Linie pomocnicze pionowe
        DateTime time = DateTime.MinValue + Math.Ceiling(absoluteTimeSpan / SecondaryTimeScale) * SecondaryTimeScale;

        while (time < _dateTimeNow)
        {
            if ((time - DateTime.MinValue).Ticks % MainTimeScale.Ticks != 0)
                DrawVerticalLine(time);

            time += SecondaryTimeScale;
        }

        // Linie g³ówne poziome
        _canvas.StrokeColor = MainLineColor;
        _canvas.StrokeSize = 3.0F;
        value = Math.Ceiling(Minimum / MainScale) * MainScale;

        while (value < Maximum)
        {
            DrawHorizontalLine(value, $"{value} {Unit}");
            value += MainScale;
        }

        DrawHorizontalLine(Minimum, $"{Minimum} {Unit}");
        DrawHorizontalLine(Maximum, $"{Maximum} {Unit}");

        // Linie g³ówne pionowe
        time = DateTime.MinValue + Math.Ceiling(absoluteTimeSpan / MainTimeScale) * MainTimeScale;

        while (time <= _dateTimeNow)
        {
            DrawVerticalLine(time, time.ToString(TimeFormat));
            time += MainTimeScale;
        }

        DrawVerticalLine(_dateTimeStart);
        DrawVerticalLine(_dateTimeNow);
    }

    public void DrawHorizontalLine(double value)
    {
        if (_canvas == null)
            return;

        float y = CalcY(value);
        _canvas.DrawLine(_marginRect.Left, y, _marginRect.Right, y);
    }

    public void DrawHorizontalLine(double value, string text)
    {
        if (_canvas == null)
            return;

        float y = CalcY(value);
        _canvas.DrawLine(_marginRect.Left, y, _marginRect.Right, y);

        RectF textRect = new RectF();

        textRect.X = 0;
        textRect.Width = _marginRect.X - 5;
        textRect.Y = y - 10;
        textRect.Height = 20;

        _canvas.FontColor = Colors.White;
        _canvas.DrawString(text.ToString(), textRect, HorizontalAlignment.Right, VerticalAlignment.Center);
    }

    public void DrawVerticalLine(DateTime time)
    {
        if (_canvas == null)
            return;

        float x = CalcX(time);
        _canvas.DrawLine(x, _marginRect.Top, x, _marginRect.Bottom);
    }

    public void DrawVerticalLine(DateTime time, string text)
    {
        if (_canvas == null)
            return;

        float x = CalcX(time);
        _canvas.DrawLine(x, _marginRect.Top, x, _marginRect.Bottom);

        _canvas.FontColor = Colors.White;
        _canvas.DrawString(text.ToString(), x, _marginRect.Bottom + 15 + 3, HorizontalAlignment.Center);
    }

    public void DrawChart()
    {
        if (_canvas == null)
            return;

        if (Data == null)
            return;

        //if (_data == null)
        //    return;

        PathF path = new PathF();

        Queue<DataPoint> localData = new Queue<DataPoint>(Data);

        foreach (DataPoint dataPoint in localData)
        {
            if (dataPoint.Time < _dateTimeStart)
                continue;

            float x = CalcX(dataPoint.Time);
            float y = CalcY(dataPoint.Value);

            if (dataPoint == Data.First())
                //if (chartData.f[i] != -1)
                path.MoveTo(x, y);
            else
                //if (Data[i].Y != -1.0)
                path.LineTo(x, y);
        }

        _canvas.StrokeSize = 5.0F;
        _canvas.StrokeColor = LineColor;
        _canvas.DrawPath(path);
    }
    #endregion
}


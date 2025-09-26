namespace Biosensor_pH.Controls;

public partial class Chart : ContentView, IDrawable
{
    #region Bindable Property

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(Chart), string.Empty);

    public string Title
    {
        get => (string)GetValue(Chart.TitleProperty);
        set => SetValue(Chart.TitleProperty, value);
    }

    public static readonly BindableProperty DataProperty = BindableProperty.Create(nameof(Data), typeof(List<Point>), typeof(Chart), null);

    public List<Point> Data
    {
        get => (List<Point>)GetValue(Chart.LineColorProperty);
        set => SetValue(Chart.LineColorProperty, value);
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

    public static readonly BindableProperty LineColorProperty = BindableProperty.Create(nameof(LineColor), typeof(Color), typeof(Chart), Colors.Gray);

    public Color LineColor
    {
        get => (Color)GetValue(Chart.LineColorProperty);
        set => SetValue(Chart.LineColorProperty, value);
    }

    #endregion

    private System.Timers.Timer _invalidateTimer;

    public Chart()
    {
        InitializeComponent();
        GraphicsView.Drawable = this;

        _invalidateTimer = new System.Timers.Timer(1000);
        _invalidateTimer.Elapsed += _invalidateTimer_Elapsed;
        _invalidateTimer.AutoReset = true;
        //_invalidateTimer.Start();
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

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        _canvas = canvas;
        _dirtyRect = dirtyRect;

        if(DeviceInfo.Platform == DevicePlatform.WinUI)
            _marginRect = MarginRect(dirtyRect, 50, 5, 5, 20);
        else
            _marginRect = MarginRect(dirtyRect, 50, 5, 5, 5);

        //DrawDebugRects();

        DrawHorizontalLines();
        //DrawVerticalLines();

        canvas.StrokeColor = LineColor;
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

    public void DrawHorizontalLines()
    {
        double value = Math.Ceiling(Minimum / SecondaryScale) * SecondaryScale;

        while (value < Maximum)
        {
            // Nie rysuj tam, gdzie podzia³ka g³ówna
            if (value % MainScale != 0)
                DrawHorizontalLine(value);

            value += SecondaryScale;
        }

        value = Math.Ceiling(Minimum / MainScale) * MainScale;

        while (value < Maximum)
        {
            DrawHorizontalLine(value, $"{value} {Unit}");

            value += MainScale;
        }

        DrawHorizontalLine(Minimum, $"{Minimum} {Unit}");
        DrawHorizontalLine(Maximum, $"{Maximum} {Unit}");
    }

    public void DrawVerticalLines()
    {
        _dateTimeNow = DateTime.Now;
        _dateTimeStart = _dateTimeNow - TimeInterval;

        DateTime time = _dateTimeStart;

        while (time < _dateTimeNow)
        {
            DrawVerticalLine(time, time.ToString("HH:mm:ss"));
            time += MainTimeScale;
        }

        time = _dateTimeStart;

        while (time < _dateTimeNow)
        {
            DrawVerticalLine(time);
            time += SecondaryTimeScale;
        }

    }

    public void DrawHorizontalLine(double value)
    {
        float y = CalcY(value);
        _canvas.StrokeColor = Colors.Gray;
        _canvas.DrawLine(_marginRect.Left, y, _marginRect.Right, y);
    }

    public void DrawHorizontalLine(double value, string text)
    {
        float y = CalcY(value);
        _canvas.StrokeColor = Colors.White;
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
        float x = CalcX(time);
        _canvas.StrokeColor = Colors.Gray;
        _canvas.DrawLine(x, _marginRect.Top, x, _marginRect.Bottom);
    }

    public void DrawVerticalLine(DateTime time, string text)
    {
        float x = CalcX(time);

        _canvas.StrokeColor = Colors.White;
        _canvas.DrawLine(x, _marginRect.Top, x, _marginRect.Bottom);

        _canvas.FontColor = Colors.White;
        _canvas.DrawString(text.ToString(), x, _marginRect.Bottom + 15 + 3, HorizontalAlignment.Center);
    }

    public void DrawTimeLines()
    {
        //DrawTimeLine(0);
        //DrawTimeLine(100);

        //if (.NoSamples <= ChartsPage.maxQueueCapacity)
         //   for (int i = 0; i <= ChartsPage.maxQueueCapacity / 100; i++)
          //      DrawTimeLine(canvas, dirtyRect, chartData, 100 * i, (10 * i).ToString() + " s");
        //else
        //    for (int i = 1; i <= ChartsPage.maxQueueCapacity / 100; i++)
        //        DrawTimeLine(canvas, dirtyRect, chartData, 100 * i - ChartsPage.NoSamples % 100, (10 * (i + (ChartsPage.NoSamples - ChartsPage.maxQueueCapacity) / 100)).ToString() + " s");
    }

    public void DrawSimpleTimeLines()
    {
        //DrawTimeLine(canvas, dirtyRect, chartData, 0);
        //DrawTimeLine(canvas, dirtyRect, chartData, ChartsPage.maxQueueCapacity);

        //if (ChartsPage.NoSamples <= ChartsPage.maxQueueCapacity)
        //    for (int i = 0; i <= ChartsPage.maxQueueCapacity / 100; i++)
        //        DrawTimeLine(canvas, dirtyRect, chartData, 100 * i);
        //else
        //    for (int i = 1; i <= ChartsPage.maxQueueCapacity / 100; i++)
        //        DrawTimeLine(canvas, dirtyRect, chartData, 100 * i - ChartsPage.NoSamples % 100);
    }

    public void DrawChart()
    {
        PathF path = new PathF();

        for (int i = 0; i < Data.Count; i++)
        {
            float x = 0.0F;//CalcX(Data[i].X);
            float y = CalcY(Data[i].Y);

            if (i == 0)
                //if (chartData.f[i] != -1)
                path.MoveTo(x, y);
            else
                if (Data[i].Y != -1.0)
                path.LineTo(x, y);
        }

        _canvas.StrokeSize = 2;
        _canvas.DrawPath(path);
    }
    #endregion
}


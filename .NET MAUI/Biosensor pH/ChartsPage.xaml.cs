using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Diagnostics;

namespace Biosensor_pH___MAUI;

public partial class ChartsPage : ContentPage
{
    public const int maxQueueCapacity = 600;

    public static Queue<float> SampleTemperature { get; private set; } = new Queue<float>(maxQueueCapacity);
    public static Queue<float> AmbientTemperature { get; private set; } = new Queue<float>(maxQueueCapacity);
    public static Queue<float> AmbientHumidity { get; private set; } = new Queue<float>(maxQueueCapacity);

    public static int CurentChart = 1;
    public static int NoSamples = 0;

    public ChartsPage()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<NewChartSamplesMessage>(this, (recipoent, message) =>
        {
            ChartSample chartSample = (ChartSample)message.Value;

            if (MainThread.IsMainThread)
                AddNewSamples(chartSample);
            else
                MainThread.BeginInvokeOnMainThread(() => AddNewSamples(chartSample));
        });
    }

    private void AddNewSamples(ChartSample chartSample)
    {
        SampleTemperature.Enqueue(chartSample.SampleTemperature);

        if (SampleTemperature.Count > maxQueueCapacity)
            SampleTemperature.Dequeue();

        AmbientTemperature.Enqueue(chartSample.AmbientTemperature);

        if (AmbientTemperature.Count > maxQueueCapacity)
            AmbientTemperature.Dequeue();

        AmbientHumidity.Enqueue(chartSample.AmbientHumidity);

        if (AmbientHumidity.Count > maxQueueCapacity)
            AmbientHumidity.Dequeue();

        NoSamples++;

        GraphicsViewSampleTemperature.Invalidate();
        GraphicsViewAmbientTemperature.Invalidate();
        GraphicsViewAmbientHumidity.Invalidate();
        GraphicsViewCurrent.Invalidate();
    }

    private void GraphicsViewSampleTemperature_StartInteraction(object sender, TouchEventArgs e)
    {
        CurentChart = 1;
        LabelCurrent.Text = "Temperatura próbki";
        Debug.WriteLine("CurentChart: " + CurentChart);
    }

    private void GraphicsViewAmbientTemperature_StartInteraction(object sender, TouchEventArgs e)
    {
        CurentChart = 2;
        LabelCurrent.Text = "Temperatura otoczenia";
        Debug.WriteLine("CurentChart: " + CurentChart);
    }

    private void GraphicsViewAmbientHumidity_StartInteraction(object sender, TouchEventArgs e)
    {
        CurentChart = 3;
        LabelCurrent.Text = "Wilgotnoœæ otoczenia";
        Debug.WriteLine("CurentChart: " + CurentChart);
    }
}

public abstract class GraphicsDrawable : IDrawable
{
    public struct ChartData
    {

        public float fMin;
        public float fMax;
        public float[] f;
        public int Length;

        public ChartData(int length)
        {
            Length = length;
            f = new float[Length];
        }
    }

    public abstract void Draw(ICanvas canvas, RectF dirtyRect);

    public void DrawSettings(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        canvas.FontColor = Color.FromRgb(127, 127, 127);
        canvas.FontSize = 15;
    }

    public RectF MarginRect(RectF dirtyRect, float margin)
    {
        return new RectF(margin, margin, dirtyRect.Width - 2 * margin, dirtyRect.Height - 2 * margin);
    }

    public RectF MarginRect(RectF dirtyRect, float marginLeft, float marginTop, float marginRight, float marginBottom)
    {
        return new RectF(marginLeft, marginTop, dirtyRect.Width - marginLeft - marginRight, dirtyRect.Height - marginTop - marginBottom);
    }

    public float DrawValueLine(ICanvas canvas, RectF dirtyRect, ChartData chartData, float position)
    {
        float y = dirtyRect.Top + dirtyRect.Height - (position - chartData.fMin) * dirtyRect.Height / (chartData.fMax - chartData.fMin);
        canvas.DrawLine(dirtyRect.Left, y, dirtyRect.Right, y);
        return y;
    }

    public void DrawValueLine(ICanvas canvas, RectF dirtyRect, ChartData chartData, float position, string text)
    {
        float y = DrawValueLine(canvas, dirtyRect, chartData, position);

        RectF textRect = new RectF();

        textRect.X = 0;
        textRect.Width = dirtyRect.X - 5;
        textRect.Y = y - 10;
        textRect.Height = 20;

        canvas.DrawString(text.ToString(), textRect, HorizontalAlignment.Right, VerticalAlignment.Center);
    }

    public void DrawTimeLine(ICanvas canvas, RectF dirtyRect, ChartData chartData, float position, string text)
    {
        float x = dirtyRect.Left + position * dirtyRect.Width / (ChartsPage.maxQueueCapacity);
        
        canvas.DrawLine(x, dirtyRect.Top, x, dirtyRect.Bottom);

        canvas.DrawString(text.ToString(), x, dirtyRect.Bottom + 15 + 3, HorizontalAlignment.Center);
    }

    public void DrawTimeLine(ICanvas canvas, RectF dirtyRect, ChartData chartData, float position)
    {
        float x = dirtyRect.Left + position * dirtyRect.Width / (ChartsPage.maxQueueCapacity);

        canvas.DrawLine(x, dirtyRect.Top, x, dirtyRect.Bottom);
    }

    public void DrawTimeLines(ICanvas canvas, RectF dirtyRect, ChartData chartData)
    {
        DrawTimeLine(canvas, dirtyRect, chartData, 0);
        DrawTimeLine(canvas, dirtyRect, chartData, ChartsPage.maxQueueCapacity);

        if (ChartsPage.NoSamples <= ChartsPage.maxQueueCapacity)
            for (int i = 0; i <= ChartsPage.maxQueueCapacity / 100; i++)
                DrawTimeLine(canvas, dirtyRect, chartData, 100 * i, (10 * i).ToString() + " s");
        else
            for (int i = 1; i <= ChartsPage.maxQueueCapacity / 100; i++)
                DrawTimeLine(canvas, dirtyRect, chartData, 100 * i - ChartsPage.NoSamples % 100, (10 * (i + (ChartsPage.NoSamples - ChartsPage.maxQueueCapacity) / 100)).ToString() + " s");
    }

    public void DrawSimpleTimeLines(ICanvas canvas, RectF dirtyRect, ChartData chartData)
    {
        DrawTimeLine(canvas, dirtyRect, chartData, 0);
        DrawTimeLine(canvas, dirtyRect, chartData, ChartsPage.maxQueueCapacity);

        if (ChartsPage.NoSamples <= ChartsPage.maxQueueCapacity)
            for (int i = 0; i <= ChartsPage.maxQueueCapacity / 100; i++)
                DrawTimeLine(canvas, dirtyRect, chartData, 100 * i);
        else
            for (int i = 1; i <= ChartsPage.maxQueueCapacity / 100; i++)
                DrawTimeLine(canvas, dirtyRect, chartData, 100 * i - ChartsPage.NoSamples % 100);
    }

    public void DrawChart(ICanvas canvas, RectF dirtyRect, ChartData chartData)
    {
        PathF path = new PathF();

        for (int i = 0; i < chartData.Length; i++)
        {
            float x = dirtyRect.Left + i * dirtyRect.Width / (ChartsPage.maxQueueCapacity);
            float y = dirtyRect.Top + dirtyRect.Height - (chartData.f[i] - chartData.fMin) * dirtyRect.Height / (chartData.fMax - chartData.fMin);

            if (i == 0)
                //if (chartData.f[i] != -1)
                path.MoveTo(x, y);
            else
                if (chartData.f[i] != -1)
                path.LineTo(x, y);
        }

        canvas.StrokeSize = 2;

        canvas.DrawPath(path);
    }
}

public class GraphicsDrawableSampleTemperature : GraphicsDrawable
{
    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        DrawSettings(canvas, dirtyRect);

#if WINDOWS
        RectF marginRect = MarginRect(dirtyRect, 50, 5, 5, 20);
#else
        RectF marginRect = MarginRect(dirtyRect, 50, 5, 5, 5);
#endif

        /*canvas.FillColor = Color.FromArgb("7FFF0000");
        canvas.FillRectangle(dirtyRect);
        canvas.FillColor = Color.FromArgb("7F00FF00");
        canvas.FillRectangle(marginRect);*/

        ChartData chartData = new ChartData(ChartsPage.SampleTemperature.Count);

        chartData.fMin = 0;
        chartData.fMax = 40;

        chartData.f = ChartsPage.SampleTemperature.ToArray();

        canvas.StrokeColor = Color.FromRgb(127, 127, 127);

        for (int i = 0; i <= 4; i++)
            DrawValueLine(canvas, marginRect, chartData, 10 * i, (10 * i).ToString() + " °C");

#if WINDOWS
        DrawTimeLines(canvas, marginRect, chartData);
#elif ANDROID
        DrawSimpleTimeLines(canvas, marginRect, chartData);
#endif

        canvas.StrokeColor = Colors.Blue;
        DrawChart(canvas, marginRect, chartData);
    }
}

public class GraphicsDrawableAmbientTemperature : GraphicsDrawable
{
    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        DrawSettings(canvas, dirtyRect);

#if WINDOWS
        RectF marginRect = MarginRect(dirtyRect, 50, 5, 5, 20);
#else
        RectF marginRect = MarginRect(dirtyRect, 50, 5, 5, 5);
#endif

        /*canvas.FillColor = Color.FromArgb("7FFF0000");
        canvas.FillRectangle(dirtyRect);
        canvas.FillColor = Color.FromArgb("7F00FF00");
        canvas.FillRectangle(marginRect);*/

        ChartData chartData = new ChartData(ChartsPage.AmbientTemperature.Count);

        chartData.fMin = 0;
        chartData.fMax = 40;

        chartData.f = ChartsPage.AmbientTemperature.ToArray();

        canvas.StrokeColor = Color.FromRgb(127, 127, 127);

        for (int i = 0; i <= 4; i++)
            DrawValueLine(canvas, marginRect, chartData, 10 * i, (10 * i).ToString() + " °C");

#if WINDOWS
        DrawTimeLines(canvas, marginRect, chartData);
#elif ANDROID
        DrawSimpleTimeLines(canvas, marginRect, chartData);
#endif

        canvas.StrokeColor = Colors.Green;
        DrawChart(canvas, marginRect, chartData);
    }
}

public class GraphicsDrawableAmbientHumidity : GraphicsDrawable
{
    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        DrawSettings(canvas, dirtyRect);

#if WINDOWS
        RectF marginRect = MarginRect(dirtyRect, 50, 5, 5, 20);
#else
        RectF marginRect = MarginRect(dirtyRect, 50, 5, 5, 5);
#endif

        /*canvas.FillColor = Color.FromArgb("7FFF0000");
        canvas.FillRectangle(dirtyRect);
        canvas.FillColor = Color.FromArgb("7F00FF00");
        canvas.FillRectangle(marginRect);*/

        ChartData chartData = new ChartData(ChartsPage.AmbientHumidity.Count);

        chartData.fMax = 100;

        chartData.f = ChartsPage.AmbientHumidity.ToArray();

        canvas.StrokeColor = Color.FromRgb(127, 127, 127);

        for (int i = 0; i <= 5; i++)
            DrawValueLine(canvas, marginRect, chartData, 20 * i, (20 * i).ToString() + " %");

#if WINDOWS
        DrawTimeLines(canvas, marginRect, chartData);
#elif ANDROID
        DrawSimpleTimeLines(canvas, marginRect, chartData);
#endif

        canvas.StrokeColor = Colors.Red;
        DrawChart(canvas, marginRect, chartData);
    }
}

public class GraphicsDrawableCurrent : GraphicsDrawable
{
    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        DrawSettings(canvas, dirtyRect);

        RectF marginRect = MarginRect(dirtyRect, 50, 5, 20, 20);

        /*canvas.FillColor = Color.FromArgb("7FFF0000");
        canvas.FillRectangle(dirtyRect);
        canvas.FillColor = Color.FromArgb("7F00FF00");
        canvas.FillRectangle(marginRect);*/

        ChartData chartData = new ChartData();

        chartData.fMin = 0;
        if (ChartsPage.CurentChart == 1)
        {
            chartData = new ChartData(ChartsPage.SampleTemperature.Count);
            chartData.fMin = 0;
            chartData.fMax = 40;
            chartData.f = ChartsPage.SampleTemperature.ToArray();
        }

        if (ChartsPage.CurentChart == 2)
        {
            chartData = new ChartData(ChartsPage.AmbientTemperature.Count);
            chartData.fMin = 0;
            chartData.fMax = 40;
            chartData.f = ChartsPage.AmbientTemperature.ToArray();
        }

        if (ChartsPage.CurentChart == 3)
        {
            chartData = new ChartData(ChartsPage.AmbientHumidity.Count);
            chartData.fMin = 0;
            chartData.fMax = 100;
            chartData.f = ChartsPage.AmbientHumidity.ToArray();
        }

        if (ChartsPage.CurentChart < 3)
        {
            for (int i = 0; i <= 40; i++)
            {
                if (i % 10 == 0)
                {
                    canvas.StrokeColor = Color.FromRgb(127, 127, 127);
                    DrawValueLine(canvas, marginRect, chartData, i, i.ToString() + " °C");
                }
                else {
                    canvas.StrokeColor = Color.FromRgb(63, 63, 63);
                    DrawValueLine(canvas, marginRect, chartData, i);
                }
            }
        }

        if (ChartsPage.CurentChart == 3)
        {
            for (int i = 0; i <= 50; i++)
            {
                if (i % 10 == 0) {
                    canvas.StrokeColor = Color.FromRgb(127, 127, 127);
                    DrawValueLine(canvas, marginRect, chartData, 2 * i, (2 * i).ToString() + " %");
                }
                else {
                    canvas.StrokeColor = Color.FromRgb(63, 63, 63);
                    DrawValueLine(canvas, marginRect, chartData, 2 * i);
                }
            }
        }

        DrawTimeLines(canvas, marginRect, chartData);

        if (ChartsPage.CurentChart == 1)
        {
            canvas.StrokeColor = Colors.Blue;
        }

        if (ChartsPage.CurentChart == 2)
        {
            canvas.StrokeColor = Colors.Green;
        }

        if (ChartsPage.CurentChart == 3)
        {
            canvas.StrokeColor = Colors.Red;
        }

        DrawChart(canvas, marginRect, chartData);
    }
}

public struct ChartSample
{
    public float SampleTemperature;
    public float AmbientTemperature;
    public float AmbientHumidity;

    public ChartSample(float sampleTemperature, float ambientTemperature, float ambientHumidity)
    {
        SampleTemperature = sampleTemperature;
        AmbientTemperature = ambientTemperature;
        AmbientHumidity = ambientHumidity;
    }
}

public class NewChartSamplesMessage : ValueChangedMessage<ChartSample>
{
    public NewChartSamplesMessage(ChartSample chartSample) : base(chartSample)
    {
    }
}


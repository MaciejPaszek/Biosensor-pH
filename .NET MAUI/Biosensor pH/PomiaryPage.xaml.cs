namespace Biosensor_pH___MAUI;

public partial class PomiaryPage : ContentPage
{
	public class Pomiar
	{
		public uint IdPomiaru { get; set; }
		public uint IdPacjenta { get; set; }
        public double pH { get; set; }
        public double TemperaturaPróbki { get; set; }
        public double TemperaturaOtoczenia { get; set; } 
		public double WilgotnoœæOtoczenia { get; set; } 
		public string? DataPomiaru { get; set; } 
		public string? GodzinaPomiaru { get; set; } 
	}

	List<Pomiar> ListaPomiar;

	public PomiaryPage()
	{
		InitializeComponent();

		ListaPomiar = new List<Pomiar>();
        CollectionViewPomiary.ItemsSource = ListaPomiar;


    }

	private void PomiarButton_Clicked(object sender, EventArgs e)
	{
		Pomiar pomiar = new Pomiar();

		DateTime dateTime = DateTime.Now;
        
		pomiar.IdPomiaru = 1;
		pomiar.IdPacjenta = 1;
		pomiar.pH = 0.0;
		pomiar.TemperaturaPróbki = 10.0;
		pomiar.TemperaturaOtoczenia = 10.0;
		pomiar.WilgotnoœæOtoczenia = 10.0;
        
        pomiar.DataPomiaru = Data(dateTime);
        pomiar.GodzinaPomiaru = Godzina(dateTime);

        CollectionViewPomiary.ItemsSource = ListaPomiar;
    }

    private string Data(DateTime dateTime)
    {
        string data = dateTime.Day + ".";

        if (dateTime.Month < 10)
            data += "0";

        data += dateTime.Month + "." + dateTime.Year;

        return data;
    }

    private string Godzina(DateTime dateTime)
    {
        string godzina = string.Empty;

        if (dateTime.Hour < 10)
            godzina += "0";

        godzina += dateTime.Hour + ":";

        if (dateTime.Minute < 10)
            godzina += "0";

        godzina += dateTime.Minute + ":";

        if (dateTime.Second < 10)
            godzina += "0";

        godzina += dateTime.Second;

        return godzina;
    }
}  
namespace Biosensor_pH___MAUI;

public partial class PacjenciPage : ContentPage
{
    public class Pacjent
    {
        public uint Id { get; set; }
        public string Alias { get; set; }
        public string Nazwisko { get; set; }
        public string Imi� { get; set; }
    }

    List<Pacjent> ListPacjent = new List<Pacjent>();

    public PacjenciPage()
	{
		InitializeComponent();

        ListPacjent = new List<Pacjent>
        {
            new Pacjent { Id = 1, Alias = "AA", Nazwisko = "Adamecki", Imi� = "Adam" },
            new Pacjent { Id = 2, Alias = "BB", Nazwisko = "Bartoszewski", Imi� = "Bartosz" },
            new Pacjent { Id = 3, Alias = "CC", Nazwisko = "Czes�awicz", Imi� = "Czes�aw" }
        };

        CollectionViewPacjenci.ItemsSource = ListPacjent;
    }

    private void AddNewButton_Clicked(object sender, EventArgs e)
    {
        ListPacjent.Add(new Pacjent { Id = 1, Alias = "AA", Nazwisko = "Adamecki", Imi� = "Adam" });
        //CollectionViewPacjenci.ItemsSource.
    }
}
using Homie.Areas.Cigars.Models;


namespace Homie.Areas.Series.Models
{
    public class MovieFilterViewModel
    {        
        public MovieFilterViewModel(string name)
        {
            SelectedName = name;
        }        
        public int? SelectedFormat { get; private set; }   // выбранный формат
        public string SelectedName { get; private set; }    // введенное имя
    }
}

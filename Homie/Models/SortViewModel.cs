
namespace Homie.Models
{
    public class SortViewModel
    {
        public SortState NameSort { get; private set; } // значение для сортировки по имени       
        public SortState FormatSort { get; private set; }   // значение для сортировки по формату
        public SortState Current { get; private set; }     // текущее значение сортировки

        public SortViewModel(SortState sortOrder)
        {
            NameSort = sortOrder == SortState.NameAsc ? SortState.NameDesc : SortState.NameAsc;            
            FormatSort = sortOrder == SortState.FormatAsc ? SortState.FormatDesc : SortState.FormatAsc;
            Current = sortOrder;
        }
    }
}

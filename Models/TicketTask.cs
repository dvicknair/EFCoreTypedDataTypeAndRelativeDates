namespace EfCore.Models
{
    public class TicketTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateOnly DueDate { get; set; }
        public List<int> TagIds { get; set; }
    }
}
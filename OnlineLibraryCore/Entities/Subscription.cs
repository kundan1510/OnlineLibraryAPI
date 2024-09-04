namespace OnlineLibraryCore.Entities
{
    public class Subscription
    {
        public string UserEmail { get; set; }
        public string BookId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double Cost { get; set; }

    }
}

namespace Api.Models.Subcribes
{
    public class SubscriptionModel
    {
        public Guid OnWhom { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}

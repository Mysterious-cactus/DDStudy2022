namespace Api.Models.Subcribes
{
    public class SubscribeModel
    {
        public Guid Who { get; set; }
        public Guid OnWhom { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}

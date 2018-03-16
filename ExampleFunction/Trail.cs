namespace ExampleFunction
{
    public class Trail : Owntracks
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Topic { get; set; }
        public string QosLevel { get; set; }
        public bool Retain { get; set; }
    }
}

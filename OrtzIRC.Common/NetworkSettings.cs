namespace OrtzIRC.Common
{
    public class NetworkSettings
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

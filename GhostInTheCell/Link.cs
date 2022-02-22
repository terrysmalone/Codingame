namespace GhostInTheCell
{
    internal sealed class Link
    {
        public int SourceFactory { get; }
        public int DestinationFactory { get; }
        public int Distance { get; }

        public Link(int sourceFactory, int destinationFactory, int distance)
        {
            SourceFactory = sourceFactory;
            DestinationFactory = destinationFactory;
            Distance = distance;
        }
    }
}

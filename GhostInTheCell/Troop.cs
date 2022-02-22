namespace GhostInTheCell
{
    internal sealed class Troop
    {
        public int EntityId { get; }
        public int NumberOfCyborgs { get; }
        public int SourceFactory { get; }
        public int DestinationFactory { get; }
        public int TurnsUntilArrival { get; }

        public Troop(int entityId, int numberOfCyborgs, int sourceFactory, int destinationFactory, int turnsUntilArrival)
        {
            EntityId = entityId;
            NumberOfCyborgs = numberOfCyborgs;
            SourceFactory = sourceFactory;
            DestinationFactory = destinationFactory;
            TurnsUntilArrival = turnsUntilArrival;
        }
    }
}

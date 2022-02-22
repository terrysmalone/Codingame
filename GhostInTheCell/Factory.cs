using System.Collections.Generic;

namespace GhostInTheCell
{
    internal sealed class Factory
    {
        public int Id { get; }
        public List<Link> Links { get; }

        public Owner Owner { get; private set; }
        public int NumberOfCyborgs { get; private set;  }
        public int Production { get; private set;  }

        public Factory(int id, List<Link> links)
        {
            Id = id;

            Links = NormaliseLinks(links);
        }
        private List<Link> NormaliseLinks(List<Link> links)
        {
            var normalisedLinks = new List<Link>();

            foreach (var link in links)
            {
                if(link.SourceFactory == Id)
                {
                    normalisedLinks.Add(new Link(link.SourceFactory,
                                                     link.DestinationFactory,
                                                     link.Distance));
                }
                else
                {
                    normalisedLinks.Add(new Link(link.DestinationFactory,
                                                     link.SourceFactory,
                                                     link.Distance));
                }
            }

            return normalisedLinks;
        }
        public void Update(Owner owner, int numberOfCyborgs, int factoryProduction)
        {
            Owner = owner;
            NumberOfCyborgs = numberOfCyborgs;
            Production = factoryProduction;
        }
    }
}

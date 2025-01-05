using System.ComponentModel;
using System.Drawing;

namespace WinterChallenge2024;

internal struct Organ
{
    internal int Id { get; private set; }
    internal int RootId { get; private set; }

    public OrganType Type { get; set; }

    internal Point Position { get; private set; }

    internal int ParentId { get; private set; }

    internal OrganDirection Direction { get; private set; }

    public Organ(int id, int rootId, OrganType type, Point position, int parentId) : this()
    {
        Id = id;
        RootId = rootId;
        Type = type;
        Position = position;
        ParentId = parentId;
    }

    public Organ(int id, int rootId, OrganType type, Point position, int parentId, OrganDirection direction) : this(id, rootId, type, position, parentId)
    {
        Direction = direction;
    }
}
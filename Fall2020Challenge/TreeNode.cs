﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fall2020Challenge
{
    internal class TreeNode
    {
        private readonly int[] _change;
        public TreeNode Parent { get; }
        public List<Spell> CurrentSpells { get; }
        public int[] PlayerIngredients { get; }

        public string Action { get; }

        private List<TreeNode> _children;

        public TreeNode(TreeNode parent, List<Spell> currentSpells, int[] playerIngredients, string action, int[] change)
        {
            _change = change;
            Parent = parent;
            CurrentSpells = currentSpells;
            PlayerIngredients = playerIngredients;
            Action = action;

            _children = new List<TreeNode>();
        }

        internal void AddChild(TreeNode child)
        {
            _children.Add(child);
        }
    }

    // public class TreeNode<T>
    // {
    //     private readonly T _value;
    //     private readonly List<TreeNode<T>> _children = new List<TreeNode<T>>();
    //
    //     public TreeNode(T value)
    //     {
    //         _value = value;
    //     }
    //
    //     public TreeNode<T> this[int i]
    //     {
    //         get { return _children[i]; }
    //     }
    //
    //     public TreeNode<T> Parent { get; private set; }
    //
    //     public T Value { get { return _value; } }
    //
    //     public ReadOnlyCollection<TreeNode<T>> Children
    //     {
    //         get { return _children.AsReadOnly(); }
    //     }
    //
    //     public TreeNode<T> AddChild(T value)
    //     {
    //         var node = new TreeNode<T>(value) {Parent = this};
    //         _children.Add(node);
    //         return node;
    //     }
    //
    //     public TreeNode<T>[] AddChildren(params T[] values)
    //     {
    //         return values.Select(AddChild).ToArray();
    //     }
    //
    //     public bool RemoveChild(TreeNode<T> node)
    //     {
    //         return _children.Remove(node);
    //     }
    //
    //     public void Traverse(Action<T> action)
    //     {
    //         action(Value);
    //         foreach (var child in _children)
    //             child.Traverse(action);
    //     }
    //
    //     public IEnumerable<T> Flatten()
    //     {
    //         return new[] {Value}.Concat(_children.SelectMany(x => x.Flatten()));
    //     }
    // }
}

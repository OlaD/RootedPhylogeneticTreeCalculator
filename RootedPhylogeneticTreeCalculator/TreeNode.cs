﻿using System.Collections.Generic;

namespace RootedPhylogeneticTreeCalculator
{
    public class TreeNode
    {
        public string Label { get; set; }
        public string AncestorEdgeLabel { get; set; }
        public HashSet<TreeNode> Children { get; }
        public int Id { get; }
        private static int nextId = 0;

        public bool IsLeaf
        {
            get
            {
                if (Children.Count == 0)
                    return true;
                return false;
            }
        }

        public Cluster Cluster
        {
            get
            {
                HashSet<TreeNode> cluster = new HashSet<TreeNode>();
                if (IsLeaf)
                    cluster.Add(this);
                else
                {
                    foreach (var child in Children)
                        cluster.UnionWith(child.Cluster.Elements);
                }
                return new Cluster(cluster);
            }
        }

        public TreeNode(string label = "")
        {
            Label = label;
            Children = new HashSet<TreeNode>();
            Id = nextId;
            nextId++;
        }

        public TreeNode AddChild(string label = "")
        {
            TreeNode child = new TreeNode(label);
            Children.Add(child);
            return child;
        }

        // pobiera klastry z węzła i jego dzieci
        public List<Cluster> GetAllClustersFromSubtree()
        {
            List<Cluster> clusters = new List<Cluster>();
            clusters.Add(Cluster);
            foreach (var child in Children)
                clusters.AddRange(child.GetAllClustersFromSubtree());
            return clusters;
        }

        public bool IsChildOf(TreeNode node)
        {
            return node.Children.Contains(this);
        }
    }
}

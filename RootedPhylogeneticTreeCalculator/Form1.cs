﻿using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RootedPhylogeneticTreeCalculator
{
    public partial class Form1 : Form
    {
        GViewer viewer;

        List<TreeNode> trees = new List<TreeNode>();
        List<Graph> graphs = new List<Graph>();
        int emptyNodeCounter = 0;
        int edgeCounter = 0;
        bool deleteEmpty = false;
        String rootName = "root";

        public Form1()
        {
            if (deleteEmpty)
            {
                rootName = "      ";
            }
            InitializeComponent();
            viewer = new GViewer();
        }

        public void ShowGraph(Graph graph)
        {
            viewer.Graph = graph;
            SuspendLayout();
            viewer.Dock = DockStyle.Fill;
            graphPanel.Controls.Add(viewer);
            ResumeLayout();
        }

        private void loadFileButton_Click(object sender, System.EventArgs e)
        {
            openFileDialog1.Filter = "xml|*.xml";
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    PhyloXMLParser parser = new PhyloXMLParser();
                    Tree tree = parser.LoadTree(file);

                    // Wyswietlanie wczytanego pliku
                    Graph graph = AddGraph();
                    listBox1.Items.Add(file.Substring(file.LastIndexOf('\\'), file.Length - file.LastIndexOf('\\')));

                    tree.Label = rootName;//"root";
                    treeToGraph(tree, tree.Label, graph, checkBox1.Checked);
                    ShowGraph(graph);

                    // Sprawdzenie poprawnosci drzewa. Wyswietlamy jedynie informacje, ale i tak wyswietlamy.
                    checkTree(tree);

                    // Dodanie drzewa do listy drzew.
                    trees.Add(tree);

                    // testowe
                    //TreeNode n = tree.Children.First().Children.First();
                    //Split split = SplitCalculator.GetSplitForEdgeTo(n, tree);
                    //MessageBox.Show(split.ToString());
                }
            }
        }

        private Graph AddGraph()
        {
            Graph graph = new Graph();
            graphs.Add(graph);
            return graph;
        }

        private void rozbicieKrawedzi(Object sender, EventArgs e)
        {
            String edgeName = "";
            //List<int> edgeList = new List<int>();
            ////String edgeName = rozbicieTextBox.Text;
            //List<String> firstNodes = new List<String>();
            //List<String> secondNodes = new List<String>();
            //if(listBox1.SelectedIndices.Count == 0)
            //{
            //    return;
            //}
            //TreeNode tree = trees[listBox1.SelectedIndices[0]];

            foreach (IViewerObject kuku in viewer.Entities)
            {
                if (kuku.GetType().Name == "DEdge")
                {
                    DEdge abc = (DEdge)kuku;
                    //abc.SelectedForEditing;
                    if (abc.SelectedForEditing)
                    {
                        edgeName = abc.Edge.LabelText;
                        MessageBox.Show(abc.Edge.Source + " " + abc.Edge.Target);

                        // IViewerNode target = ((IViewerEdge)kuku).Target;
                    }
                    //edgeList.Add(abc.Edge.LabelText.Length);
                }
                var adsdsa = kuku.GetType();
            }

            //collectNodes(tree, edgeName, firstNodes, secondNodes, false);

            //String rozbicieText = "";
            //rozbicieText = "{{";
            //foreach (String nodeName in firstNodes)
            //{
            //    rozbicieText += nodeName + ", ";
            //}
            //rozbicieText.Remove(rozbicieText.Length - 1);
            //rozbicieText += "},\n\n{";
            //foreach (String nodeName in secondNodes)
            //{
            //    rozbicieText += nodeName + ", ";
            //}
            //rozbicieText.Remove(rozbicieText.Length - 1);
            //rozbicieText += "}}";

            //const string caption = "Rozbicie krawedzi";
            //var result = MessageBox.Show(rozbicieText, caption,
            //                             MessageBoxButtons.OK,
            //                             MessageBoxIcon.Information);
        }

        private void collectNodes(TreeNode currentNode, String edgeName, List<String> firstNodes, List<String> secondNodes,
            bool isAncestorFound)
        {
            if (currentNode.AncestorEdgeLabel == edgeName)
            {
                isAncestorFound = true;
            }

            
            // Wezel musi miec nazwe
            if (currentNode.Label != "" && currentNode.Label[0] != ' ' && currentNode.Label != rootName)
            {
                // Sprawdza, czy rozbiciem nie jest wezel poprzedzajacy
                if (!isAncestorFound)
                {
                    firstNodes.Add(currentNode.Label);
                }
                else
                {
                    secondNodes.Add(currentNode.Label);
                }
            }

            if (!currentNode.IsLeaf)
            {
                foreach (TreeNode child in currentNode.Children)
                {
                    collectNodes(child, edgeName, firstNodes, secondNodes, isAncestorFound);
                }
            }

        }
        // Przeszukuje rekurencyjnie nody, dodaje kazdy klaster do listy
        private void addClusters(TreeNode currentNode, List<String> clusterNames)
        {
            if (!currentNode.IsLeaf)
            {
                foreach (TreeNode child in currentNode.Children)
                {
                    addClusters(child, clusterNames);
                }
            }
            clusterNames.Add(currentNode.Cluster.ToString());
        }

        // Dodaje krawedzie na podstawie rekurencyjnego przeszukania drzewa
        private void treeToGraph(TreeNode currentNode, String ancestorLabel, Graph graph, bool showCluster)
        {

            // Dodaje spacje do pustych nodow, przy dodaniu do krawedzi nie moze byc pustych
            if (!deleteEmpty && currentNode.Label == "")
            {
                for (int i = 0; i <= emptyNodeCounter; i++)
                {
                    // Brzydki workaround na puste <clad> bez <name>
                    currentNode.Label += " ";
                }
                emptyNodeCounter++;
            }

            // Rekurencyjne przeszukiwanie
            if (!currentNode.IsLeaf)
            {
                foreach (TreeNode child in currentNode.Children)
                {
                    String ancestorString = currentNode.Label;
                    if (deleteEmpty && currentNode.Label == "")
                    {
                        ancestorString = ancestorLabel;
                    }
                    treeToGraph(child, ancestorString, graph, showCluster);
                }
                if (currentNode.Label == rootName)
                {
                    Node node = graph.AddNode(rootName);
                    if (showCluster)
                        node.LabelText = currentNode.Cluster.ToString();                    
                    return;
                }
            }

            if ((!deleteEmpty) || (deleteEmpty && (currentNode.Label != "") && (currentNode.Label != rootName)))// && ancestorLabel != rootName))
            {
                Edge edge = graph.AddEdge(ancestorLabel, currentNode.Label);
                edge.Attr.ArrowheadAtSource = ArrowStyle.None;
                edge.Attr.ArrowheadAtTarget = ArrowStyle.None;

                //Labelki do krawedzi
   
                edge.LabelText = "";
                for (int counterr = 0; counterr < edgeCounter; counterr++)
                {
                    edge.LabelText += ' ';
                }

                //"edge" + edgeCounter.ToString();
                edgeCounter++;

                // Zapamietanie krawedzi jako laczacej z przodkiem (przyda sie do rozbicia drzewa)

                currentNode.AncestorEdgeLabel = edge.LabelText;

                if (showCluster)
                {
                    edge.TargetNode.LabelText = currentNode.Cluster.ToString();
                }
            }
            else if (deleteEmpty && (currentNode.Label != "") && (currentNode.Label == rootName))
            {

            }

        }

        // Sprawdza zgodnosc drzewa (czy nie powtarzaja sie wierzcholki)
        private void checkTree(TreeNode tree)
        {
            List<String> nodesLabelsList = new List<String>();
            if (allNodesLabelsToList(nodesLabelsList, tree))
            {
                treeCheckLabel.Text = "Tak";
            }
            else
            {
                treeCheckLabel.Text = "Nie";
            }
        }

        // Funkcja zwraca false jezeli ktorys label sie powtorzyl
        private bool allNodesLabelsToList(List<string> leafsList, TreeNode currentNode)
        {
            bool status = true;

            if (!currentNode.IsLeaf)
            {
                foreach (TreeNode child in currentNode.Children)
                {
                    if (!allNodesLabelsToList(leafsList, child))
                    {
                        // Jezeli ktores z childow zwrocilo false oznacza niezgodnosc i dalej nie sprawdzamy
                        status = false;
                    }
                }
            }

            if(leafsList.Contains(currentNode.Label))
            {
                // Znaleziono taki sam label, zwracamy false
                status = false;
            }

            // Dodanie do listy w celu pozniejszych porownan
            leafsList.Add(currentNode.Label);

            return status;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BuildConsensusTree();
        }

        private void BuildConsensusTree()
        {
            int x = Int32.Parse(textBox1.Text);
            ConsensusTreeBuilder consensusTreeBuilder = new ConsensusTreeBuilder();
            List<TreeNode> selectedTrees = new List<TreeNode>();
            foreach (int index in listBox1.SelectedIndices)
                selectedTrees.Add(trees[index]);
            Graph graph = consensusTreeBuilder.CreateConsensusTree(selectedTrees, x);
            ShowGraph(graph);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((listBox1.SelectedIndex >=0) && (listBox1.SelectedIndex <= trees.Count))
            {
                checkTree(trees[listBox1.SelectedIndex]);
                ShowGraph(graphs[listBox1.SelectedIndex]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Sprawdzenie odleglosci RF miedzy drzewami.
            checkDistance();
        }

        private void checkDistance()
        {
            if (trees.Count > 1)
            {
                // Pobranie dwoch wybranych drzew
                if(listBox1.SelectedIndices.Count != 2)
                {
                    MessageBox.Show("wybierz 2 drzewa");
                    return;
                }
                TreeNode tree1 = trees[listBox1.SelectedIndices[0]];
                TreeNode tree2 = trees[listBox1.SelectedIndices[1]];

                List<String> clusterNames1 = new List<String>();
                List<String> clusterNames2 = new List<String>();

                // Tworzy liste klastrow poszczegolnych drzew
                addClusters(tree1, clusterNames1);
                addClusters(tree2, clusterNames2);

                int uniqueClustersCount = 0;

                // Sprawdzenie unikalnych klastrow
                foreach (String clusterName in clusterNames2)
                {
                    if (!clusterNames1.Contains(clusterName))
                    {
                        uniqueClustersCount++;
                    }
                }

                foreach (String clusterName in clusterNames1)
                {
                    if (!clusterNames2.Contains(clusterName))
                    {
                        uniqueClustersCount++;
                    }
                }

                float rfDistance = uniqueClustersCount / 2.0f;
                rfDistanceLabel.Text = rfDistance.ToString();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            graphs.Clear();
            foreach(TreeNode tree in trees)
            {
                Graph graph = AddGraph();
                tree.Label = rootName;
                treeToGraph(tree, tree.Label, graph, checkBox1.Checked);
            }
            if(listBox1.SelectedIndex == -1)
                ShowGraph(graphs.Last());
            else
                ShowGraph(graphs[listBox1.SelectedIndex]);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Compatibility();
        }

        private void Compatibility()
        {
            // Pobranie dwoch wybranych drzew
            if (listBox1.SelectedIndices.Count != 2)
            {
                MessageBox.Show("wybierz 2 drzewa");
                return;
            }
            TreeNode tree1 = trees[listBox1.SelectedIndices[0]];
            TreeNode tree2 = trees[listBox1.SelectedIndices[1]];

            HashSet<Cluster> clusters = new HashSet<Cluster>();
            clusters.UnionWith(tree1.GetAllClustersFromSubtree());
            clusters.UnionWith(tree2.GetAllClustersFromSubtree());

            bool zgodne = true;
            foreach (Cluster c in clusters)
            {
                foreach (Cluster other in clusters)
                {
                    if ( !(c.IsSubsetOf(other) || other.IsSubsetOf(c) || c.IsDisjointFrom(other)) )
                        zgodne = false;
                }
            }

            if (zgodne)
                compLabel.Text = "Zgodne";
            else
                compLabel.Text = "Niezgodne";
        }
    }
}

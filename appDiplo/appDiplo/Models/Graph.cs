using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace appDiplo.Models
{
    /// <summary>
    /// Class representing graph.
    /// </summary>
    public class Graph
    {
        public List<POI> Nodes { get; }
        public int Nodes_count { get; private set; }
        
        /// <summary>
        /// Constructor of the class.
        /// </summary>
        public Graph()
        {
            Nodes = new List<POI>();
            Nodes_count = 0;
        }
        /// <summary>
        /// Method for adding node to the end of list;
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(POI node)
        {
            Nodes.Add(node);
            Nodes_count++;
        }
        /// <summary>
        /// Method for creating directed edge between nodes.
        /// </summary>
        /// <param name="from">Start node.</param>
        /// <param name="to">End node</param>
        /// <param name="time">tTravel Time between nodes.</param>
        public void AddDirectedEdge(POI from, POI to, int time)
        {
            from.Neighbours.Add(to);
            from.TravelTime.Add(time);
        }

        /// <summary>
        /// Method for creating directed edge
        /// </summary>
        /// <param name="from">Nodes from.</param>
        /// <param name="to">Nodes to. </param>
        /// <param name="data">Travel times.</param>
        public void AddDirectedEdge(POI from, List<POI> to, List<Results2> data)
        {
            int j = 0;
            if (data != null)
            {
                for (int i = 0; i < to.Count; i++)
                {
                    if (to[i] != from)
                    {
                        from.Neighbours.Add(to[i]);
                        from.TravelTime.Add((int)data[j].travelDuration);
                        from.Distance.Add((int)data[j].travelDuration);
                        j++;
                    }
                }
            }
        }
        /// <summary>
        /// Method for adding undirecting edge. Using for loading test data.
        /// </summary>
        /// <param name="from">Node from.</param>
        /// <param name="to">Node to.</param>
        /// <param name="distance">Time distance between nodes.</param>
        public void AddUndirectedEdge(POI from, POI to, double distance)
        {
            from.Neighbours.Add(to);
            to.Neighbours.Add(from);
            from.Distance.Add(distance);
            to.Distance.Add(distance);
        }

    }
}

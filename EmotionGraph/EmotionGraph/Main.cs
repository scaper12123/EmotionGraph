using System;
using System.Collections.Generic;
using System.IO;

namespace EmotionGraph
{
    class Test
    {
        // boolean switch for testing
        static Boolean DEBUG = true;

        static void Main()
        {
            if (DEBUG)
            {
                string temp;
                List<int> tempListInt = new List<int>();

                // initilize the graph with NUM_EMOTIONS+1 nodes. each sub-tree of the root node will contain a max-value emotion.
                Console.WriteLine("Initializing graph with " + EmotionGraph.NUM_EMOTIONS + " emotion values.");

                EmotionGraph.Node graph = EmotionGraph.InitGraph(EmotionGraph.IFP_NAME);

                Console.WriteLine("Done forming graph base. Adding test nodes...");
                graph.AddNode(new List<int>() { 20, 0, 0, 0, 0 }, "Faint Smile");
                graph.AddNode(new List<int>() { 0, 40, 0, 60, 0 }, "Depression");
                graph.AddNode(new List<int>() { 0, 0, 40, 60, 0 }, "Horror");

                Console.WriteLine("Testing finding nodes...");
                string emotion = graph.FindEmotion(new List<int>() { 90, 0, 0, 0, 0 });
            }
        }
    }
}

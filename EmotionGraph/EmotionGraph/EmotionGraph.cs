using System;
using System.Collections.Generic;
using System.IO;

namespace EmotionGraph
{
    public class EmotionGraph
    {
        // list for keeping record of all the emotion nodes we have in the graph.
        public static List<GraphEntry> GRAPH_LISTINGS = new List<GraphEntry>();

        // the default name of a files for loading/saving emotion names and associated values
        public static string IFP_NAME = "GraphEntries.txt";

        // if values are too small, we should ignore them. This changes what value is considered "too small"
        public static int IRRELEVANT = 15;

        // list & ordering of emotions considered by the program. List should be changed with caution
        public static List<string> EMOTIONS = new List<string>() { "Joy", "Sad", "Anger", "Disgust", "Suprise" };
        public static int NUM_EMOTIONS = EMOTIONS.Count;

        // for finding a node, these are used to determine if a string we've chosen is the best fit.
        static int max_distance = IRRELEVANT;       // the maximum distance from a value that should be checked for possible matches; recommend setting this equal to IRRELEVANT
        static int store_this_sum = Int32.MaxValue; // a value representing the sum of the emotion values of an emotion retrieved from the tree. 
        static int confidence = -1;
        static int target_sum = -1;                  // the sum of the values of an emotion being searched for. For a perfect match, target_sum-confidence = 0
        static string store_this_emotion = "";      // a global string to carry an emotion of closest fit

        // boolean switch for testing
        static Boolean DEBUG = true;

        static void Main()
        {
            if (DEBUG)
            {
                string temp;
                List<int> tempListInt = new List<int>();

                // initilize the graph with NUM_EMOTIONS+1 nodes. each sub-tree of the root node will contain a max-value emotion.
                Console.WriteLine("Initializing graph with " + NUM_EMOTIONS + " emotion values.");

                Node graph = InitGraph(IFP_NAME);

                Console.WriteLine("Done forming graph base. Adding test nodes...");
                graph.AddNode(new List<int>() { 20, 0, 0, 0, 0 }, "Faint Smile");
                graph.AddNode(new List<int>() { 0, 40, 0, 60, 0 }, "Depression");
                graph.AddNode(new List<int>() { 0, 0, 40, 60, 0 }, "Horror");

                Console.WriteLine("Testing finding nodes...");
                string emotion = graph.FindEmotion(new List<int>() { 90, 0, 0, 0, 0 });
            }
        }

        // initialize a graph which has branches each leading to a the base emotion
        public static Node InitGraph(string filename)
        {
            string[] lines = File.ReadAllLines(@filename);

            Node newGraph = new Node(EMOTIONS);

            foreach (string line in lines)
            {
                string[] parse = line.Split(' ');
                List<int> values = new List<int>();
                for (int i = 0; i < NUM_EMOTIONS; i++)
                {
                    values.Add(int.Parse(parse[i]));
                }
                string data = parse[parse.Length - 1];
                newGraph.AddNodeInit(values, data);
            }

            return newGraph;
        }

        // function for finding the peak inded of a List of ints
        public static int GetPeakIndex(List<int> arr)
        {
            // assume a huge peak. also assign a dummy to our index variable
            int peakValue = Int32.MinValue;
            int peakIndex = 0;

            // base cases
            if (arr == null)
                return -1;
            else if (arr.Count == 0)
                return -1;

            // loop through the array
            foreach (int i in arr)
            {
                // if this value is peak, keep this value in our index variable
                if (i > peakValue)
                {
                    peakIndex = arr.IndexOf(i);
                    peakValue = i;
                }
            }

            return peakIndex;
        }

        // find the index of a value in the array which is "closest" to a value
        public static int GetClosestValueIndex(List<int> arr, int val)
        {
            // assign dummy value for closest index, also a dummy difference
            int closest = -1, difference = 101;

            // loop through the array
            for (int i = 0; i < arr.Count; i++)
            {
                // if the abs value of difference between this index and val is less than diff,
                if (Math.Abs(arr[i] - val) < difference)
                {
                    // save this index as the closest index
                    difference = Math.Abs(arr[i] - val);
                    closest = i;
                }
            }

            return closest;
        }

        // helper function to get a list sum, bearing irrelevancy in mind
        public static int Sum(List<int> values)
        {
            int total = 0;
            foreach (int i in values)
            {
                if (i >= IRRELEVANT)
                    total += i;
            }
            return total;
        }

        public struct Node
        {
            public string emotionString;
            public List<string> emotions;
            public Dictionary<string, Dictionary<int, Node>> values; // one leaf is added for each emotion in this particular node

            // Node struct where this node leads to another node
            public Node(List<string> emotions)
            {
                emotionString = "";
                values = new Dictionary<string, Dictionary<int, Node>>();
                this.emotions = new List<string>();
                this.emotions.AddRange(emotions);
                foreach (string s in this.emotions)
                {
                    values[s] = new Dictionary<int, Node>();
                }
            }

            // initalize graph addition
            public void AddNode(List<int> values, string data)
            {
                List<string> emotions = new List<string>();
                emotions.AddRange(EMOTIONS);

                GRAPH_LISTINGS.Add(new GraphEntry(values, data));

                this.InsertNode(values, emotions, data);
            }

            // initalize graph addition with the caviat that all nodes added already exist in our list
            public void AddNodeInit(List<int> values, string data)
            {
                List<string> emotions = new List<string>();
                emotions.AddRange(EMOTIONS);

                if (DEBUG)
                {
                    GRAPH_LISTINGS.Add(new GraphEntry(values, data));
                }

                this.InsertNode(values, emotions, data);
            }

            // adds a node to the graph
            private void InsertNode(List<int> values, List<string> emotions, string data)
            {
                int peakIndex, peakValue, peakValueRounded;
                string peakEmotion;
                List<int> valuesCopy = new List<int>();
                List<string> emotionsCopy = new List<string>();
                Boolean irrelevancyFlag;

                // make a copy of the values and emotions to avoid eliminating data
                valuesCopy.AddRange(values);
                emotionsCopy.AddRange(emotions);

                // determine which value is largest and determine its index
                peakIndex = GetPeakIndex(valuesCopy);

                // obtain the value and a rounded value for the peak. Remove the peak from the values list and its corresponding emotion
                peakValue = valuesCopy[peakIndex];
                peakEmotion = emotionsCopy[peakIndex];
                peakValueRounded = (valuesCopy[peakIndex] / 5) * 5;
                valuesCopy.RemoveAt(peakIndex);
                emotionsCopy.RemoveAt(peakIndex);

                // determine if the peak value is the only relevant value
                irrelevancyFlag = true;
                foreach (int i in valuesCopy)
                    if (i > IRRELEVANT) irrelevancyFlag = false;

                // check to see if a definition for this value exists
                // if it does not, create a new reference
                if (!this.values[peakEmotion].ContainsKey(peakValueRounded))
                {
                    this.values[peakEmotion].Add(peakValueRounded, new Node(emotionsCopy));
                }

                // if the remaining values are irrelevant, give the next node the data and return
                if (irrelevancyFlag)
                {
                    Node temp = this.values[peakEmotion][peakValueRounded];
                    temp.emotionString = data;
                    this.values[peakEmotion][peakValueRounded] = temp;
                    return;
                }

                // otherwise continue down to the next node
                this.values[peakEmotion][peakValueRounded].InsertNode(valuesCopy, emotionsCopy, data);

                return;
            } // end of insertNode

            // a method to initialize graph search
            public string FindEmotion(List<int> values)
            {
                List<string> emotions = new List<string>();
                emotions.AddRange(EMOTIONS);

                store_this_sum = Int32.MaxValue;
                store_this_emotion = "";
                target_sum = Sum(values);
                confidence = -1;

                if (values[GetPeakIndex(values)] < IRRELEVANT)
                {
                    return "neutral";
                }

                this.FindNode(values, emotions, 0);
                if (store_this_emotion == "")
                {
                    store_this_emotion = emotions[GetPeakIndex(values)];
                    return emotions[GetPeakIndex(values)];
                }
                else
                {
                    return store_this_emotion;
                }
            }

            private void FindNode(List<int> values, List<string> emotions, int valSum)
            {
                int peakIndex, peakValue, peakValueRounded, num, bestKey, bestKeyIndex;
                List<int> valuesCopy = new List<int>();
                string peakEmotion;
                List<string> emotionsCopy = new List<string>();
                Boolean irrelevancyFlag;

                // make a copy of the values and emotions to avoid eliminating data
                valuesCopy.AddRange(values);
                emotionsCopy.AddRange(emotions);

                // determine if any relevant values exist
                irrelevancyFlag = true;
                foreach (int i in valuesCopy)
                    if (i > IRRELEVANT) irrelevancyFlag = false;

                // if no relevant values exist, return the string stored by this node
                if (irrelevancyFlag)
                {
                    if (Math.Abs(target_sum - valSum) < Math.Abs(target_sum - store_this_sum))
                    {
                        confidence = Math.Abs(target_sum - valSum);
                        store_this_sum = valSum;
                        store_this_emotion = this.emotionString;
                    }

                    return; // return after this point
                }
                //otherwise, we continue by finding which node to go to next

                // determine which value is largest and determine its index
                peakIndex = GetPeakIndex(valuesCopy);

                // obtain the value and a rounded value for the peak. Remove the peak from the values list and its corresponding emotion
                peakValue = valuesCopy[peakIndex];
                peakEmotion = emotionsCopy[peakIndex];
                peakValueRounded = (valuesCopy[peakIndex] / 5) * 5;
                valuesCopy.RemoveAt(peakIndex);
                emotionsCopy.RemoveAt(peakIndex);

                // obtain an array of the key values
                List<int> keyValues = new List<int>(this.values[peakEmotion].Keys);

                // make an int for the number of keys in the sub-node, and set a counter to 1
                num = keyValues.Count;

                // for the best value in the sub-node
                while (keyValues.Count > 0)
                {
                    // choose the closest value in the array
                    bestKeyIndex = GetClosestValueIndex(keyValues, peakValue);
                    bestKey = keyValues[bestKeyIndex];

                    // perform FindNode on the next node in this sequence
                    this.values[peakEmotion][bestKey].FindNode(valuesCopy, emotionsCopy, valSum + bestKey);

                    // if it returns a non-empty string, return
                    if (Math.Abs(bestKey - peakValueRounded) > IRRELEVANT)
                    {
                        return;
                    }

                    // if we reach this point, the key didn't return anything. remove it from the list
                    keyValues.RemoveAt(bestKeyIndex);
                }

                // if we've failed to find a node by now, try and return this emotion node.
                if (Math.Abs(target_sum - valSum) < Math.Abs(target_sum - store_this_sum))
                {
                    confidence = Math.Abs(target_sum - valSum);
                    store_this_sum = valSum;
                    store_this_emotion = this.emotionString;
                }

                return; // end of FindNode
            }

            // getter function to return graph listings
            public List<GraphEntry> GetList()
            {
                return GRAPH_LISTINGS;
            }

            public int GetConfidence()
            {
                return confidence;
            }
        }

        public struct GraphEntry
        {
            public string emotion;
            public List<int> values;

            public GraphEntry(List<int> values, string emotion)
            {
                this.emotion = emotion;
                this.values = new List<int>();
                this.values.AddRange(values);
            }

            public string WriteThisEmotion()
            {
                string result = "";

                foreach (int i in this.values)
                {
                    result += i + " ";
                }

                return result + this.emotion;
            }
        }
    }
}

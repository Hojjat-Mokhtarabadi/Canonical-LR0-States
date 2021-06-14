using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LALR_Parser
{
    class DFA_builder
    {
        int statesId = -1;
        List<GrammerModel> grammer;
        List<char> teminalSet = new List<char>() { '=', 'i', '*'};
        List<char> nonTeminalSet = new List<char>();
        Queue<State> dfaTempStates = new Queue<State>();
        List<State> dfaStates = new List<State>();
        List<State> dfaStageMode = new List<State>();
        public DFA_builder(List<GrammerModel> grammer)
        {
            this.grammer = grammer;
            dfaTempStates.Enqueue(new State(-1, -1, '~', new List<GrammerModel>() { new GrammerModel('Q', new List<char>() { ' ','S' }, 0, new List<char>() { '$' }) }));
            dfaStates.Add(new State(-1, -1, '~', new List<GrammerModel>() { new GrammerModel('Q', new List<char>() { ' ', 'S' }, 0, new List<char>() { '$' }) }));
        }

        //first
        //this method is a recursive function which computes first set of a given list of chars
        public string computeFirst(List<char> lst, int index, char symbol)
        {
            string str = "";
            var gmList = grammer.FindAll(x => x.antecedent == symbol);

            if (index == lst.Count)
            {
                return '~'.ToString(); 
            }

            if (teminalSet.Exists((x) => x == lst[index]))
            {
                return lst[index].ToString();
            }
            else 
            {
                foreach (var item in gmList)
                {
                    str += computeFirst(item.consequent, 0, lst[index]);
                }
                return str;
            }

        }

        //closure
        //this function has the same data structure as builder for iterating
        //it gets first symbol from consequent as a next symbol for expansion
        public List<GrammerModel> closure(char symbol, int index, List<char> lookahead)
        {
            Queue<char> closureExpantionSymbols = new Queue<char>();
            List<GrammerModel> closureSet = new List<GrammerModel>();
            List<char> expandedSymbols = new List<char>();

            //Console.WriteLine($"symbol {symbol}");
            closureExpantionSymbols.Enqueue(symbol);
            do
            {

                symbol = closureExpantionSymbols.Dequeue();
                var gmList = grammer.FindAll(x => x.antecedent == symbol);
                foreach (var item in gmList)
                {
                    var first = item.consequent.First();
                    if (!expandedSymbols.Exists(x => x == first))
                    {
                        closureExpantionSymbols.Enqueue(item.consequent.First());
                        expandedSymbols.Add(first);
                    }
                    closureSet.Add(new GrammerModel(item.antecedent, item.consequent, 0, lookahead.ToList()));
                }

            }
            while (closureExpantionSymbols.Count != 0);
            return closureSet;
        }

        //goto
        //goto function maps current grammer and index to a new set of grammer items. closure fucntion is used in this method
        public void gotoFunction(GrammerModel grammer, int prevStateId, char relation)
        {
            //Console.WriteLine("in goto");
            List<GrammerModel> generatedGrammer = new List<GrammerModel>();
            List<GrammerModel> clousreSet = new List<GrammerModel>();
            List<char> beta = new List<char>();
            List<char> newLookahead = new List<char>();

            //check if this grammer has been already added
            if (dfaStates.Exists((x) => x.grammer[0].antecedent == grammer.antecedent && x.grammer[0].consequent == grammer.consequent 
                                                             && x.grammer[0].index == grammer.index + 1 
                                                             && new string(x.grammer[0].lookahead.ToArray()).Equals(new string(grammer.lookahead.ToArray()))))
            {
                dfaStates.Add(new State(statesId, prevStateId, relation, new List<GrammerModel>() { new GrammerModel('n', new List<char>(){ 'n' }, 0, grammer.lookahead)}));
            }
            else 
            {
               // if not added first make a new grammer with new index and then create closure set if possible
                GrammerModel newGrammer = new GrammerModel(grammer.antecedent, grammer.consequent, grammer.index + 1, grammer.lookahead);

                if (newGrammer.index <= newGrammer.consequent.Count - 1)
                {
                    if (!teminalSet.Exists(x => x == newGrammer.consequent[newGrammer.index]))
                    {
                        clousreSet = closure(newGrammer.consequent[newGrammer.index], newGrammer.index, newGrammer.lookahead);
                        foreach (var item in clousreSet)
                        {
                            
                            if (item.consequent.Count - 1 > 1)
                            {
                                //beta is used to be able create loodaheads
                                beta = item.consequent.GetRange(1, item.consequent.Count - 1);
                                var contacted = beta.Concat(newGrammer.lookahead).ToList();
                                newLookahead = computeFirst(contacted, 0, item.consequent[1]).ToList();
                              //Console.WriteLine();
                            }
                            else 
                            {
                                newLookahead = newGrammer.lookahead;
                            }

                            var lst3 =  clousreSet.FindAll((x) => x.antecedent == item.consequent[0]);
                            foreach (var j in lst3)
                            {
                                int idx = clousreSet.IndexOf(j);
                                clousreSet[idx].lookahead = clousreSet[idx].lookahead.Concat(newLookahead).ToList().Distinct().ToList();
                            }

                        }
                    }
                }

                //generated grammer is our last grammer itms, we add them to stage mode for just one more
                //change and the enque it to states queue (mentioned in below comments)
                generatedGrammer.Add(newGrammer);
                generatedGrammer = generatedGrammer.Concat(clousreSet).ToList();

                var lst = dfaStageMode.FindAll((x) => x.prevStateId == prevStateId && x.relation == relation);
                if (lst.Any())
                {
                    foreach (var i in lst)
                    {
                        int idx = dfaStageMode.IndexOf(i);
                        dfaStageMode[idx].grammer = dfaStageMode[idx].grammer.Concat(generatedGrammer).ToList();
                    }
                    //dfaStegeMode.
                }
                else
                {
                    statesId += 1;
                    dfaStageMode.Add(new State(statesId, prevStateId, relation, generatedGrammer));
                }
            }


        }

        //DFA builder
        //DFA builder which iterates over states. for this purpose we've used  queue data structure which fills in goto function and frees in builder
        //
        public void builder() 
        {
            int cnt = 0;
            while (dfaTempStates.Count != 0) 
            {
                var st = dfaTempStates.Dequeue();
                Console.WriteLine();
                Console.WriteLine($"in state {st.id} with parent {st.prevStateId} and relation {st.relation}");
                foreach (var j in st.grammer)
                {
                    var cons = " ";
                    foreach (var z in j.consequent)
                    {
                        cons = cons + z;
                    }
                    var ts = "";
                    foreach (var t in j.lookahead)
                    {
                        ts += t;
                    }
                    Console.WriteLine($"{j.antecedent} -> {cons} with index {j.index} and lookahead {ts}");
                }
                Console.WriteLine();
                foreach (var item in st.grammer) 
                {
                    // Console.WriteLine($"in state {st.id}");
                    if (item.index != item.consequent.Count)
                        gotoFunction(item, st.id, item.consequent[item.index]);
                    else
                    { 
                        dfaStates.Add(new State(statesId, st.prevStateId, st.relation, new List<GrammerModel>() { item })); 

                    }
                }
                foreach (var i in dfaStageMode)
                {
                    dfaTempStates.Enqueue(i);
                    dfaStates.Add(i);
                }
                dfaStageMode.Clear();
                cnt ++;
                //Console.WriteLine("foreach finished");
            }
            Console.WriteLine();
        }

        //states
        //action table
        //goto table
        //other items to 
    }


    //state model that holds current state id and its parent id which are related to each other with relation property
    //it also has a list of grammer models representing each state's grammers
    class State 
    {
        public int id;
        public int prevStateId;
        public char relation;
        public List<GrammerModel> grammer;
        public State(int id, int prevStateId, char relation, List<GrammerModel> grammer)
        {
            this.id = id;
            this.prevStateId = prevStateId;
            this.relation = relation;
            this.grammer = grammer;
        }
    }

    //grammer model that has a antecedent and conseqent and also a index which show where we are in grammer and 
    //which sample should be expanded now, lookahead is a property for cls algorithm to avoid conflicts
    class GrammerModel
    {
        public char antecedent;
        public List<char> consequent;
        public int index;
        public List<char> lookahead;
        public GrammerModel(char antecedent, List<char> consequent, int index, List<char> lookahead)
        {
            this.antecedent = antecedent;
            this.consequent = consequent;
            this.index = index;
            this.lookahead = lookahead;
        }
    }
}

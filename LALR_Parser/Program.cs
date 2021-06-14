using System;
using System.Collections.Generic;

namespace LALR_Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = "id*id+id";
            string grammer = "S:A|B \nA:a \nB:b";
            List<GrammerModel> grammerSymbolsList = new List<GrammerModel>() 
            {
                    new GrammerModel('S', new List<char>(){'L','=','R'}, 0, new List<char>(){ '$'}),
                    new GrammerModel('S', new List<char>(){'R'},0,new List<char>(){ '$'}),
                    new GrammerModel('L', new List<char>(){'*','R'},0,new List<char>(){ '$', '='}),
                    new GrammerModel('L', new List<char>(){'i'},0,new List<char>(){ '$', '='}),
                    new GrammerModel('R', new List<char>(){'L'},0,new List<char>(){ '$'}),
            };
            DFA_builder dfa = new DFA_builder(grammerSymbolsList);
            dfa.builder();
            Console.WriteLine("Hello World!");
        }

        static void grammerConverter(string grammer) 
        {
            char[] arr = grammer.ToCharArray();
            List<Tuple<char, List<char>>> grammerSymbolsList = new List<Tuple<char, List<char>>>();
            char right = ' ';
            List<char> left = new List<char>();
            foreach (var i in arr)
            {
                if (i == '\n')
                {
                    grammerSymbolsList.Add(new Tuple<char, List<char>>(right, left));
                }
                else 
                {
                    
                }
                //if (i != '\n' && i != ' ') Console.WriteLine(i);
            }
               
        }
    }
}

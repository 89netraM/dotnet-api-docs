﻿//<snippet1>
// Sample for String.LastIndexOf(Char, Int32)
using System;

class Sample {
    public static void Main() {

    string br1 = "0----+----1----+----2----+----3----+----4----+----5----+----6----+-";
    string br2 = "0123456789012345678901234567890123456789012345678901234567890123456";
    string str = "Now is the time for all good men to come to the aid of their party.";
    int start;
    int at;

    start = str.Length-1;
    Console.WriteLine("All occurrences of 't' from position {0} to 0.", start);
    Console.WriteLine("{1}{0}{2}{0}{3}{0}", Environment.NewLine, br1, br2, str);
    Console.Write("The letter 't' occurs at position(s): ");

    at = 0;
    while((start > -1) && (at > -1))
        {
        at = str.LastIndexOf('t', start);
        if (at > -1)
            {
            Console.Write("{0} ", at);
            start = at - 1;
            }
        }
    Console.Write("{0}{0}{0}", Environment.NewLine);
    }
}
/*
This example produces the following results:
All occurrences of 't' from position 66 to 0.
0----+----1----+----2----+----3----+----4----+----5----+----6----+-
0123456789012345678901234567890123456789012345678901234567890123456
Now is the time for all good men to come to the aid of their party.

The letter 't' occurs at position(s): 64 55 44 41 33 11 7
*/
//</snippet1>
using System;
using pystruct;

Console.WriteLine(Struct.unpack("si", (Struct.pack("si", new object[] { "asdf", 10 })))[0]);
Console.WriteLine(Struct.calcsize("f3si"));
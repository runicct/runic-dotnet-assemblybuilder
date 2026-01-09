/*
 * MIT License
 * 
 * Copyright (c) 2026 Runic Compiler Toolkit Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runic.Dotnet
{
    public partial class AssemblyBuilder
    {
        internal class USStreamBuilder : Runic.Dotnet.Assembly.MetadataRoot.Stream
        {
            Runic.Dotnet.Assembly.Heap.StringHeap _heap = new Runic.Dotnet.Assembly.Heap.StringHeap(false, 0, 0);
            public Runic.Dotnet.Assembly.Heap.StringHeap Heap { get { return _heap; } }
            public override uint RelativeVirtualAddress { get { return _heap.RelativeVirtualAddress; } set { _heap.RelativeVirtualAddress = value; } }
            public override uint Size { get { return (((uint)_stringData.Count + 3) / 4) * 4; } set { } }
            Dictionary<string, uint> _strings = new Dictionary<string, uint>();
            List<byte> _stringData = new List<byte>();
            public Runic.Dotnet.Assembly.Heap.StringHeap.String AddString(string str)
            {
                uint existingIndex;
                if (_strings.TryGetValue(str, out existingIndex))
                {
                    return new Runic.Dotnet.Assembly.Heap.StringHeap.String(_heap, existingIndex);
                }
                byte[] bytes = Encoding.UTF8.GetBytes(str + "\0");
                uint index = (uint)_stringData.Count;
                _stringData.AddRange(bytes);
                _heap.Size = (uint)_stringData.Count;
                _strings.Add(str, index);
                return new Runic.Dotnet.Assembly.Heap.StringHeap.String(_heap, index);
            }
            public USStreamBuilder() : base(0, 0, "#US")
            {
                AddString("");
            }
            public void Save(System.IO.BinaryWriter writer)
            {
                writer.Write(_stringData.ToArray());
                uint padding = Size - (uint)_stringData.Count;
                for (uint n = 0; n < padding; n++) { writer.Write((byte)0); }
            }
        }
    }
}

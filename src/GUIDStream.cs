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
using System.Text;
using static Runic.Dotnet.Assembly;

namespace Runic.Dotnet
{
    public partial class AssemblyBuilder
    {
        public class GUIDStreamBuilder : Runic.Dotnet.Assembly.MetadataRoot.Stream
        {
            int count = 0;
            List<byte> _data = new List<byte>();
            Runic.Dotnet.Assembly.Heap.GUIDHeap _heap = new Runic.Dotnet.Assembly.Heap.GUIDHeap(false, 0, 0);
            public Runic.Dotnet.Assembly.Heap.GUIDHeap Heap { get { return _heap; } }
            public override uint RelativeVirtualAddress { get { return _heap.RelativeVirtualAddress; } set { _heap.RelativeVirtualAddress = value; } }
            public override uint Size { get { return (((uint)_data.Count + 3) / 4) * 4; } set { throw new Exception(); } }
            public GUIDStreamBuilder() : base(0, 0, "#GUID") { }
            public Heap.GUIDHeap.GUID AddGUID(Guid guid)
            {
                int index = count + 1;
                count++;
                _data.AddRange(guid.ToByteArray());
                return new Heap.GUIDHeap.GUID(_heap, (uint)index);
            }
            public void Save(System.IO.BinaryWriter writer)
            {
                writer.Write(_data.ToArray());
                uint padding = Size - (uint)_data.Count;
                for (uint n = 0; n < padding; n++) { writer.Write((byte)0); }
            }
        }
    }
}

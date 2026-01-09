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
using System.Threading.Tasks;

namespace Runic.Dotnet
{
    public partial class AssemblyBuilder
    {
        internal class BlobStreamBuilder : Runic.Dotnet.Assembly.MetadataRoot.Stream
        {
            List<byte> _blobData = new List<byte>();
            Runic.Dotnet.Assembly.Heap.BlobHeap _heap = new Runic.Dotnet.Assembly.Heap.BlobHeap(false, 0, 0);
            public Runic.Dotnet.Assembly.Heap.BlobHeap Heap { get { return _heap; } }
            public override uint RelativeVirtualAddress { get { return _heap.RelativeVirtualAddress; } set { _heap.RelativeVirtualAddress = value; } }
            public override uint Size { get { return (((uint)_blobData.Count + 3) / 4) * 4; } set { _heap.Size = value; } }
            public void WriteCompressedInteger(uint integer)
            {
                if (integer <= 127) { _blobData.Add((byte)integer); return; }
                if (integer <= 0x3FFF)
                {
                    _blobData.Add(((byte)((integer >> 8) | 0x80)));
                    _blobData.Add(((byte)((integer & 0xFF))));
                    return;
                }
                if (integer > 0x1FFFFFFF) { throw new ArgumentOutOfRangeException(); }
                _blobData.Add(((byte)((integer >> 24) | 0xFF)));
                _blobData.Add( ((byte)((integer >> 16) | 0xFF)));
                _blobData.Add(((byte)((integer >> 8) | 0xFF)));
                _blobData.Add(((byte)((integer) | 0xFF)));
            }
            public Runic.Dotnet.Assembly.Heap.BlobHeap.Blob AddBlob(byte[] blob)
            {
                if (blob.Length == 0) { return new Runic.Dotnet.Assembly.Heap.BlobHeap.Blob(_heap, 0); }
                uint index = (uint)_blobData.Count;
                WriteCompressedInteger((uint)blob.Length);
                _blobData.AddRange(blob);
                return new Runic.Dotnet.Assembly.Heap.BlobHeap.Blob(_heap, index);
            }
            public BlobStreamBuilder() : base(0, 0, "#Blob") 
            {
                _blobData.Add(0);
            }
            public void Save(System.IO.BinaryWriter writer)
            {
                writer.Write(_blobData.ToArray());
                uint padding = Size - (uint)_blobData.Count;
                for (uint n = 0; n < padding; n++) { writer.Write((byte)0); }
            }
        }
    }
}

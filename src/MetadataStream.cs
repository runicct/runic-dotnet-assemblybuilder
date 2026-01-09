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
using static Runic.Dotnet.Assembly;

namespace Runic.Dotnet
{
    public partial class AssemblyBuilder
    {
        internal class MetadataStreamBuilder : Runic.Dotnet.Assembly.MetadataRoot.Stream
        {
            Runic.Dotnet.Assembly.MetadataTable.MethodDefTable _methodDefTable = new Dotnet.Assembly.MetadataTable.MethodDefTable();
            public Runic.Dotnet.Assembly.MetadataTable.MethodDefTable MethodDefTable { get { return _methodDefTable; } }

            Runic.Dotnet.Assembly.MetadataTable.ParamTable _paramTable = new Dotnet.Assembly.MetadataTable.ParamTable();
            public Runic.Dotnet.Assembly.MetadataTable.ParamTable ParamTable { get { return _paramTable; } }
            Runic.Dotnet.Assembly.MetadataTable.AssemblyTable _assemblyTable = new Dotnet.Assembly.MetadataTable.AssemblyTable();
            public Runic.Dotnet.Assembly.MetadataTable.AssemblyTable AssemblyTable { get { return _assemblyTable; } }
            Dotnet.Assembly.MetadataTable.ModuleTable _moduleTable = new Dotnet.Assembly.MetadataTable.ModuleTable();
            public Dotnet.Assembly.MetadataTable.ModuleTable ModuleTable { get { return _moduleTable; } }
            Assembly.MetadataTable.TypeDefTable _typeDefTable = new Dotnet.Assembly.MetadataTable.TypeDefTable();
            public Assembly.MetadataTable.TypeDefTable TypeDefTable { get { return _typeDefTable; } }
            Assembly.MetadataTable.FieldTable _fieldTable = new Dotnet.Assembly.MetadataTable.FieldTable();
            public Assembly.MetadataTable.FieldTable FieldTable { get { return _fieldTable; } }
            Assembly.MetadataTable.AssemblyRefTable _assemblyRefTable = new Dotnet.Assembly.MetadataTable.AssemblyRefTable();
            public Assembly.MetadataTable.AssemblyRefTable AssemblyRefTable { get { return _assemblyRefTable; } }
            Assembly.MetadataTable.TypeRefTable _typeRefTable = new Dotnet.Assembly.MetadataTable.TypeRefTable();
            public Assembly.MetadataTable.TypeRefTable TypeRefTable { get { return _typeRefTable; } }
            Assembly.MetadataTable.MemberRefTable _memberRefTable = new Dotnet.Assembly.MetadataTable.MemberRefTable();
            public Assembly.MetadataTable.MemberRefTable MemberRefTable { get { return _memberRefTable; } }
            Assembly.MetadataTable.CustomAttributeTable _customAttributeTable = new Dotnet.Assembly.MetadataTable.CustomAttributeTable();
            public Assembly.MetadataTable.CustomAttributeTable CustomAttributeTable { get { return _customAttributeTable; } }
            uint _size;
            public override uint Size { get { return ((_size + 3) / 4) * 4; } set { _size = value; } }
            byte[] _data;
            public MetadataStreamBuilder() : base(0, 0, "#~") { }
            public void Finalize(Runic.Dotnet.Assembly.Heap.StringHeap stringHeap, Runic.Dotnet.Assembly.Heap.BlobHeap blobHeap, Runic.Dotnet.Assembly.Heap.GUIDHeap guidHeap)
            {
                using (System.IO.MemoryStream stream = new MemoryStream())
                {
                    using (System.IO.BinaryWriter tableWriter = new System.IO.BinaryWriter(stream))
                    {
                        Runic.Dotnet.Assembly.MetadataTable.Save(new MetadataTable[] { _assemblyRefTable, _fieldTable, _methodDefTable, _paramTable, _assemblyTable, _moduleTable, _typeDefTable, _typeRefTable, _memberRefTable, _customAttributeTable }, stringHeap, blobHeap, guidHeap, tableWriter);
                    }
                    _data = stream.ToArray();
                    _size = (uint)_data.Length;
                }
            }
            public void Save(System.IO.BinaryWriter writer)
            {
                writer.Write(_data);
                uint padding = Size - _size;
                for (uint n = 0; n < padding; n++) { writer.Write((byte)0); }
            }
        }
    }
}

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

using Runic.FileFormats;
using System;
using System.Collections.Generic;
using static Runic.Dotnet.Assembly;
using static Runic.Dotnet.AssemblyBuilder;
using static Runic.FileFormats.PortableExecutable.Directories;

namespace Runic.Dotnet
{
    public partial class AssemblyBuilder
    {
        BlobStreamBuilder _blobStream = new BlobStreamBuilder();
        GUIDStreamBuilder _GUIDStream = new GUIDStreamBuilder();
        StringsStreamBuilder _stringsStream = new StringsStreamBuilder();
        MetadataStreamBuilder _metadataStream = new MetadataStreamBuilder();
        USStreamBuilder _USStream = new USStreamBuilder();
        Dotnet.Assembly.MetadataRoot _metadataRoot;
        ulong _imageBase = 0x1000000;
        public ulong ImageBase { get { return _imageBase; } set { _imageBase = value; } }
        Version _imageVersion = new Version(1, 0, 0, 0);
        public Version ImageVersion { get { return _imageVersion; } set { _imageVersion = value; } }
        List<byte> _textData = new List<byte>();
        public AssemblyBuilder(string assemblyName, Version assemblyVersion, string assemblyCulture, string moduleName, Guid moduleMvid)
        {
            _metadataRoot = new Dotnet.Assembly.MetadataRoot(0, "v4.0.30319", new Dotnet.Assembly.MetadataRoot.Stream[] { _stringsStream, _GUIDStream, _blobStream, _metadataStream, _USStream });
            _metadataStream.AssemblyTable.Add(assemblyVersion, null, _stringsStream.AddString(assemblyName), _stringsStream.AddString(assemblyCulture));
            _metadataStream.ModuleTable.Add(_stringsStream.AddString(moduleName), _GUIDStream.AddGUID(moduleMvid));
            _metadataStream.TypeDefTable.Add(_stringsStream.AddString("<Module>"), _stringsStream.AddString(""), 0, null, null, null);
        }
        public Assembly.MetadataTable.ParamTable.ParamTableRow AddParameter(string parameterName, ParamAttributes parameterAttributes, int sequence)
        {
            return _metadataStream.ParamTable.Add(parameterAttributes, _stringsStream.AddString(parameterName), sequence);
        }
#if NET6_0_OR_GREATER
        public Assembly.MetadataTable.MethodDefTable.MethodDefTableRow AddMethod(string methodName, byte[] methodSignature, MethodAttributes methodAttributes, MethodImplAttributes methodImplAttributes, Assembly.MetadataTable.ParamTable.ParamTableRow? parameters)
#else
        public Assembly.MetadataTable.MethodDefTable.MethodDefTableRow AddMethod(string methodName, byte[] methodSignature, MethodAttributes methodAttributes, MethodImplAttributes methodImplAttributes, Assembly.MetadataTable.ParamTable.ParamTableRow parameters)
#endif
        {
            return _metadataStream.MethodDefTable.Add(_stringsStream.AddString(methodName), _blobStream.AddBlob(methodSignature), methodAttributes, methodImplAttributes, 0, parameters);
        }
        public void SetMethodBody(Assembly.MetadataTable.MethodDefTable.MethodDefTableRow method, byte[] headerAndBytecode)
        {
            uint rva = (uint)_textData.Count;
            _textData.AddRange(headerAndBytecode);
            method.MethodBodyRelativeVirtualAddress = rva;
        }
        public void SetMethodBody(Assembly.MetadataTable.MethodDefTable.MethodDefTableRow method, byte[] header, byte[] bytecode)
        {
            uint rva = (uint)_textData.Count;
            _textData.AddRange(header);
            _textData.AddRange(bytecode);
            method.MethodBodyRelativeVirtualAddress = rva;
        }
#if NET6_0_OR_GREATER
        public Assembly.MetadataTable.TypeDefTable.TypeDefTableRow AddType(string typeName, string namespaceName, TypeAttributes attributes, Assembly.MetadataTable.ITypeDefOrRefOrSpec parentType, Assembly.MetadataTable.FieldTable.FieldTableRow? fields, Assembly.MetadataTable.MethodDefTable.MethodDefTableRow? methods)
#else
        public Assembly.MetadataTable.TypeDefTable.TypeDefTableRow AddType(string typeName, string namespaceName, TypeAttributes attributes, Assembly.MetadataTable.ITypeDefOrRefOrSpec parentType, Assembly.MetadataTable.FieldTable.FieldTableRow fields, Assembly.MetadataTable.MethodDefTable.MethodDefTableRow methods)
#endif
        {
            return _metadataStream.TypeDefTable.Add(_stringsStream.AddString(typeName), _stringsStream.AddString(namespaceName), attributes, parentType, fields, methods);
        }
        public Assembly.MetadataTable.StandAloneSigTable.StandAloneSigTableRow AddStandAloneSignature(byte[] signature)
        {
            return _metadataStream.StandAloneSigTable.Add(_blobStream.AddBlob(signature));
        }
        public Assembly.MetadataTable.AssemblyRefTable.AssemblyRefTableRow AddAssemblyReference(string Name, Version version,  byte[] publicKeyOrToken, string culture)
        {
            return _metadataStream.AssemblyRefTable.Add(version,  _blobStream.AddBlob(publicKeyOrToken), _stringsStream.AddString(Name), _stringsStream.AddString(culture));
        }
        public Assembly.MetadataTable.TypeRefTable.TypeRefTableRow AddTypeReference(Assembly.MetadataTable.IResolutionScope resolutionScope, string name, string @namespace)
        {
            return _metadataStream.TypeRefTable.Add(resolutionScope, _stringsStream.AddString(name), _stringsStream.AddString(@namespace));
        }
        public Assembly.MetadataTable.MemberRefTable.MemberRefTableRow AddMemberReference(Assembly.MetadataTable.IMemberRefParent parent, string name, byte[] signature)
        {
            return _metadataStream.MemberRefTable.Add(parent, _stringsStream.AddString(name), _blobStream.AddBlob(signature));
        }
        public Assembly.MetadataTable.FieldTable.FieldTableRow AddField(FieldAttributes attributes, string fieldName, byte[] fieldSignature)
        {
            return _metadataStream.FieldTable.Add(attributes, _stringsStream.AddString(fieldName), _blobStream.AddBlob(fieldSignature));
        }
        public Assembly.MetadataTable.CustomAttributeTable.CustomAttributeTableRow AddCustomAttribute(Assembly.MetadataTable.IHasCustomAttribute parent, Assembly.MetadataTable.ICustomAttributeConstructor constructor, byte[] value)
        {
            return _metadataStream.CustomAttributeTable.Add(parent, constructor, _blobStream.AddBlob(value));
        }
        public Heap.StringHeap.String AddUserString(string value) { return _USStream.AddString(value); }
        public Heap.StringHeap.String AddString(string value) { return _stringsStream.AddString(value); }
        public Heap.GUIDHeap.GUID AddGUID(Guid guid) { return _GUIDStream.AddGUID(guid); }
        public Heap.BlobHeap.Blob AddBlob(byte[] data) { return _blobStream.AddBlob(data); }    
        public void Save(System.IO.BinaryWriter writer)
        {
            TextSection textSection = new TextSection();
            textSection.RelativeVirtualAddress = 0x1000;
            for (uint n = 0; n < _metadataStream.MethodDefTable.Rows; n++)
            {
                _metadataStream.MethodDefTable[n + 1].MethodBodyRelativeVirtualAddress += textSection.RelativeVirtualAddress;
            }
            int alignedTextDataSize = ((_textData.Count + 7) / 8) * 8;

            _metadataRoot.RelativeVirtualAddress = textSection.RelativeVirtualAddress + (uint)alignedTextDataSize;
            _blobStream.RelativeVirtualAddress = _metadataRoot.RelativeVirtualAddress + _metadataRoot.Size;
            _stringsStream.RelativeVirtualAddress = _blobStream.RelativeVirtualAddress + _blobStream.Size;
            _GUIDStream.RelativeVirtualAddress = _stringsStream.RelativeVirtualAddress + _stringsStream.Size;
            _USStream.RelativeVirtualAddress = _GUIDStream.RelativeVirtualAddress + _GUIDStream.Size;
            _metadataStream.RelativeVirtualAddress = _USStream.RelativeVirtualAddress + _USStream.Size;

            _metadataStream.Finalize(_stringsStream.Heap, _blobStream.Heap, _GUIDStream.Heap);
            uint totalSize = _metadataStream.RelativeVirtualAddress + _metadataStream.Size - _metadataRoot.RelativeVirtualAddress;
            using (System.IO.MemoryStream textSectionStream = new System.IO.MemoryStream())
            {
                using (System.IO.BinaryWriter testSectionWriter = new System.IO.BinaryWriter(textSectionStream))
                {
                    testSectionWriter.Write(_textData.ToArray());
                    for (int n = 0; n < alignedTextDataSize - _textData.Count; n++) { testSectionWriter.Write((byte)0); }
                    _metadataRoot.Save(testSectionWriter);
                    _blobStream.Save(testSectionWriter);
                    _stringsStream.Save(testSectionWriter);
                    _GUIDStream.Save(testSectionWriter);
                    _USStream.Save(testSectionWriter);
                    _metadataStream.Save(testSectionWriter);
                    PortableExecutable.Directories.CLIHeader cliHeader = new PortableExecutable.Directories.CLIHeader(new Version(2, 0), _metadataRoot.RelativeVirtualAddress, totalSize, PortableExecutable.Directories.CLIHeader.CorFlags.ILOnly, 0, 0, 0, 0, 0, 0, 0);
                    cliHeader.Save(testSectionWriter);
                }
                textSection.SetData(textSectionStream.ToArray());
            }

            uint textSectionSize = textSection.Size;
            textSectionSize = (textSectionSize + 4096 - 1) / 4096 * 4096;


            uint cliHeaderRVA = _metadataStream.RelativeVirtualAddress + _metadataStream.Size;
        
            Runic.FileFormats.PortableExecutable portableExecutable = new Runic.FileFormats.PortableExecutable();
            portableExecutable.IsPE32Plus = false;
            portableExecutable.Machine = FileFormats.PortableExecutable.MachineType.i386;
            portableExecutable.Sections = new FileFormats.PortableExecutable.Section[] { textSection };
            portableExecutable.CLIHeader = new PortableExecutable.Directory(cliHeaderRVA, 72);
            portableExecutable.Characteristics = PortableExecutable.DllCharacteristics.NoSeh | PortableExecutable.DllCharacteristics.TerminalServerAware;
            portableExecutable.ImageFlags = PortableExecutable.CoffImageFlags.ExecutableImage | PortableExecutable.CoffImageFlags.Machine32Bits | PortableExecutable.CoffImageFlags.Dll;
            portableExecutable.LinkerVersion = new Version(11, 0, 0, 0);
            portableExecutable.SectionAlignment = 0x1000;
            portableExecutable.FileAlignment = 0x200;
            portableExecutable.ImageBase = _imageBase;
            portableExecutable.ImageVersion = _imageVersion;
            portableExecutable.Save(writer);
        }
    }
}
 
﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.NRefactory;
using dnlib.DotNet;

namespace ICSharpCode.ILSpy
{
	/// <summary>
	/// IL language support.
	/// </summary>
	/// <remarks>
	/// Currently comes in two versions:
	/// flat IL (detectControlStructure=false) and structured IL (detectControlStructure=true).
	/// </remarks>
	public class ILLanguage : Language
	{
		private readonly bool detectControlStructure;
		
		public ILLanguage(bool detectControlStructure)
		{
			this.detectControlStructure = detectControlStructure;
		}
		
		public override string Name {
			get { return "IL"; }
		}
		
		public override string FileExtension {
			get { return ".il"; }
		}
		
		public override void DecompileMethod(MethodDef method, ITextOutput output, DecompilationOptions options)
		{
			var dis = new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken);
			dis.DisassembleMethod(method);
		}
		
		public override void DecompileField(FieldDef field, ITextOutput output, DecompilationOptions options)
		{
			var dis = new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken);
			dis.DisassembleField(field);
		}
		
		public override void DecompileProperty(PropertyDef property, ITextOutput output, DecompilationOptions options)
		{
			ReflectionDisassembler rd = new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken);
			rd.DisassembleProperty(property);
			if (property.GetMethod != null) {
				output.WriteLine();
				rd.DisassembleMethod(property.GetMethod);
			}
			if (property.SetMethod != null) {
				output.WriteLine();
				rd.DisassembleMethod(property.SetMethod);
			}
			foreach (var m in property.OtherMethods) {
				output.WriteLine();
				rd.DisassembleMethod(m);
			}
		}
		
		public override void DecompileEvent(EventDef ev, ITextOutput output, DecompilationOptions options)
		{
			ReflectionDisassembler rd = new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken);
			rd.DisassembleEvent(ev);
			if (ev.AddMethod != null) {
				output.WriteLine();
				rd.DisassembleMethod(ev.AddMethod);
			}
			if (ev.RemoveMethod != null) {
				output.WriteLine();
				rd.DisassembleMethod(ev.RemoveMethod);
			}
			foreach (var m in ev.OtherMethods) {
				output.WriteLine();
				rd.DisassembleMethod(m);
			}
		}
		
		public override void DecompileType(TypeDef type, ITextOutput output, DecompilationOptions options)
		{
			var dis = new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken);
			dis.DisassembleType(type);
		}
		
		public override void DecompileNamespace(string nameSpace, IEnumerable<TypeDef> types, ITextOutput output, DecompilationOptions options)
		{
			new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken).DisassembleNamespace(nameSpace, types);
		}
		
		public override void DecompileAssembly(LoadedAssembly assembly, ITextOutput output, DecompilationOptions options)
		{
			output.WriteLine("// " + assembly.FileName, TextTokenType.Comment);
			output.WriteLine();
			
			ReflectionDisassembler rd = new ReflectionDisassembler(output, detectControlStructure, options.CancellationToken);
			if (options.FullDecompilation)
				rd.WriteAssemblyReferences(assembly.ModuleDefinition as ModuleDefMD);
			if (assembly.AssemblyDefinition != null)
				rd.WriteAssemblyHeader(assembly.AssemblyDefinition);
			output.WriteLine();
			rd.WriteModuleHeader(assembly.ModuleDefinition);
			if (options.FullDecompilation) {
				output.WriteLine();
				output.WriteLine();
				rd.WriteModuleContents(assembly.ModuleDefinition);
			}
		}
		
		public override string TypeToString(ITypeDefOrRef t, bool includeNamespace, IHasCustomAttribute attributeProvider = null)
		{
			PlainTextOutput output = new PlainTextOutput();
			t.WriteTo(output, includeNamespace ? ILNameSyntax.TypeName : ILNameSyntax.ShortTypeName);
			return output.ToString();
		}
	}
}

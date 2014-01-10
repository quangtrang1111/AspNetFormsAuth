﻿/*
    Copyright (C) 2013 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;

namespace Rhetos.Utilities
{
    internal class GenericDataContractResolver : DataContractResolver
    {
        private static string Decode(string value)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
                if (value[i] == '_')
                {
                    int code = int.Parse(value.Substring(i + 1, 4), System.Globalization.NumberStyles.HexNumber);
                    sb.Append((char)code);
                    i += 4;
                }
                else
                    sb.Append(value[i]);

            return sb.ToString();
        }

        private static string Encode(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
                if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '.')
                    sb.Append(c);
                else
                    sb.AppendFormat("_{0:x4}", (int)c);
            return sb.ToString();
        }

        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
        {
            var decodedTypeName = Decode(typeName);

            Type type = Type.GetType(decodedTypeName + ", " + typeNamespace);
            if (type != null)
                return type;

            if (XmlUtility.Dom != null)
            {
                // Without explicit use of XmlUtility.Dom, there is a possibility that ServerDom.dll will not get in AppDomain.CurrentDomain.GetAssemblies(), so ResolveName will fail to resolve the type. Unexpectedly, the problem occured stochastically when the server was under heavy load.
                type = XmlUtility.Dom.GetType(decodedTypeName);
                if (type != null)
                    return type;
            }

            type = Type.GetType(decodedTypeName);
            if (type != null)
                return type;

            type = 
                (from asm in AppDomain.CurrentDomain.GetAssemblies()
                 where asm.FullName.StartsWith(typeNamespace + ",")
                 let asmType = asm.GetType(decodedTypeName)
                 where asmType != null
                 select asmType).FirstOrDefault();
            if (type != null)
                return type;

            return ResolveRuntimeType(decodedTypeName);
        }

        private readonly Dictionary<string, Type> _runtimeTypesCache = new Dictionary<string, Type>();

        private Type ResolveRuntimeType(string typeName)
        {
            if (_runtimeTypesCache.ContainsKey(typeName))
                return _runtimeTypesCache[typeName];

            lock (_runtimeTypesCache)
            {
                if (_runtimeTypesCache.ContainsKey(typeName))
                    return _runtimeTypesCache[typeName];

                var asms = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                            where !asm.FullName.StartsWith("System.") && !asm.FullName.StartsWith("Microsoft.")
                            select asm);

                foreach (Assembly assembly in asms)
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.FullName == typeName)
                        {
                            _runtimeTypesCache.Add(typeName, type);
                            return type;
                        }
                    }
                }
            }

            return null;
        }

//        private Assembly _objectModelAssembly;
//
//        private Assembly ObjectModelAssembly
//        {
//            get
//            {
//                if (null == _objectModelAssembly)
//                    _objectModelAssembly = FindObjectModelAssembly();
//
//                return _objectModelAssembly;
//            }
//        }
//
//        private static Assembly FindObjectModelAssembly()
//        {
//            var asms = (from asm in AppDomain.CurrentDomain.GetAssemblies()
//                        where asm.FullName.StartsWith("Rhetos.ObjectModel,")
//                        select asm).ToList();
//
//            if (asms.Count > 1)
//                throw new FrameworkException("There are multiple assemblies named Rhetos.ObjectModel.");
//            if (asms.Count == 0)
//                throw new FrameworkException("Rhetos.ObjectModel assembly not found.");
//
//            return asms[0];
//        }

        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            XmlDictionary dictionary = new XmlDictionary();
            typeName = dictionary.Add(Encode(type.FullName));
            typeNamespace = dictionary.Add(type.Namespace);
            return true;
        }
    }
}
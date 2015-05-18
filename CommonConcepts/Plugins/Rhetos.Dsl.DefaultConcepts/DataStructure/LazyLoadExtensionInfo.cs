﻿/*
    Copyright (C) 2014 Omega software d.o.o.

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
using System.Linq;
using System.Text;
using Rhetos.Dsl;
using System.ComponentModel.Composition;

namespace Rhetos.Dsl.DefaultConcepts
{
    /// <summary>
    /// Enables lazy loading of the navigation property.
    /// </summary>
    [Export(typeof(IConceptInfo))]
    [ConceptKeyword("LazyLoadExtension")]
    public class LazyLoadExtensionInfo : IConceptInfo, IMacroConcept, IValidatedConcept
    {
        [ConceptKey]
        public DataStructureExtendsInfo Extends { get; set; }

        public IEnumerable<IConceptInfo> CreateNewConcepts(IEnumerable<IConceptInfo> existingConcepts)
        {
            return new[] { new LazyLoadSupportInfo { DataStructure = Extends.Base } };
        }

        public void CheckSemantics(IDslModel existingConcepts)
        {
            if (!DslUtility.IsQueryable(Extends.Base))
                throw new DslSyntaxException(this, this.GetKeywordOrTypeName() + " can only be used on a queryable base data structure, such as Entity. " + Extends.Base.GetKeywordOrTypeName() + " is not queryable.");

            if (!DslUtility.IsQueryable(Extends.Extension))
                throw new DslSyntaxException(this, this.GetKeywordOrTypeName() + " can only be used on an extension that is a queryable data structure, such as Entity. " + Extends.Extension.GetKeywordOrTypeName() + " is not queryable.");
        }
    }
}
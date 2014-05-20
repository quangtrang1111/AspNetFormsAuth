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
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Rhetos.Dsl.DefaultConcepts
{
    [Export(typeof(IConceptInfo))]
    [ConceptKeyword("ComputedFrom")]
    public class EntityComputedFromInfo : IConceptInfo, IMacroConcept
    {
        [ConceptKey]
        public EntityInfo Target { get; set; }

        [ConceptKey]
        public DataStructureInfo Source { get; set; }

        public static string RecomputeFunctionName(EntityComputedFromInfo info)
        {
            return "RecomputeFrom" + DslUtility.NameOptionalModule(info.Source, info.Target.Module);
        }

        public IEnumerable<IConceptInfo> CreateNewConcepts(IEnumerable<IConceptInfo> existingConcepts)
        {
            if (!existingConcepts.OfType<KeyPropertyComputedFromInfo>().Any(kp => kp.PropertyComputedFrom.Dependency_EntityComputedFrom == this)
                && !existingConcepts.OfType<KeyPropertyIDComputedFromInfo>().Any(kp => kp.EntityComputedFrom == this)
                && !existingConcepts.OfType<PersistedKeyPropertiesInfo>().Any(kp => kp.Persisted == Target && kp.Persisted.Source == Source))
                return new[] { new KeyPropertyIDComputedFromInfo { EntityComputedFrom = this } };
            return null;
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Reflection;
using IntelliTect.Coalesce.Utilities;

namespace IntelliTect.Coalesce.TypeDefinition
{
    internal class ReflectionParameterViewModel : ParameterViewModel
    {
        protected internal ParameterInfo Info { get; internal set; }

        public ReflectionParameterViewModel(MethodViewModel parent, ParameterInfo info) 
            : base(parent, ReflectionTypeViewModel.GetOrCreate(
                parent.Parent.ReflectionRepository,
                info.ParameterType.IsByRef
                    ? info.ParameterType.GetElementType()!
                    : info.ParameterType
            ))
        {
            Info = info;

        }

        public override string Name => Info.Name ?? throw new Exception("Parameter has no name???");

        public override bool HasDefaultValue => Info.HasDefaultValue;

        protected override object? RawDefaultValue => Info.RawDefaultValue;

        public override object? GetAttributeValue<TAttribute>(string valueName)
        {
            return Info.GetAttributeValue<TAttribute>(valueName);
        }

        public override bool HasAttribute<TAttribute>()
        {
            return Info.HasAttribute<TAttribute>();
        }
    }
}

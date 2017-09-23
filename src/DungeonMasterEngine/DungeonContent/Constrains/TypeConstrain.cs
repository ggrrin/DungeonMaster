﻿using System;
using DungeonMasterEngine.Interfaces;

namespace DungeonMasterEngine.DungeonContent.Constrains
{
    class TypeConstrain : IConstrain
    {
        public Type AcceptableType { get; }

        public bool AccepChild { get; }

        public TypeConstrain(Type acceptableType, bool acceptChild = true)
        {
            AcceptableType = acceptableType;
            AccepChild = acceptChild;
        }

        public bool IsAcceptable(object item)
        {
            if (item == null)
                return true;

            if (AccepChild)
                return AcceptableType.IsInstanceOfType(item); 
            else
                return item.GetType() == AcceptableType;
        }

        public override string ToString()
        {
            return $"T:{AcceptableType.Name}\r\n->child:{AccepChild}";
        }
    }
}

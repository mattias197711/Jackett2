﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Jackett2.Core
{
    public static class XElementExtensions
    {
        public static string AttributeString(this XElement element, string name)
        {
            var attr = element.Attribute(name);
            if (attr == null)
                return string.Empty;
            return attr.Value;
        }

        public static List<string> AttributeStringList(this XElement element, string name)
        {
            var value = element.AttributeString(name);
            return value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}

//
// The MIT License (MIT)
// Copyright (c) 2016 Adriano Repetti

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// - The above copyright notice and this permission notice shall be
//   included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
// USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Mustache;

namespace MicroBench.Engine.Renderer
{
    /// <summary>
    /// Definition for mustache-sharp of a new custom tag {{#join}}.
    /// </summary>
    /// <remarks>
    /// It represents a new {{#join collection selector}} tag used to create a new string concatenating items of "collection"
    /// (separated by comma). For each element value is calculated getting value of a property using "selector" (dot separated
    /// list of properties), if omitted item itself is used. Conversion to string is made using invariant culture and string
    /// escaping is performed according to JavaScript rules.
    /// </remarks>
    sealed class MustacheJoinTagDefinition : InlineTagDefinition
    {
        public MustacheJoinTagDefinition()
            : base("join")
        {
        }

        protected override IEnumerable<TagParameter> GetParameters()
        {
            return new TagParameter[] { new TagParameter("collection"), new TagParameter("selector") { IsRequired = false } };
        }

        public override void GetText(TextWriter writer, Dictionary<string, object> arguments, Scope context)
        {
            var collection = (System.Collections.IEnumerable)arguments["collection"];
            var selector = arguments.ContainsKey("selector") ? (string)arguments["selector"] : "";

            writer.Write(String.Join(", ", collection.Cast<object>().Select(o => ToJavaScriptToken(o, selector))));
        }

        private static string ToJavaScriptToken(object obj, string selector)
        {
            if (obj == null)
                return "";

            return ToJavaScriptToken(GetPropertyValue(obj, selector));
        }

        private static string ToJavaScriptToken(object value)
        {
            if (value == null)
                return "";

            return HttpUtility.JavaScriptStringEncode(
                Convert.ToString(value, CultureInfo.InvariantCulture),
                value is string);
        }

        private static object GetPropertyValue(object obj, string selector)
        {
            Debug.Assert(obj != null);

            if (String.IsNullOrWhiteSpace(selector))
                return obj;

            if (selector.Contains("."))
            {
                int separatorIndex = selector.IndexOf(".");
                string propertyName = selector.Substring(0, separatorIndex);

                return GetPropertyValue(GetPropertyValueCore(obj, propertyName),
                    selector.Substring(separatorIndex + 1));
            }

            return GetPropertyValueCore(obj, selector);
        }

        private static object GetPropertyValueCore(object obj, string propertyName)
        {
            Debug.Assert(obj != null);
            Debug.Assert(!String.IsNullOrWhiteSpace(propertyName));

            var property = obj.GetType().GetProperty(propertyName);
            if (property == null)
                throw new ArgumentException(String.Format("Cannot find property {0} on object {1}", propertyName, obj.GetType()));

            return property.GetValue(obj);
        }
    }
}

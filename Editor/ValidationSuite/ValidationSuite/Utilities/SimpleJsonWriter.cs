using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    static class SimpleJsonWriter
    {
        public static void Indent(StringBuilder sb, int indent)
        {
            sb.Append(' ', 2 * indent);
        }

        public static void Eol(StringBuilder sb, bool emitComma)
        {
            sb.Append(emitComma ? ",\n" : "\n");
        }

        /// <summary>
        /// Emit a "generic" JSON value as read by SimpleJsonReader.
        /// (Obviously less performant than the specific emitters.)
        /// </summary>
        public static void EmitGeneric(StringBuilder sb, string name, object val, int indent, bool emitComma = true)
        {
            if (val == null)
                EmitNamedValue(sb, name, null, indent, emitComma);
            else if (val is bool b)
                EmitNamedValue(sb, name, b, indent, emitComma);
            else if (val is double d)
                EmitNamedValue(sb, name, d, indent, emitComma);
            else if (val is int i)
                EmitNamedValue(sb, name, i, indent, emitComma);
            else if (val is string s)
                EmitNamedValue(sb, name, s, indent, emitComma);
            else if (val is Dictionary<string, object> dict)
            {
                EmitName(sb, name, indent);
                sb.Append('{');
                EmitGenericItems(sb, dict.OrderBy(kv => kv.Key, StringComparer.Ordinal), indent);
                sb.Append('}');
                Eol(sb, emitComma);
            }
            else if (val is IEnumerable<object> list)
            {
                EmitName(sb, name, indent);
                sb.Append('[');
                EmitGenericItems(sb, list.Select(item => new KeyValuePair<string, object>(null, item)), indent);
                sb.Append(']');
                Eol(sb, emitComma);
            }
            else throw new ArgumentException($"Cannot {nameof(EmitGeneric)} value of type {val.GetType()}: '{val}'");
        }

        static void EmitGenericItems(StringBuilder sb, IEnumerable<KeyValuePair<string, object>> items, int indent)
        {
            var anyElements = false;
            foreach (var item in items)
            {
                if (!anyElements)
                {
                    anyElements = true;
                    sb.Append("\n");
                }
                EmitGeneric(sb, item.Key, item.Value, indent + 1);
            }
            if (!anyElements) return; // for an empty sequence, emit nothing at all

            sb.Length = sb.Length - 2; // remove last ",\n"
            sb.Append("\n");
            Indent(sb, indent);
        }

        public static void EmitNamedValue(StringBuilder sb, string name, string val, int indent, bool emitComma = true)
        {
            EmitName(sb, name, indent);
            EmitValue(sb, val);
            Eol(sb, emitComma);
        }

        public static void EmitNamedValue(StringBuilder sb, string name, double val, int indent, bool emitComma = true)
        {
            EmitName(sb, name, indent);
            EmitValue(sb, val);
            Eol(sb, emitComma);
        }

        public static void EmitNamedValue(StringBuilder sb, string name, int val, int indent, bool emitComma = true)
        {
            EmitName(sb, name, indent);
            EmitValue(sb, val);
            Eol(sb, emitComma);
        }

        public static void EmitNamedValue(StringBuilder sb, string name, bool val, int indent, bool emitComma = true)
        {
            EmitName(sb, name, indent);
            sb.Append(val ? "true" : "false");
            Eol(sb, emitComma);
        }

        public static void EmitValue(StringBuilder sb, string str)
        {
            if (str == null)
            {
                sb.Append("null");
                return;
            }
            sb.Append('"');
            for (var i = 0; i < str.Length; ++i)
            {
                var c = str[i];
                if (c < ' ' || c == '"' || c == '\\')
                {
                    sb.Append('\\');
                    var j = "\"\\\n\r\t\b\f".IndexOf(c);
                    if (j >= 0)
                        sb.Append("\"\\nrtbf"[j]);
                    else
                        sb.AppendFormat("u{0:X4}", (uint)c);
                }
                else
                    sb.Append(c);
            }
            sb.Append('"');
        }

        public static void EmitValue(StringBuilder sb, double d)
        {
            sb.Append(d.ToString(CultureInfo.InvariantCulture));
        }

        public static void EmitValue(StringBuilder sb, int i)
        {
            sb.Append(i.ToString(CultureInfo.InvariantCulture));
        }

        public static void EmitArray(StringBuilder sb, string name, int[] arr, int indent, bool emitComma = true)
        {
            if (arr == null)
                return;
            if (!ArrayStart(sb, name, arr.Any(), indent, emitComma))
                return;
            for (var i = 0; i < arr.Length; i++)
            {
                Indent(sb, indent + 1);
                EmitValue(sb, arr[i]);
                Eol(sb, i != arr.Length - 1);
            }
            ArrayEnd(sb, indent, emitComma);
        }

        public static void EmitArray<T>(StringBuilder sb, string name, T[] arr, int indent, bool emitComma = true)
        {
            if (arr == null)
                return;
            if (!ArrayStart(sb, name, arr.Any(), indent, emitComma))
                return;
            for (var i = 0; i < arr.Length; i++)
            {
                Indent(sb, indent + 1);
                EmitValue(sb, arr[i].ToString());
                Eol(sb, i != arr.Length - 1);
            }
            ArrayEnd(sb, indent, emitComma);
        }

        public static void EmitObjects<T>(StringBuilder sb, string name, List<T> arr, Action<StringBuilder, T, int> emit, int indent, bool emitComma = true)
        {
            if (arr == null)
                return;
            if (!ArrayStart(sb, name, arr.Any(), indent, emitComma))
                return;
            for (var i = 0; i < arr.Count; i++)
            {
                Indent(sb, indent + 1); sb.Append("{\n");
                emit(sb, arr[i], indent + 2);
                Indent(sb, indent + 1); sb.Append("}");
                Eol(sb, i != arr.Count - 1);
            }
            ArrayEnd(sb, indent, emitComma);
        }

        static bool ArrayStart(StringBuilder sb, string name, bool hasContents, int indent, bool emitComma)
        {
            EmitName(sb, name, indent);
            if (!hasContents)
            {
                sb.Append("[]");
                Eol(sb, emitComma);
                return false;
            }
            sb.Append("[\n");
            return true;
        }

        static void ArrayEnd(StringBuilder sb, int indent, bool emitComma)
        {
            Indent(sb, indent);
            sb.Append(']');
            Eol(sb, emitComma);
        }

        public static void EmitName(StringBuilder sb, string name, int indent)
        {
            Indent(sb, indent);
            if (name != null)
            {
                sb.Append('"');
                sb.Append(name);
                sb.Append("\": ");
            }
        }
    }
}

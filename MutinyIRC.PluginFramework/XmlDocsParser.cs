namespace MutinyIRC.PluginFramework
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using System.Text;
    using System.Reflection;

    public class XmlDocsParser
    {
        private XDocument xdoc;

        public XmlDocsParser(string path)
        {
            xdoc = XDocument.Load(path);
        }

        /// <summary>
        /// Gets the &lt;summary&gt; text from the XML docs for the given method.
        /// </summary>
        /// <returns>The trimmed summary, or null if none is present.</returns>
        public string GetMethodSummary(Type type, MethodInfo info)
        {
            string name = MemberNameString(type, info);
            var summary = xdoc.Descendants("member")
                .Where(m => m.Attribute("name") != null && m.Attribute("name").Value == name)
                .Select(m => m.Element("summary"))
                .FirstOrDefault();

            return summary == null ? null : summary.Value.Trim();
        }

        /// <summary>
        /// Gets the &lt;summary&gt; text from the XML docs for the given type.
        /// </summary>
        /// <returns>The trimmed summary, or null if none is present.</returns>
        public string GetTypeSummary(Type type)
        {
            string name = "T:" + type.FullName;
            var summary = xdoc.Descendants("member")
                .Where(m => m.Attribute("name") != null && m.Attribute("name").Value == name)
                .Select(m => m.Element("summary"))
                .FirstOrDefault();

            return summary == null ? null : summary.Value.Trim();
        }

        /// <summary>
        /// Builds a name attribute as used in xml docs for comparison
        /// </summary>
        /// <param name="type">The Type that the method belongs to</param>
        /// <param name="method"></param>
        /// <returns>A name attribute string</returns>
        /// <example>"M:MutinyIRC.Commands.Join.Execute(MutinyIRC.Common.Channel,MutinyIRC.Common.ChannelInfo)"</example>
        public static string MemberNameString(Type type, MemberInfo member)
        {
            StringBuilder sb = new StringBuilder();

            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                    sb.Append("M:");
                    break;
                case MemberTypes.Event:
                    sb.Append("E:");
                    break;
                case MemberTypes.Field:
                    sb.Append("F:");
                    break;
                case MemberTypes.Method:
                    sb.Append("M:");
                    break;
                case MemberTypes.Property:
                    sb.Append("P:");
                    break;
                case MemberTypes.TypeInfo:
                    sb.Append("T:");
                    break;
            }

            if (member.MemberType != MemberTypes.TypeInfo)
            {
                sb.Append(type.ToString());
                sb.Append("." + member.Name);
            }

            if (member.MemberType == MemberTypes.Constructor)
                sb.Append(".#ctor");

            if (member.MemberType is MemberTypes.Constructor or MemberTypes.Method)
            {
                sb.Append("(");

                ParameterInfo[] p = ((MethodInfo)member).GetParameters();
                for (int i = 0; i < p.Length; i++)
                {
                    sb.Append(p[i].ParameterType.ToString());
                    if (i < p.Length - 1)
                    {
                        sb.Append(",");
                    }
                }

                sb.Append(")");
            }

            return sb.ToString();
        }
    }
}

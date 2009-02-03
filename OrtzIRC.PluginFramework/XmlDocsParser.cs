namespace OrtzIRC.PluginFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Text;
    using System.Reflection;

    public class XmlDocsParser
    {
        private XDocument xdoc;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public XmlDocsParser(string path)
        {
            xdoc = XDocument.Load(path);
        }

        public string GetMethodSummary(Type type, MethodInfo info)
        {
            //TODO: clean this up
            IEnumerable<string> summaries = from member in xdoc.Descendants("member")
                                            where member.Attribute("name") != null
                                            && member.Attribute("name").Value == XmlDocsParser.MemberNameString(type, info)
                                            select member.Element("summary").Value.Trim();

            List<string> l = new List<string>(summaries);

            if (l.Count > 1)
                throw new Exception(); //this should never happen

            if (l.Count == 0)
                return null;

            return l[0];
        }

        /// <summary>
        /// Builds a name attribute as used in xml docs for comparison
        /// </summary>
        /// <param name="type">The Type that the method belongs to</param>
        /// <param name="method"></param>
        /// <returns>A name attribute string</returns>
        /// <example>"M:OrtzIRC.Commands.Join.Execute(OrtzIRC.Common.Channel,OrtzIRC.Common.ChannelInfo)"</example>
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

            if (member.MemberType == MemberTypes.Constructor || member.MemberType == MemberTypes.Method)
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

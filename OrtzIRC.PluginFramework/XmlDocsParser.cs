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
            xdoc = new XDocument(path);
        }

        public string GetMethodSummary(string methodName)
        {

        }
    }
}

namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    using System;

    public static class XmlNodeFactory
    {
        public static XmlNode CreateXmlNode(Xml.XmlNode data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            switch (data)
            {
                case Xml.XmlDocument dataDocument:
                    return new XmlDocument(dataDocument);

                case Xml.XmlElement dataElement:
                    return new XmlElement(dataElement);

                case Xml.XmlText dataText:
                    return new XmlText(dataText);

                case Xml.XmlComment dataComment:
                    return new XmlComment(dataComment);

                case Xml.XmlCDATA dataCData:
                    return new XmlCDATA(dataCData);

                case Xml.XmlSpecial dataSpecial:
                    return new XmlSpecial(dataSpecial);

                case Xml.XmlDeclaration dataXmlDecl:
                    return new XmlDeclaration(dataXmlDecl);

                default:
                    throw new InvalidOperationException("Unknown node type: " + data.GetType());
            }
        }
    }
}
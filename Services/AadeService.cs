using System.Net;
using System.Xml.Linq;
using System.Xml;
using DataNex.Model.Dtos;

namespace DataNexApi
{
    public class AadeService
    {

        public static AadeCompanyDto GetDataFromAade(string username, string password, string afmCalledBy, string afmCalledFor)
        {
            HttpWebRequest request = CreateWebRequest();
            XmlDocument soapEnvelopeXml = new XmlDocument();
            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                <env:Envelope
                xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/""
                xmlns:ns=""http://gr/gsis/rgwspublic/RgWsPublic.wsdl""
                xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                xmlns:ns1=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-
                1.0.xsd"">
                <env:Header>
                <ns1:Security>
                <ns1:UsernameToken>
                <ns1:Username>$username$</ns1:Username>
                <ns1:Password>$password$</ns1:Password>
                </ns1:UsernameToken>
                </ns1:Security>
                </env:Header>
                <env:Body>
                <ns:rgWsPublicAfmMethod>
                <RgWsPublicInputRt_in xsi:type=""ns:RgWsPublicInputRtUser"">
                <ns:afmCalledBy>$afmCalledBy$</ns:afmCalledBy>
                <ns:afmCalledFor>$afmCalledFor$</ns:afmCalledFor>
                </RgWsPublicInputRt_in>
                <RgWsPublicBasicRt_out xsi:type=""ns:RgWsPublicBasicRtUser"">
                <ns:afm xsi:nil=""true""/>
                <ns:stopDate xsi:nil=""true""/>
                <ns:postalAddressNo xsi:nil=""true""/>
                <ns:doyDescr xsi:nil=""true""/>
                <ns:doy xsi:nil=""true""/>
                <ns:onomasia xsi:nil=""true""/>
                <ns:legalStatusDescr xsi:nil=""true""/>
                <ns:registDate xsi:nil=""true""/>
                <ns:deactivationFlag xsi:nil=""true""/>
                <ns:deactivationFlagDescr xsi:nil=""true""/>
                <ns:postalAddress xsi:nil=""true""/>
                <ns:firmFlagDescr xsi:nil=""true""/>
                <ns:commerTitle xsi:nil=""true""/>
                <ns:postalAreaDescription xsi:nil=""true""/>
                <ns:INiFlagDescr xsi:nil=""true""/>
                <ns:postalZipCode xsi:nil=""true""/>
                </RgWsPublicBasicRt_out>
                <arrayOfRgWsPublicFirmActRt_out xsi:type=""ns:RgWsPublicFirmActRtUserArray""/>
                <pCallSeqId_out xsi:type=""xsd:decimal"">0</pCallSeqId_out>
                <pErrorRec_out xsi:type=""ns:GenWsErrorRtUser"">
                <ns:errorDescr xsi:nil=""true""/>
                <ns:errorCode xsi:nil=""true""/>
                </pErrorRec_out>
                </ns:rgWsPublicAfmMethod>
                </env:Body>
                </env:Envelope>";

            xml = xml.Replace("$username$", username).Replace("$password$", password)
                .Replace("$afmCalledBy$", afmCalledBy).Replace("$afmCalledFor$", afmCalledFor);

            soapEnvelopeXml.LoadXml(xml);

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    Console.WriteLine(soapResult);
                    return FromXMLString(soapResult);
                }
            }




        }
        public static AadeCompanyDto FromXMLString(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            XNamespace env = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace m = "http://gr/gsis/rgwspublic/RgWsPublic.wsdl";
            AadeCompanyDto res = new AadeCompanyDto();
            XElement output = doc.Element(env + "Envelope").Element(env + "Body").Element(m + "rgWsPublicAfmMethodResponse").Element("RgWsPublicBasicRt_out");
            res.AFM = ParseElement(output.Element(m + "afm"));
            res.StopDate = ParseElement(output.Element(m + "stopDate"));
            res.PostalAddressNo = ParseElement(output.Element(m + "postalAddressNo"));
            res.DoyDescr = ParseElement(output.Element(m + "doyDescr"));
            res.Doy = ParseElement(output.Element(m + "doy"));
            res.Onomasia = ParseElement(output.Element(m + "onomasia"));
            res.LegalStatusDescr = ParseElement(output.Element(m + "legalStatusDescr"));
            res.RegistDate = ParseElement(output.Element(m + "registDate"));
            res.DeactivationFlag = ParseElement(output.Element(m + "deactivationFlag"));
            res.DeactivationFlagDescr = ParseElement(output.Element(m + "deactivationFlagDescr"));
            res.PostalAddress = ParseElement(output.Element(m + "postalAddress"));
            res.FirmFlagDescr = ParseElement(output.Element(m + "firmFlagDescr"));
            res.CommerTitle = ParseElement(output.Element(m + "commerTitle"));
            res.PostalAreaDescription = ParseElement(output.Element(m + "postalAreaDescription"));
            res.INiFlagDescr = ParseElement(output.Element(m + "INiFlagDescr"));
            res.PostalZipCode = ParseElement(output.Element(m + "postalZipCode"));

            XElement output2 = doc.Element(env + "Envelope").Element(env + "Body").Element(m + "rgWsPublicAfmMethodResponse").Element("arrayOfRgWsPublicFirmActRt_out").Element(m + "RgWsPublicFirmActRtUser");
            res.FirmActCode = ParseElement(output2?.Element(m + "firmActCode"));
            res.FirmActDescr = ParseElement(output2?.Element(m + "firmActDescr"));
            res.FirmActKind = ParseElement(output2?.Element(m + "firmActKind"));
            res.FirmActKindDescr = ParseElement(output2?.Element(m + "firmActKindDescr"));

            XElement outputError = doc.Element(env + "Envelope").Element(env + "Body").Element(m + "rgWsPublicAfmMethodResponse").Element("pErrorRec_out");
            res.ErrorCode = ParseElement(outputError.Element(m + "errorCode"));
            res.ErrorDescr = ParseElement(outputError.Element(m + "errorDescr"));

            return res;
        }

        private static string ParseElement(XElement element)
        {
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            if (element?.Attribute(xsi + "nil")?.Value == "true")
                return null;
            else
                return element?.Value;
        }
        public static HttpWebRequest CreateWebRequest()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"https://www1.gsis.gr/webtax2/wsgsis/RgWsPublic/RgWsPublicPort");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

      
    }

}

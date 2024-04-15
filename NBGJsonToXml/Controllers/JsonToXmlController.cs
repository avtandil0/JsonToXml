using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace NBGJsonToXml.Controllers
{
    [Produces("application/xml")]
    [ApiController]
    [Route("[controller]")]
    public class JsonToXmlController : ControllerBase
    {
        public class Currency
        {
            public string code { get; set; }
            public int quantity { get; set; }
            public string rateFormated { get; set; }
            public string diffFormated { get; set; }
            public double rate { get; set; }
            public string name { get; set; }
            public double diff { get; set; }
            public string date { get; set; }
            public string validFromDate { get; set; }
        }

        public class NBGService
        {
            public string date { get; set; }
            public List<Currency> currencies { get; set; }

        }
        // GET
        [HttpGet]
        public async Task<XmlDocument>  Get()
        {
            var data = await GetCurrencyAsync();
            XmlDocument doc = new XmlDocument( );

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration( "1.0", "UTF-8", null );
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore( xmlDeclaration, root );

            //(2) string.Empty makes cleaner code
            XmlElement element1 = doc.CreateElement("JsonToXml");
            doc.AppendChild(element1);
            
            XmlElement element2 = doc.CreateElement( string.Empty, "Cube", string.Empty );
            element1.AppendChild( element2 );

            XmlElement element3 = doc.CreateElement( string.Empty, "Cube", string.Empty );
            element3.SetAttribute("time", data[0].date.Substring(0,10));
            
            foreach (var item in data[0].currencies)
            {
                XmlElement CubeChild = doc.CreateElement( string.Empty, "Cube", string.Empty );
                CubeChild.SetAttribute("currency", item.code);
                CubeChild.SetAttribute("rate", item.rateFormated);
                element3.AppendChild( CubeChild );
            }
            element2.AppendChild( element3 );

           
            return doc;

        }
        
        static async Task<List<NBGService>> GetCurrencyAsync()
        {
            using var client = new HttpClient();

            client.BaseAddress = new Uri("https://nbg.gov.ge/gw/");
            client.DefaultRequestHeaders.Add("User-Agent", "C# console program");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var url = "api/ct/monetarypolicy/currencies/ka/json";
            List<NBGService> currencies;
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var resp = await response.Content.ReadAsStringAsync();
                var newResp = "{ 'result': " + resp + "}";
                XmlDocument doc = JsonConvert.DeserializeXmlNode(newResp);
                // return doc;
                currencies = JsonConvert.DeserializeObject<List<NBGService>>(resp);

            }
            catch (Exception ex)
            {
                throw;
            }


            return currencies;

        }
    }
}
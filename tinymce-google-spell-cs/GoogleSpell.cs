using System;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace WolfeReiter.Web.Utility
{
/*
 * http post to http://www.google.com/tbproxy/spell?lang=en&hl=en
 * 
 * Google spellcheck API request looks like this.
 * 
 * <?xml version="1.0" encoding="utf-8" ?> 
 * <spellrequest textalreadyclipped="0" ignoredups="0" ignoredigits="1" ignoreallcaps="1">
 * <text>Ths is a tst</text>
 * </spellrequest>
 * 
 * The response look like ...
 * 
 * <?xml version="1.0" encoding="UTF-8"?>
 * <spellresult error="0" clipped="0" charschecked="12">
 * <c o="0" l="3" s="1">This Th's Thus Th HS</c>
 * <c o="9" l="3" s="1">test tat ST St st</c>
 * </spellresult>
 */

    public class GoogleSpell
    {
        const string GOOGLE_REQUEST_TEMPLATE = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><spellrequest textalreadyclipped=\"0\" ignoredups=\"0\" ignoredigits=\"1\" ignoreallcaps=\"1\"><text>{0}</text></spellrequest>";

        public async Task<IEnumerable<string>> SpellcheckAsync(string lang, IEnumerable<string> wordList)
        {
            //convert list of words to space-delimited string.
            var words = string.Join(" ", wordList);
            var result = (await QueryGoogleAsync(lang, words));

            var doc = new XmlDocument();
            doc.LoadXml(result);

            // Build misspelled word list
            var misspelledWords = new List<string>();
            foreach (var node in doc.SelectNodes("//c"))
            {
                var cElm = (XmlElement)node;
                //google sends back bad word positions to slice out of original data we sent.
                try
                {
                    var badword = words.Substring(Convert.ToInt32(cElm.GetAttribute("o")), Convert.ToInt32(cElm.GetAttribute("l")));
                    misspelledWords.Add(badword);
                }
                catch( ArgumentOutOfRangeException e)
                {
                    Trace.WriteLine(e);
                    Debug.WriteLine(e);
                }
            }
            return misspelledWords;
        }

        public async Task<IEnumerable<string>> SuggestionsAsync(string lang, string word)
        {
            var result = (await QueryGoogleAsync(lang, word));

            // Parse XML result
            var doc = new XmlDocument();
            doc.LoadXml(result);

            // Build misspelled word list
            var suggestions = new List<string>();
            foreach (XmlNode node in doc.SelectNodes("//c"))
            {
                var element = (XmlElement)node;
                if(!string.IsNullOrWhiteSpace(element.InnerText))
                {
                    foreach (var suggestion in element.InnerText.Split('\t'))
                    {
                        if (!string.IsNullOrEmpty(suggestion)) { suggestions.Add(suggestion); }
                    }
                }
            }

            return suggestions;
        }

        async Task<string> QueryGoogleAsync(string lang, string data)
        {
            var scheme     = "https";
            var server     = "www.google.com";
            var port       = 443;
            var path       = "/tbproxy/spell";
            var query      = string.Format("?lang={0}&hl={1}", lang, data);
            var uriBuilder = new UriBuilder(scheme, server, port, path, query);
            string xml     = string.Format(GOOGLE_REQUEST_TEMPLATE, EncodeUnicodeToASCII(data));

            var request           = WebRequest.CreateHttp(uriBuilder.Uri);
            request.Method        = "POST";
            request.KeepAlive     = false;
            request.ContentType   = "application/PTI26";
            request.ContentLength = xml.Length;

            // Google-specific headers
            var headers = request.Headers;
            headers.Add("MIME-Version: 1.0");
            headers.Add("Request-number: 1");
            headers.Add("Document-type: Request");
            headers.Add("Interface-Version: Test 1.4");

            using (var requestStream = (await request.GetRequestStreamAsync()))
            {
                var xmlData = Encoding.ASCII.GetBytes(xml);
                requestStream.Write(xmlData, 0, xmlData.Length);

                var response = (await request.GetResponseAsync());
                using (var responseStream = new StreamReader(response.GetResponseStream()))
                {
                    return responseStream.ReadToEnd();
                }
            }
        }

        string EncodeUnicodeToASCII(string s)
        {
            var builder = new StringBuilder();
            foreach(var c in s.ToCharArray())
            {
                //encode Unicode characters that can't be represented as ASCII
                if (c > 127) { builder.AppendFormat( "&#{0};", (int)c); }
                else { builder.Append(c); }
            }
            return builder.ToString();
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WolfeReiter.Web.Utility;

namespace MvcProject.Controllers
{
    public class TinyMCESpellcheckGatewayController : AsyncController
    {
        [HttpPost]
        public async Task<JsonResult> Index(SpellcheckRequest model)
        {
            var spellService = new GoogleSpell();
            IEnumerable<string> result = null;
            if(string.Equals(model.method, "getSuggestions", StringComparison.InvariantCultureIgnoreCase))
            {
                result = (await spellService.SuggestionsAsync(model.@params.First().Single(), model.@params.Skip(1).First().Single()));
            }
            else //assume checkWords
            {
                result = (await spellService.SpellcheckAsync(model.@params.First().Single(), model.@params.Skip(1).First()));
            }
            string error = null;
            return Json( new { result, id = model.id, error } );
        }

        //class models JSON posted by TinyMCE allows MVC Model Binding to "just work"
        public class SpellcheckRequest
        {
            public SpellcheckRequest()
            {
                @params = new List<IEnumerable<string>>();
            }
            public string method { get; set; }
            public string id { get; set; }
            public IEnumerable<IEnumerable<string>> @params { get; set; }
        }
    }
}
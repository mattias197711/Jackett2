using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Jackett2.Core;
using Conditions;
using Microsoft.Extensions.Logging;

namespace Jackett.Irc.Models.AutoDL.Parser
{
    public class Regex : IParserCommand
    {
        System.Text.RegularExpressions.Regex regex;
        ILogger logger;

        public Regex(XElement x, ILogger l)
        {
            logger = l;
            var value = x.AttributeString("value");
            Condition.Requires(value).IsNotNull();
            regex = new System.Text.RegularExpressions.Regex(value);
        }

        public bool Execute(ParserState state)
        {
            var matches = regex.Matches(state.CurrentItem);

            logger.LogDebug($"{state.Tracker} Regex matches: {matches.Count}");
            if (matches.Count == 0)
            {
                return false;
            }

            state.TempVariables = new List<string>();

            foreach(var match in matches.Cast<Match>().Where(m => m.Success))
            {
                foreach(var group in match.Groups.Cast<Group>().Skip(1))
                {
                    state.TempVariables.Add(group.Value);
                }
            }

            return true;
        }
    }
}

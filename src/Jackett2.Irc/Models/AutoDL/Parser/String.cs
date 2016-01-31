using Jackett2.Core;
using Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace Jackett.Irc.Models.AutoDL.Parser
{
    class String : BaseParserCommand, IParserCommand
    {
        string name;
        string value;
        ILogger logger;

        public String(XElement x, ILogger l)
        {
            logger = l;
            name = x.AttributeString("name");
            value = x.AttributeString("value");
            //Condition.Requires(name).IsNotNullOrWhiteSpace();
        }

        public bool Execute(ParserState state)
        {
            if (string.IsNullOrEmpty(name))
            {
                state.CurrentValue = value;
                logger.LogDebug($"{state.Tracker} String returning value {value}.");
            }
            else
            {
                state.CurrentValue = state.Variables[name];
                logger.LogDebug($"{state.Tracker} String returning {state.CurrentValue}.");
            }
            return true;
        }
    }
}

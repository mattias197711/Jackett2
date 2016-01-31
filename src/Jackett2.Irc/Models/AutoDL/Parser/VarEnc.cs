using Jackett2.Core;
using Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Jackett.Irc.Models.AutoDL.Parser
{
    class VarEnc : BaseParserCommand, IParserCommand
    {
        string name;
        ILogger logger;

        public VarEnc(XElement x, ILogger l)
        {
            logger = l;
            name = x.AttributeString("name");
            Condition.Requires(name).IsNotNullOrWhiteSpace();
        }

        public bool Execute(ParserState state)
        {
            state.CurrentValue = WebUtility.UrlEncode(state.Variables[name]);
            logger.LogDebug($"{state.Tracker} VarEnc returning {state.CurrentValue}.");
            return true;
        }
    }
}

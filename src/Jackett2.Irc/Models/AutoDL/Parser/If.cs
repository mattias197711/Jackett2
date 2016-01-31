using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Jackett2.Core;
using Conditions;
using Microsoft.Extensions.Logging;

namespace Jackett.Irc.Models.AutoDL.Parser
{
    public class If : BaseParserCommand, IParserCommand
    {
        string srcVar;
        System.Text.RegularExpressions.Regex regex;
        ILogger logger;

        public If(XElement x, ILogger l)
        {
            logger = l;
            srcVar = x.AttributeString("srcvar");
            var regexStr = x.AttributeString("regex");
            Condition.Requires(srcVar).IsNotNullOrWhiteSpace();
            Condition.Requires(regexStr).IsNotNullOrWhiteSpace();
            regex = new System.Text.RegularExpressions.Regex(regexStr);
        }

        public bool Execute(ParserState state)
        {
            if (regex.Match(state.Variables[srcVar]).Success)
            {
                foreach(var subAction in this.Children)
                {
                    if (!subAction.Execute(state))
                        return false;
                }

                return true;
            }
            else
            {
                return true;
            }
        }
    }
}

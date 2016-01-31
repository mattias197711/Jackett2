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
    public class SetRegex : BaseParserCommand, IParserCommand
    {
        string srcvar;
        string varname;
        string newvalue;
        System.Text.RegularExpressions.Regex regex;
        ILogger logger;

        public SetRegex(XElement x, ILogger l)
        {
            logger = l;
            srcvar = x.AttributeString("srcvar");
            regex = new System.Text.RegularExpressions.Regex(x.AttributeString("regex"));
            varname = x.AttributeString("varName");
            newvalue = x.AttributeString("newValue");
            Condition.Requires(srcvar).IsNotNullOrWhiteSpace();
            Condition.Requires(regex).IsNotNull();
            Condition.Requires(varname).IsNotNullOrWhiteSpace();
            Condition.Requires(newvalue).IsNotNull();
        }

        public bool Execute(ParserState state)
        {
            var item = state.Variables[srcvar];
            if (item == null)
            {
                logger.LogDebug($"{state.Tracker} SetRegex failed setting null var: {srcvar}");
                return false;
            } else
            {
                state.Variables[srcvar] = regex.Replace(state.Variables[srcvar], newvalue);
                return true;
            }
        }
    }
}

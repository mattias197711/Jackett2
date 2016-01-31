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
    public class ExtractOne : BaseParserCommand, IParserCommand
    {
        ILogger logger;

        public ExtractOne(XElement x, ILogger l)
        {
            logger = l;
        }

        public bool Execute(ParserState state)
        {
            logger.LogDebug($"{state.Tracker} ExtractOne has {base.Children.Count} child actions.");
            foreach(var item in base.Children)
            {
               if(item.Execute(state))
                    return true;
            }
            return true;
        }
    }
}

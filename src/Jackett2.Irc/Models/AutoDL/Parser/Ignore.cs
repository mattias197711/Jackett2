using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Jackett.Irc.Models.AutoDL.Parser
{
    public class Ignore : BaseParserCommand, IParserCommand
    {
        ILogger logger;

        public Ignore(ILogger l)
        {
            logger = l;
        }

        public bool Execute(ParserState state)
        {
           foreach(var action in base.Children)
            {
                if (action.Execute(state))
                    return false;
            }

            return true;
        }
    }
}

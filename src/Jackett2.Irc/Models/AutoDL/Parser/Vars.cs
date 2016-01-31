using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Jackett.Irc.Models.AutoDL.Parser
{
    public class Vars : BaseParserCommand, IParserCommand
    {
        ILogger logger;

        public Vars(XElement x, ILogger l)
        {
            logger = l;
        }

        public bool Execute(ParserState state)
        {
            logger.LogDebug($"{state.Tracker} Vars");
            for (int i = 0; i < state.TempVariables.Count; i++)
            {
                if (i >= base.Children.Count)
                {
                    logger.LogDebug($"{state.Tracker} Vars more variables than children!");
                }
                else
                {
                    var originalItem = state.CurrentItem;
                    state.CurrentValue = state.TempVariables[i];
                    base.Children[i].Execute(state);
                }
            }

            return true;
        }
    }
}

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
    public class Http : BaseParserCommand, IParserCommand
    {
        string name;
        ILogger logger;

        public Http(XElement x, ILogger l)
        {
            logger = l;
            name = x.AttributeString("name");
            Condition.Requires(name).IsNotNullOrWhiteSpace();
        }

        public bool Execute(ParserState state)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < base.Children.Count; i++)
            {
                var action = base.Children[i];
                var subState = state.Clone();
                subState.CurrentItem = string.Empty;
                if (!action.Execute(subState))
                {
                    logger.LogDebug($"{state.Tracker} Http sub {i} action failed.");
                    return false;
                }
                else
                {
                    builder.Append(state.CurrentItem);
                }
            }

            var value = builder.ToString();
            logger.LogDebug($"{state.Tracker} Http setting {name} to {value}.");
            state.HTTPHeaders[name] = value;
            return true;
        }
    }
}

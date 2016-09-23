using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace EpisodeScraper
{
    class Options
    {
        [Option('h', "hours", DefaultValue = 48, HelpText = "Hours ahead to fetch details for.")]
        public int HoursAheadToGetDetail { get; set; }

        [Option('i', "input", Required = true, HelpText = "Input file path.")]
        public string InputFilePath { get; set; }

        [Option('o', "output", DefaultValue = "updated.xml", HelpText = "Output file path.")]
        public string OutputFilePath { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
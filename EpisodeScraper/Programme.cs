using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpisodeScraper
{
    public class Programme
    {
        public string Title { get; set; }
        public string ProgramId { get; set; }
        public DateTime StartDate { get; set; }

        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }

        public string EpisodeInfoInXmltvnsFormat
        {
            get { return (Season - 1) + "." + (Episode - 1) + "."; }
        }

        public override string ToString()
        {
            return Season + "." + Episode + " - " + Title + ": " + Description;
        }
    }
}

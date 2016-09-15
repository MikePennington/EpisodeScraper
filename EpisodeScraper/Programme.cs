using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpisodeScraper
{
    [Serializable]
    public class Programme
    {
        public string Title { get; set; }
        public string ProgramId { get; set; }
        public DateTime StartDate { get; set; }

        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string EpisodeNumber { get; set; }

        public override string ToString()
        {
            return EpisodeNumber + " - " + Title + ": " + Description;
        }
    }
}

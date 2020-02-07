using System;
using System.Collections.Generic;
using OurUmbraco.Wiki.Models;

namespace OurUmbraco.Our.Examine
{
    /// <summary>
    /// Used to calculate popularity
    /// </summary>
    /// <remarks>
    /// This is a struct because it's a tiny object that we don't want hanging around in memory and is created for every project.
    /// </remarks>
    public struct ProjectPopularityPoints
    {
        public ProjectPopularityPoints(
            DateTime now,
            MonthlyProjectDownloads projectDownloads, 
            DateTime createDate, DateTime updateDate, bool worksOnCloud, bool hasForum, bool hasSourceCodeLink, bool openForCollab, int downloads, int votes, int? dailyNugetDownLoads)
        {
            _now = now;
            _projectDownloads = projectDownloads;
            _createDate = createDate;
            _updateDate = updateDate;
            _worksOnCloud = worksOnCloud;
            _hasForum = hasForum;
            _hasSourceCodeLink = hasSourceCodeLink;
            _openForCollab = openForCollab;
            _downloads = downloads;
            _votes = votes;
            _dailyNugetDownLoads = dailyNugetDownLoads;
        }

        private readonly DateTime _now;
        private readonly MonthlyProjectDownloads _projectDownloads;
        private readonly DateTime _createDate;
        private readonly DateTime _updateDate;
        private readonly bool _worksOnCloud;
        private readonly bool _hasForum;
        private readonly bool _hasSourceCodeLink;
        private readonly bool _openForCollab;
        private readonly int _downloads;
        private readonly int _votes;
        private readonly int? _dailyNugetDownLoads;

        public DateTime CreateDate
        {
            get { return _createDate; }
        }

        public DateTime UpdateDate
        {
            get { return _updateDate; }
        }

        public bool WorksOnCloud
        {
            get { return _worksOnCloud; }
        }

        public bool HasForum
        {
            get { return _hasForum; }
        }

        public bool HasSourceCodeLink
        {
            get { return _hasSourceCodeLink; }
        }

        public bool OpenForCollab
        {
            get { return _openForCollab; }
        }

        public int Downloads
        {
            get { return _downloads; }
        }

        public int Votes
        {
            get { return _votes; }
        }

        public int? DailyNugetDownloads
        {
            get { return _dailyNugetDownLoads; }
        }

        private int GetDownloadScore()
        {
            if (_projectDownloads == null) return 0;

            var score = 0;

            var downloadsLast6Months = _projectDownloads.GetLatestDownloads(_now, 6);                   
            score += downloadsLast6Months;

            //get the previous 6 month downloads
            var downloadsLast12Months = _projectDownloads.GetLatestDownloads(_now, 12) - downloadsLast6Months;
            score += downloadsLast12Months;

            // add the nuget downloads for the last 6 months
            var nugetDownloads = 0;

            if (_dailyNugetDownLoads.HasValue)
            {
                nugetDownloads = (_dailyNugetDownLoads.Value * (int)((_now - _now.AddDays(-6)).TotalDays));               
            }

            score += nugetDownloads;

            return score;
        }

        private int GetUpdateDateScore()
        {
            //sort of an exponential calculation on recent update date
            var days = (_now - UpdateDate).Days;

            if (days <= 30) return 5;
            if (days <= 60) return 4;
            if (days <= 100) return 3;
            if (days <= 200) return 2;
            if (days <= 355) return 1;
            return 0;
        }

        public int Calculate()
        {
            var downloadScore = GetDownloadScore();
            var updateScore = GetUpdateDateScore();

            //Each factor is rated (on various scales), then we can boost each factor accordingly
            //the boost factor is the first value
            var ranking = new List<KeyValuePair<int, int>>
            {
                // - download count in a recent timeframe - since old downloads should count for less
                new KeyValuePair<int, int>(1, downloadScore),
                // - votes
                new KeyValuePair<int, int>(20, Votes),
                // - recently updated            
                new KeyValuePair<int, int>(100, updateScore),
                // - works on Cloud
                new KeyValuePair<int, int>(250, WorksOnCloud ? 1 : 0),
                // - has a forum
                new KeyValuePair<int, int>(100, HasForum ? 1 : 0),
                // - has source code link
                new KeyValuePair<int, int>(500, HasSourceCodeLink ? 1 : 0),
                // - open for collab / has collaborators
                new KeyValuePair<int, int>(250, OpenForCollab ? 1 : 0),
            };

            //TODO:
            // - works on latest umbraco versions            

            var pop = 0;
            foreach (var val in ranking)
            {
                pop += val.Key * val.Value;
            }
            return pop;
        }
    }
}
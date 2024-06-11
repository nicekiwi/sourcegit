﻿using System.Threading.Tasks;

namespace SourceGit.ViewModels
{
    public class DeleteBranch : Popup
    {
        public Models.Branch Target
        {
            get;
            private set;
        }

        public Models.Branch TrackingRemoteBranch
        {
            get;
            private set;
        }

        public object DeleteTrackingRemoteTip
        {
            get;
            private set;
        }

        public bool AlsoDeleteTrackingRemote
        {
            get => _alsoDeleteTrackingRemote;
            set => SetProperty(ref _alsoDeleteTrackingRemote, value);
        }

        public DeleteBranch(Repository repo, Models.Branch branch)
        {
            _repo = repo;
            Target = branch;

            if (branch.IsLocal && !string.IsNullOrEmpty(branch.Upstream))
            {
                var upstream = branch.Upstream.Substring(13);
                TrackingRemoteBranch = repo.Branches.Find(x => !x.IsLocal && $"{x.Remote}/{x.Name}" == upstream);
                DeleteTrackingRemoteTip = new Views.NameHighlightedTextBlock("DeleteBranch.WithTrackingRemote", upstream);
            }

            View = new Views.DeleteBranch() { DataContext = this };
        }

        public override Task<bool> Sure()
        {
            _repo.SetWatcherEnabled(false);
            ProgressDescription = "Deleting branch...";

            return Task.Run(() =>
            {
                if (Target.IsLocal)
                {
                    Commands.Branch.DeleteLocal(_repo.FullPath, Target.Name);

                    if (_alsoDeleteTrackingRemote && TrackingRemoteBranch != null)
                    {
                        SetProgressDescription("Deleting tracking remote branch...");
                        Commands.Branch.DeleteRemote(_repo.FullPath, TrackingRemoteBranch.Remote, TrackingRemoteBranch.Name);
                    }
                }
                else
                {
                    Commands.Branch.DeleteRemote(_repo.FullPath, Target.Remote, Target.Name);
                }

                CallUIThread(() => _repo.SetWatcherEnabled(true));
                return true;
            });
        }

        private readonly Repository _repo = null;
        private bool _alsoDeleteTrackingRemote = false;
    }
}

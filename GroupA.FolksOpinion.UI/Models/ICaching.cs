using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupA.FolksOpinion.UI.Models
{
    interface ICaching
    {
        private void PullFromCache();
        private void PushToCache();
    }
}

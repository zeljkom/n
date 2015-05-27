using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace MyBackgroundService
{
    public sealed class MyBackTask: IBackgroundTask {



        public void Run(IBackgroundTaskInstance taskInstance) {

            var defferal = taskInstance.GetDeferral();

            taskInstance.Canceled += (s, e) => { };
            taskInstance.Progress = 0;

            defferal.Complete();
        }
    }
}

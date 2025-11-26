using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.ApplicationServices;

namespace CourseProject
{
    class App : Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase
    {
        public App()
        {
            // Сделать это однопользовательским приложением
            this.IsSingleInstance = true;
            this.EnableVisualStyles = true;

            // Есть другие доступные возможности в 
            // VB application model, например
            // стиль завершения работы:
            this.ShutdownStyle =
              Microsoft.VisualBasic.ApplicationServices.ShutdownMode.AfterMainFormCloses;

            // Добавить обработчик StartupNextInstance
            /*this.StartupNextInstance +=
              new StartupNextInstanceEventHandler(this.SIApp_StartupNextInstance);*/
        }
    }
}
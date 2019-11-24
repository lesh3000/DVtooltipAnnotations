using System.Web;
using System.Web.Routing;
using GdPicture14.WEB;
using System.Reflection;
using GdPicture14;
using GdPicture14.Annotations;
using System.IO;
using System.Web.Mvc;

namespace aspnet_mvc_razor_app
{

    public class MvcApplication : HttpApplication
    {

        public static readonly int SESSION_TIMEOUT = 20; //Set to 20 minutes. use -1 to handle DocuVieware session timeout through asp.net session mechanism.
        private const bool STICKY_SESSION = true; //Set false to use DocuVieware on Servers Farm witn non sticky sessions.
       
        private const DocuViewareSessionStateMode DOCUVIEWARE_SESSION_STATE_MODE = DocuViewareSessionStateMode.InProc; //Set DocuViewareSessionStateMode.File is STICKY_SESSION is False.

        public static string GetCacheDirectory()
        {
            return HttpRuntime.AppDomainAppPath + "\\cache";
        }


        public static string GetDocumentsDirectory()
        {
            return HttpRuntime.AppDomainAppPath + "\\documents";
        }

        protected void Application_Start()
        {
            try
            {
                Assembly.Load("GdPicture.NET.14.WEB.DocuVieware");
            }
            catch (System.IO.FileNotFoundException)
            {
                throw new System.IO.FileNotFoundException(" The system cannot find the DocuVieware assembly. Please set the Copy Local Property of the GdPicture.NET.14.WEB.DocuVieware reference to true. More information: https://msdn.microsoft.com/en-us/library/t1zz5y8c(v=vs.100).aspx");
            }
            DocuViewareEventsHandler.NewDocumentLoaded += NewDocumentLoadedHandler;
            DocuViewareManager.SetupConfiguration(STICKY_SESSION, DOCUVIEWARE_SESSION_STATE_MODE, GetCacheDirectory());
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            DocuViewareLicensing.RegisterKEY("0437367244592426255451672"); //Unlocking DocuVieware. Please set your demo or commercial license key here.

            //Subscribe to delegate
            DocuViewareEventsHandler.CustomAction += CustomActionDispatcher;
        }
        

     


        private static void CustomActionDispatcher(object sender, CustomActionEventArgs e)
        {
            GdPicturePDF oPDF = new GdPicturePDF();
            AnnotationManager oAnnotationsManager = new AnnotationManager();
            Annotation oAnnotation ;
           
            
            switch (e.actionName)
            {

                case "tooltip":

                    e.docuVieware.GetNativePDF(out oPDF);
                    oAnnotationsManager.InitFromGdPicturePDF(oPDF);
                    for (int y = 1;   y <= oPDF.GetPageCount(); y++)
                    {
                        oAnnotationsManager.SelectPage(y);
                        oPDF.SetOrigin(PdfOrigin.PdfOriginTopLeft);
                        oPDF.SetMeasurementUnit(PdfMeasurementUnit.PdfMeasurementUnitInch);
                        int AnnotationCount = oAnnotationsManager.GetAnnotationCount();
                        if (AnnotationCount > 0)
                        {
                            for (int c = 0; c < AnnotationCount; c++)
                            {

                                oAnnotation = oAnnotationsManager.GetAnnotationFromIdx(c);
                                if (oAnnotation.Guid == e.args.ToString())
                                {
                                    e.result = oAnnotation;
                                }
                            }
                        }
                    }

                    break;

               

            }

        }
        private static void NewDocumentLoadedHandler(object sender, NewDocumentLoadedEventArgs e)
        {
            e.docuVieware.PagePreload = e.docuVieware.PageCount <= 50 ? PagePreloadMode.AllPages : PagePreloadMode.AdjacentPages;
        }
    }
}